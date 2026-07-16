using Compass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Infrastructure.Persistence.Configurations;

public class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
{
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.ToTable("schedules");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.UserId).HasColumnName("user_id").IsRequired();
        builder.HasOne<User>().WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.DayOfWeek).HasColumnName("day_of_week").IsRequired();
        builder.ToTable(t => t.HasCheckConstraint("chk_schedule_day", "day_of_week BETWEEN 0 AND 6"));

        builder.Property(s => s.WorkStart).HasColumnName("work_start").IsRequired();
        builder.Property(s => s.WorkEnd).HasColumnName("work_end").IsRequired();
        builder.ToTable(t => t.HasCheckConstraint("chk_schedule_time_order", "work_end > work_start"));

        builder.Property(s => s.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);
    }
}