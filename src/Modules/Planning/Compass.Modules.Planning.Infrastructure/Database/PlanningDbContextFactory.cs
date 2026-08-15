using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Compass.Modules.Planning.Infrastructure.Database;

public class PlanningDbContextFactory : IDesignTimeDbContextFactory<PlanningDbContext>
{
    public PlanningDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<PlanningDbContext>();
        
        // Esta connection string genérica é usada APENAS em design-time para gerar as migrations.
        // Em execução real, a API injetará a connection string correta através da injeção de dependências.
        builder.UseNpgsql("Host=localhost;Database=compass_dev;Username=postgres;Password=postgres");

        return new PlanningDbContext(builder.Options);
    }
}
