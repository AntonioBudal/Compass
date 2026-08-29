using Compass.Modules.Calendar.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Modules.Calendar.Infrastructure.Persistence.Configurations;

public class ScheduleProfileConfiguration : IEntityTypeConfiguration<ScheduleProfile>
{
    public void Configure(EntityTypeBuilder<ScheduleProfile> builder)
    {
        builder.ToTable("schedule_profiles", CalendarDbContext.SchemaName);

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.TimeZone)
            .HasConversion(
                tz => tz.Value,
                str => new TimeZoneId(str)
            )
            .HasColumnName("time_zone_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(p => p.WeeklyAvailability)
            .WithOne()
            .HasForeignKey(r => r.ScheduleProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.WeeklyAvailability)
            .AutoInclude();
    }
}
