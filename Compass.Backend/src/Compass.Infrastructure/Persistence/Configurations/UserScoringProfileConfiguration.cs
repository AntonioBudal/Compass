using Compass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Infrastructure.Persistence.Configurations;

public class UserScoringProfileConfiguration : IEntityTypeConfiguration<UserScoringProfile>
{
    public void Configure(EntityTypeBuilder<UserScoringProfile> builder)
    {
        builder.ToTable("user_scoring_profiles");

        builder.HasKey(p => p.UserId);
        builder.Property(p => p.UserId).HasColumnName("user_id");

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<UserScoringProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(p => p.EaiMultiplier)
            .HasColumnName("eai_multiplier")
            .IsRequired()
            .HasDefaultValue(1.0);

        builder.Property(p => p.MorningEnergyBias)
            .HasColumnName("morning_energy_bias")
            .IsRequired()
            .HasDefaultValue(1.0);

        builder.Property(p => p.AfternoonEnergyBias)
            .HasColumnName("afternoon_energy_bias")
            .IsRequired()
            .HasDefaultValue(1.0);

        builder.Property(p => p.EveningEnergyBias)
            .HasColumnName("evening_energy_bias")
            .IsRequired()
            .HasDefaultValue(1.0);

        builder.Property(p => p.UrgencyWeightAdjust)
            .HasColumnName("urgency_weight_adjust")
            .IsRequired()
            .HasDefaultValue(0.0);

        builder.Property(p => p.StrategyWeightAdjust)
            .HasColumnName("strategy_weight_adjust")
            .IsRequired()
            .HasDefaultValue(0.0);

        builder.Property(p => p.SampleCount)
            .HasColumnName("sample_count")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Configuração Industrial de Concorrência Otimista (PostgreSQL xmin)
        builder.Property<uint>("Version")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
    }
}