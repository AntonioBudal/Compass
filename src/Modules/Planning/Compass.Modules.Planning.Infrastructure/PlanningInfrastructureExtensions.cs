using Compass.Modules.Planning.Domain.Repositories;
using Compass.Modules.Planning.Infrastructure.Persistence;
using Compass.Modules.Planning.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Compass.Modules.Planning.Infrastructure;

public static class PlanningInfrastructureExtensions
{
    public static IServiceCollection AddPlanningInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CompassDb")
            ?? "Host=localhost;Port=5432;Database=compass;Username=postgres;Password=postgres";

        services.AddDbContext<PlanningDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", PlanningDbContext.SchemaName);
            });
        });

        services.AddScoped<ITaskRepository, TaskRepository>();

        return services;
    }
}
