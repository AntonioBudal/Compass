using Compass.Modules.Calendar.Infrastructure.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Modules.Calendar.Infrastructure.Database.Configurations;

internal sealed class ScheduleProfileDataConfiguration
    : IEntityTypeConfiguration<ScheduleProfileData>
{
    public void Configure(
        EntityTypeBuilder<ScheduleProfileData> builder)
    {
        builder.ToTable("ScheduleProfiles");
        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.Timezone)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasMany(profile => profile.Windows)
            .WithOne(window => window.Profile)
            .HasForeignKey(window => window.ScheduleProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ScheduleWindowDataConfiguration
    : IEntityTypeConfiguration<ScheduleWindowData>
{
    public void Configure(
        EntityTypeBuilder<ScheduleWindowData> builder)
    {
        builder.ToTable("ScheduleWindows");
        builder.HasKey(window => window.Id);

        builder.Property(window => window.DayOfWeek)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(window => window.StartTime)
            .HasColumnType("time without time zone")
            .IsRequired();

        builder.Property(window => window.EndTime)
            .HasColumnType("time without time zone")
            .IsRequired();
    }
}

