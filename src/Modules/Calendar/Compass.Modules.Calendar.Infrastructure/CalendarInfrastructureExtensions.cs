using Compass.Modules.Calendar.Domain.Repositories;
using Compass.Modules.Calendar.Infrastructure.Persistence;
using Compass.Modules.Calendar.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Compass.Modules.Calendar.Infrastructure;

public static class CalendarInfrastructureExtensions
{
    public static IServiceCollection AddCalendarInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CompassDb")
            ?? "Host=localhost;Port=5432;Database=compass;Username=postgres;Password=postgres";

        services.AddDbContext<CalendarDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", CalendarDbContext.SchemaName);
            });
        });

        services.AddScoped<IScheduleProfileRepository, ScheduleProfileRepository>();

        return services;
    }
}
