using Microsoft.EntityFrameworkCore;
using TaskModel = Compass.Modules.Planning.Domain.Model.Task;

namespace Compass.Modules.Planning.Infrastructure.Persistence;

public class PlanningDbContext : DbContext
{
    public const string SchemaName = "planning";

    public DbSet<TaskModel> Tasks => Set<TaskModel>();

    public PlanningDbContext(DbContextOptions<PlanningDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlanningDbContext).Assembly);
    }
}
