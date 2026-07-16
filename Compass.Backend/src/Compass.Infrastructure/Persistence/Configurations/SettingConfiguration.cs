using Compass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Infrastructure.Persistence.Configurations;

public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("settings");

        builder.HasKey(s => s.UserId);
        builder.Property(s => s.UserId).HasColumnName("user_id");

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<Setting>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.DefaultEnergyLevel)
            .HasColumnName("default_energy_level")
            .IsRequired()
            .HasDefaultValue(2);

        builder.ToTable(t => t.HasCheckConstraint("chk_default_energy_level", "default_energy_level BETWEEN 1 AND 3"));

        builder.Property(s => s.Theme).HasColumnName("theme").HasMaxLength(20).IsRequired().HasDefaultValue("dark");
        builder.Property(s => s.AutoPostponeEnabled).HasColumnName("auto_postpone_enabled").IsRequired().HasDefaultValue(true);
        
        builder.Property(s => s.DailyReviewTime)
            .HasColumnName("daily_review_time")
            .IsRequired()
            .HasDefaultValueSql("'20:00:00'");

        // Mapeamento nativo para coluna JSONB
        builder.Property(s => s.PreferencesJson)
            .HasColumnName("preferences_json")
            .HasColumnType("jsonb")
            .IsRequired()
            .HasDefaultValueSql("'{}'::jsonb");

        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}