using Compass.Modules.Calendar.Infrastructure.Persistence;
using Compass.Modules.Planning.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Compass.Host.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("compass_api_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var calendarDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<CalendarDbContext>));
            if (calendarDescriptor != null)
            {
                services.Remove(calendarDescriptor);
            }

            services.AddDbContext<CalendarDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            var planningDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PlanningDbContext>));
            if (planningDescriptor != null)
            {
                services.Remove(planningDescriptor);
            }

            services.AddDbContext<PlanningDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            
            var calendarDb = scope.ServiceProvider.GetRequiredService<CalendarDbContext>();
            var calendarCreator = calendarDb.GetService<IRelationalDatabaseCreator>();
            if (!calendarCreator.Exists())
            {
                calendarCreator.Create();
            }
            try
            {
                calendarCreator.CreateTables();
            }
            catch
            {
                // Tables already created
            }

            var planningDb = scope.ServiceProvider.GetRequiredService<PlanningDbContext>();
            var planningCreator = planningDb.GetService<IRelationalDatabaseCreator>();
            try
            {
                planningCreator.CreateTables();
            }
            catch
            {
                // Tables already created
            }
        });
    }
}
