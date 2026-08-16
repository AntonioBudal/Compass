using System.Linq;
using Compass.Modules.Planning.Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Compass.Modules.Planning.IntegrationTests.Setup;

public class PlanningApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Sobe um banco de dados real do PostgreSQL no Docker em uma porta aleatória disponível
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("compass_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove o DbContext original apontando para localhost
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PlanningDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Injeta o DbContext apontando dinamicamente para o Testcontainer recém-criado
            services.AddDbContext<PlanningDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });
        });
    }

    public async System.Threading.Tasks.Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Roda as Migrations para montar o Schema planning no container zerado
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PlanningDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public new async System.Threading.Tasks.Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }
}
