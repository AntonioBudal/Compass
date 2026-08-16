using Compass.Modules.Execution.Domain.DailyCycles;
using Microsoft.EntityFrameworkCore;

namespace Compass.Modules.Execution.Infrastructure.Database;

internal class ExecutionDbContext : DbContext
{
    public DbSet<DailyCycle> DailyCycles { get; set; } = null!;

    public ExecutionDbContext(
        DbContextOptions<ExecutionDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("execution");

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ExecutionDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
