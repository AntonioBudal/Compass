using Compass.Modules.Calendar.Infrastructure.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Modules.Calendar.Infrastructure.Database.Configurations;

internal sealed class ScheduleExceptionDataConfiguration : IEntityTypeConfiguration<ScheduleExceptionData>
{
    public void Configure(EntityTypeBuilder<ScheduleExceptionData> builder)
    {
        builder.ToTable("ScheduleExceptions");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Date).HasColumnType("date").IsRequired();
        builder.Property(e => e.StartTime).HasColumnType("time without time zone").IsRequired();
        builder.Property(e => e.EndTime).HasColumnType("time without time zone").IsRequired();
        builder.Property(e => e.Reason).HasMaxLength(500).IsRequired();

        builder.HasOne(e => e.Profile)
            .WithMany()
            .HasForeignKey(e => e.ScheduleProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
