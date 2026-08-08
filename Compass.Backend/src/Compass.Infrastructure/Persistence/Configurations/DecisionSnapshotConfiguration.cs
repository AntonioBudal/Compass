using Compass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Infrastructure.Persistence.Configurations;

public class DecisionSnapshotConfiguration : IEntityTypeConfiguration<DecisionSnapshot>
{
    public void Configure(EntityTypeBuilder<DecisionSnapshot> builder)
    {
        builder.ToTable("decision_snapshots");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(d => d.UserId).HasColumnName("user_id").IsRequired();
        builder.HasOne<User>().WithMany().HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(d => d.AvailableWindowMinutes).HasColumnName("available_window_minutes").IsRequired();
        builder.Property(d => d.UserEnergyContext).HasColumnName("user_energy_context").IsRequired();

        // Chaves estrangeiras opcionais para os itens recomendados (com SET NULL se a tarefa for apagada)
        builder.Property(d => d.Top1Id).HasColumnName("top1_id");
        builder.HasOne<Commitment>().WithMany().HasForeignKey(d => d.Top1Id).OnDelete(DeleteBehavior.SetNull);

        builder.Property(d => d.Top2Id).HasColumnName("top2_id");
        builder.HasOne<Commitment>().WithMany().HasForeignKey(d => d.Top2Id).OnDelete(DeleteBehavior.SetNull);

        builder.Property(d => d.Top3Id).HasColumnName("top3_id");
        builder.HasOne<Commitment>().WithMany().HasForeignKey(d => d.Top3Id).OnDelete(DeleteBehavior.SetNull);

        builder.Property(d => d.ChosenCommitmentId).HasColumnName("chosen_commitment_id");
        builder.HasOne<Commitment>().WithMany().HasForeignKey(d => d.ChosenCommitmentId).OnDelete(DeleteBehavior.SetNull);

        builder.Property(d => d.WasIgnored).HasColumnName("was_ignored").IsRequired().HasDefaultValue(true);
        builder.Property(d => d.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Índice analítico para consultas no histórico de decisões do usuário por data recente
        builder.HasIndex(d => new { d.UserId, d.CreatedAt })
            .HasDatabaseName("idx_decision_snapshots_user_recent_analytics")
            .HasFilter("was_bypassed = true OR action_taken IS NOT NULL")
            .IsDescending(false, true); // CreatedAt DESC
    }
}