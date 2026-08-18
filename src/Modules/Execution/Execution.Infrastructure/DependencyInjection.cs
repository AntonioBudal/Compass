using Compass.Modules.Execution.Application.DailyCycles;
using Compass.Modules.Execution.Application.DailyCycles.Queries;
using Compass.Modules.Execution.Infrastructure.Database;
using Compass.Modules.Execution.Infrastructure.Repositories;
using Compass.Modules.Execution.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Compass.Modules.Execution.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddExecutionInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<ExecutionDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IDailyCycleRepository, EfDailyCycleRepository>();
        services.AddScoped<IDailyCycleQueryService, DailyCycleQueryService>();
        services.AddScoped<Compass.Modules.Execution.Application.DailyPlanning.IDailyPlanRepository, Compass.Modules.Execution.Infrastructure.Repositories.EfDailyPlanRepository>();

        return services;
    }

    public static async Task MigrateExecutionDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ExecutionDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}

