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
        // Verifica se a Factory do xUnit já injetou o provedor InMemory.
        // Se sim, abortamos a injeção do Npgsql para não causar a colisão de provedores.
        if (optionsBuilder.Options.Extensions.Any(e => e.GetType().Name.Contains("InMemory")))
        {
            return;
        }
        
        // Mapeamento nativo dos Enums no Npgsql (Rodará apenas em Produção/Dev)
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