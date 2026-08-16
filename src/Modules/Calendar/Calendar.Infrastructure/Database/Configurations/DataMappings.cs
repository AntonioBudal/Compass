using Compass.Modules.Calendar.Infrastructure.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Modules.Calendar.Infrastructure.Database.Configurations;

internal class ScheduleProfileDataConfiguration : IEntityTypeConfiguration<ScheduleProfileData>
{
    public void Configure(EntityTypeBuilder<ScheduleProfileData> builder)
    {
        builder.ToTable("ScheduleProfiles");
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Timezone).IsRequired().HasMaxLength(100);

        builder.HasMany(p => p.Windows)
               .WithOne(w => w.Profile)
               .HasForeignKey(w => w.ScheduleProfileId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

internal class ScheduleWindowDataConfiguration : IEntityTypeConfiguration<ScheduleWindowData>
{
    public void Configure(EntityTypeBuilder<ScheduleWindowData> builder)
    {
        builder.ToTable("ScheduleWindows");
        builder.HasKey(w => w.Id);
        
        builder.Property(w => w.DayOfWeek).HasConversion<string>().IsRequired();
        builder.Property(w => w.StartTime).HasColumnType("time without time zone").IsRequired();
        builder.Property(w => w.EndTime).HasColumnType("time without time zone").IsRequired();
    }
}
