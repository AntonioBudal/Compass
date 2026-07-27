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
        
        builder.Property(s => s.DefaultEnergyLevel).HasColumnName("default_energy_level").IsRequired();
        builder.Property(s => s.Theme).HasColumnName("theme").HasMaxLength(32).IsRequired();
        builder.Property(s => s.AutoPostponeEnabled).HasColumnName("auto_postpone_enabled").IsRequired();
        
        // Mapeamento nativo para 'time without time zone' no PostgreSQL
        builder.Property(s => s.DailyReviewTime)
            .HasColumnName("daily_review_time")
            .HasColumnType("time without time zone")
            .IsRequired();
        
        // Mapeamento para coluna JSONB nativa para performance em buscas de preferências
        builder.Property(s => s.PreferencesJson)
            .HasColumnName("preferences_json")
            .HasColumnType("jsonb")
            .IsRequired();
            
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at").IsRequired();
    }
}