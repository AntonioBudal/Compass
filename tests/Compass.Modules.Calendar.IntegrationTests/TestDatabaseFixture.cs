using Compass.Modules.Calendar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Compass.Modules.Calendar.IntegrationTests;

public class TestDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("compass_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public CalendarDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CalendarDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new CalendarDbContext(options);
    }
}
