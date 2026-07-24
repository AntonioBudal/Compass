using Compass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Infrastructure.Persistence.Configurations;

public class FocusSessionConfiguration : IEntityTypeConfiguration<FocusSession>
{
    public void Configure(EntityTypeBuilder<FocusSession> builder)
    {
        builder.ToTable("focus_sessions");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(f => f.UserId).HasColumnName("user_id").IsRequired();
        builder.HasOne<User>().WithMany().HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(f => f.CommitmentId).HasColumnName("commitment_id").IsRequired();
        builder.HasOne<Commitment>().WithMany().HasForeignKey(f => f.CommitmentId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(f => f.StartTime).HasColumnName("start_time").IsRequired();
        builder.Property(f => f.EndTime).HasColumnName("end_time").IsRequired();
        builder.Property(f => f.ActualDurationMinutes).HasColumnName("actual_duration_minutes").IsRequired();
        builder.Property(f => f.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(f => f.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Índice Analítico de Alta Performance: Cobertura de histórico temporal por usuário
        builder.HasIndex(f => new { f.UserId, f.StartTime })
            .HasDatabaseName("idx_focus_sessions_user_time")
            .HasFilter("actual_duration_minutes > 0");
    }
}