using Compass.Modules.Calendar.Application.Profiles;
using Compass.Modules.Calendar.Application.Profiles.Commands;
using Compass.Modules.Calendar.Application.Profiles.Queries;
using Compass.Modules.Calendar.Contracts.Queries;
using Compass.Modules.Calendar.Infrastructure.Commands;
using Compass.Modules.Calendar.Infrastructure.Database;
using Compass.Modules.Calendar.Infrastructure.Queries;
using Compass.Modules.Calendar.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Compass.Modules.Calendar.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCalendarModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(
                "DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not configured.");

        services.AddDbContext<CalendarDbContext>(
            options => options.UseNpgsql(connectionString));

        services.AddScoped<
            IScheduleProfileRepository,
            EfScheduleProfileRepository>();

        services.AddScoped<
            IAvailabilityQueryService,
            AvailabilityQueryService>();

        services.AddScoped<
            IAvailabilityQuery,
            AvailabilityCrossQuery>();

        services.AddScoped<
            IScheduleProfileCommandService,
            ScheduleProfileCommandService>();

services.AddScoped<Compass.Modules.Calendar.Application.Profiles.Commands.IAddScheduleExceptionCommandService, Compass.Modules.Calendar.Infrastructure.Commands.AddScheduleExceptionCommandService>();
        services.AddScoped<Compass.Modules.Calendar.Application.Commitments.ICommitmentRepository, Compass.Modules.Calendar.Infrastructure.Repositories.EfCommitmentRepository>();
        return services;
    }

    public static async Task MigrateCalendarDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<CalendarDbContext>();

        await dbContext.Database
            .MigrateAsync(cancellationToken);
    }
}


