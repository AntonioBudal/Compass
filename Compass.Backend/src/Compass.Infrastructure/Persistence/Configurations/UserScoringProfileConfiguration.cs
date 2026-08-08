using Compass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Infrastructure.Persistence.Configurations;

public class UserScoringProfileConfiguration : IEntityTypeConfiguration<UserScoringProfile>
{
    public void Configure(EntityTypeBuilder<UserScoringProfile> builder)
    {
        builder.ToTable("user_scoring_profiles");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        
        builder.Property(p => p.UserId).HasColumnName("user_id").IsRequired();
        
        builder.Property(p => p.SampleCount).HasColumnName("sample_count").IsRequired();
        builder.Property(p => p.UrgencyWeightAdjust).HasColumnName("urgency_weight_adjust").IsRequired();
        builder.Property(p => p.StrategyWeightAdjust).HasColumnName("strategy_weight_adjust").IsRequired();
        builder.Property(p => p.EnergyAlignmentWeight).HasColumnName("energy_alignment_weight").IsRequired();
        builder.Property(p => p.PostponementPenaltyWeight).HasColumnName("postponement_penalty_weight").IsRequired();
        builder.Property(p => p.EaiMultiplier).HasColumnName("eai_multiplier").IsRequired();
        builder.Property(p => p.MorningEnergyBias).HasColumnName("morning_energy_bias").IsRequired();
        builder.Property(p => p.AfternoonEnergyBias).HasColumnName("afternoon_energy_bias").IsRequired();
        builder.Property(p => p.EveningEnergyBias).HasColumnName("evening_energy_bias").IsRequired();
        builder.Property(p => p.NightEnergyBias).HasColumnName("night_energy_bias").IsRequired();
        
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // BLINDAGEM DE CONCORRÊNCIA: Mapeia o token para a coluna de sistema 'xmin' do PostgreSQL
        // builder.Property(p => p.Version)
        //     .HasColumnName("xmin")
        //     .HasColumnType("xid")
        //     .IsRowVersion();

        // Índice único para garantir relação 1:1 entre Usuário e Perfil de Pontuação
        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("idx_user_scoring_profiles_user_id")
            .IsUnique();
    }
}