using Compass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Infrastructure.Persistence.Configurations;

public class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
{
    public void Configure(EntityTypeBuilder<Reminder> builder)
    {
        builder.ToTable("reminders");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(r => r.CommitmentId).HasColumnName("commitment_id").IsRequired();
        builder.HasOne<Commitment>().WithMany().HasForeignKey(r => r.CommitmentId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(r => r.TriggerTime).HasColumnName("trigger_time").IsRequired();
        builder.Property(r => r.IsSent).HasColumnName("is_sent").IsRequired().HasDefaultValue(false);
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Índice parcial de alta performance para o job de background do Quartz.NET
        builder.HasIndex(r => r.TriggerTime)
            .HasDatabaseName("idx_reminders_unsent_trigger")
            .HasFilter("is_sent = FALSE");
    }
}