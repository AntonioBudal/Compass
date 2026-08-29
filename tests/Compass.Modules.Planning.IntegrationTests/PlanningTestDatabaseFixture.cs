using Compass.Modules.Planning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Testcontainers.PostgreSql;
using Xunit;

namespace Compass.Modules.Planning.IntegrationTests;

public class PlanningTestDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("compass_planning_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var context = CreateDbContext();
        var creator = context.GetService<IRelationalDatabaseCreator>();
        if (!await creator.ExistsAsync())
        {
            await creator.CreateAsync();
        }
        try
        {
            await creator.CreateTablesAsync();
        }
        catch
        {
            // Already created
        }
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public PlanningDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PlanningDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new PlanningDbContext(options);
    }
}
