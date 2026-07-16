using Compass.Domain.Entities;
using Compass.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Compass.Infrastructure.Persistence;

public class CompassDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Setting> Settings { get; set; } = null!;
    public DbSet<Goal> Goals { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<Commitment> Commitments { get; set; } = null!;
    public DbSet<Dependency> Dependencies { get; set; } = null!;
    public DbSet<Schedule> Schedules { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<Reminder> Reminders { get; set; } = null!;
    public DbSet<FocusSession> FocusSessions { get; set; } = null!;

    public CompassDbContext(DbContextOptions<CompassDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        // Mapeamento nativo dos Enums no Npgsql (PostgreSQL 16+)
        optionsBuilder.UseNpgsql(npgsqlBuilder =>
        {
            npgsqlBuilder.MapEnum<CommitmentType>("commitment_type");
            npgsqlBuilder.MapEnum<CommitmentStatus>("commitment_status");
            npgsqlBuilder.MapEnum<GoalStatus>("goal_status");
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica automaticamente todas as classes de configuração (IEntityTypeConfiguration)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CompassDbContext).Assembly);
    }
}