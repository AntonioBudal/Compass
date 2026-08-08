using Compass.Domain.Entities;
using Compass.Domain.Enums;
using Microsoft.EntityFrameworkCore;

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
    public DbSet<DecisionSnapshot> DecisionSnapshots { get; set; } = null!; 
    public DbSet<UserScoringProfile> UserScoringProfiles { get; set; } = null!;
    public DbSet<DailyReview> DailyReviews { get; set; } = null!;

    public CompassDbContext(DbContextOptions<CompassDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        // 🚨 O DISJUNTOR DOS TESTES 🚨
        // Mantemos apenas a trava de testes em memória. 
        // Removemos o Npgsql para permitir que o Program.cs injete o SQLite com sucesso!
        if (optionsBuilder.Options.Extensions.Any(e => e.GetType().Name.Contains("InMemory")))
        {
            return;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica automaticamente todas as classes de configuração (IEntityTypeConfiguration)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CompassDbContext).Assembly);

        modelBuilder.Entity<DecisionSnapshot>(builder =>
        {
            // Índice Parcial Otimizado: Permite que o job de calibração varra 
            // apenas decisões que geraram aprendizado real (escolhas ou ignorados explicitamente)
            builder.HasIndex(d => new { d.UserId, d.CreatedAt })
                .HasDatabaseName("idx_snapshots_reinforcement")
                .HasFilter("was_ignored = false OR chosen_commitment_id IS NOT NULL");
        });
    }
}