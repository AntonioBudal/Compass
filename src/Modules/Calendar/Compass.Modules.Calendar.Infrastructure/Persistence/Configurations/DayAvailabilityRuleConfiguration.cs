using System.Text.Json;
using Compass.Modules.Calendar.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Modules.Calendar.Infrastructure.Persistence.Configurations;

public class DayAvailabilityRuleConfiguration : IEntityTypeConfiguration<DayAvailabilityRule>
{
    public void Configure(EntityTypeBuilder<DayAvailabilityRule> builder)
    {
        builder.ToTable("day_availability_rules", CalendarDbContext.SchemaName);

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.ScheduleProfileId)
            .HasColumnName("schedule_profile_id")
            .IsRequired();

        builder.Property(r => r.DayOfWeek)
            .HasColumnName("day_of_week")
            .IsRequired();

        var valueComparer = new ValueComparer<IReadOnlyList<TimeWindow>>(
            (c1, c2) => JsonSerializer.Serialize(c1, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(c2, (JsonSerializerOptions?)null),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => JsonSerializer.Deserialize<List<TimeWindow>>(JsonSerializer.Serialize(c, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null)!
        );

        builder.Property(r => r.Windows)
            .HasColumnName("windows")
            .HasColumnType("jsonb")
            .HasConversion(
                w => JsonSerializer.Serialize(w, (JsonSerializerOptions?)null),
                json => JsonSerializer.Deserialize<List<TimeWindow>>(json, (JsonSerializerOptions?)null) ?? new List<TimeWindow>()
            )
            .Metadata.SetValueComparer(valueComparer);
    }
}
