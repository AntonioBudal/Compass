using Compass.Modules.Planning.Domain.Habits;
using Compass.Modules.Planning.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Task = Compass.Modules.Planning.Domain.Tasks.Task;

namespace Compass.Modules.Planning.Infrastructure.Database;

public class PlanningDbContext : DbContext
{
    public DbSet<Task> Tasks { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<Habit> Habits { get; set; } = null!;

    public PlanningDbContext(DbContextOptions<PlanningDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Isola as tabelas do módulo de Planning no seu próprio schema de banco de dados
        modelBuilder.HasDefaultSchema("planning");
        
        // Aplica todas as configurações que criaremos abaixo
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlanningDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
