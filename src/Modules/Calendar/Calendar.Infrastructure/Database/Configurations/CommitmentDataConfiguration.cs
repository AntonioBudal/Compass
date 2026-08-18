using Compass.Modules.Calendar.Infrastructure.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Modules.Calendar.Infrastructure.Database.Configurations;

internal sealed class CommitmentDataConfiguration : IEntityTypeConfiguration<CommitmentData>
{
    public void Configure(EntityTypeBuilder<CommitmentData> builder)
    {
        builder.ToTable("Commitments");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title).HasMaxLength(255).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(1000);
        builder.Property(c => c.StartTime).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(c => c.EndTime).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(c => c.Status).HasMaxLength(50).IsRequired();

        builder.HasOne(c => c.Profile)
            .WithMany()
            .HasForeignKey(c => c.ScheduleProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
