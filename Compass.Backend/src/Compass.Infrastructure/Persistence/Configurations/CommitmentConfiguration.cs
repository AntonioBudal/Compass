using Compass.Domain.Entities;
using Compass.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Infrastructure.Persistence.Configurations;

public class CommitmentConfiguration : IEntityTypeConfiguration<Commitment>
{
    public void Configure(EntityTypeBuilder<Commitment> builder)
    {
        builder.ToTable("commitments");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");

        // Relacionamentos sem propriedade de navegação (estritamente usando Tipos de Referência em <T>)
        builder.Property(c => c.UserId).HasColumnName("user_id").IsRequired();
        builder.HasOne<User>().WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.ProjectId).HasColumnName("project_id");
        builder.HasOne<Project>().WithMany().HasForeignKey(c => c.ProjectId).OnDelete(DeleteBehavior.SetNull);

        builder.Property(c => c.ConvertedToCommitmentId).HasColumnName("converted_to_commitment_id");
        builder.HasOne<Commitment>().WithMany().HasForeignKey(c => c.ConvertedToCommitmentId).OnDelete(DeleteBehavior.SetNull);

        builder.Property(c => c.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
        builder.ToTable(t => t.HasCheckConstraint("chk_commitment_title_length", "char_length(title) >= 3"));

        builder.Property(c => c.Status).HasColumnName("status");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(c => c.CompletedAt).HasColumnName("completed_at");

        // Configuração da Herança TPH (Table-Per-Hierarchy)
        builder.Property(c => c.Type).HasColumnName("type").IsRequired();
        
        builder.HasDiscriminator(c => c.Type)
            .HasValue<TaskCommitment>(CommitmentType.Task)
            .HasValue<EventCommitment>(CommitmentType.Event)
            .HasValue<HabitCommitment>(CommitmentType.Habit)
            .HasValue<NoteCommitment>(CommitmentType.Note);

        // --- Mapeamento de Propriedades Específicas dos Subtipos ---

        // TaskCommitment & HabitCommitment (Compartilham colunas)
        builder.Property<int>("EstimatedDurationMinutes")
            .HasColumnName("estimated_duration_minutes")
            .HasDefaultValue(30);

        builder.Property<short>("EnergyRequired")
            .HasColumnName("energy_required")
            .HasDefaultValue((short)2);

        // TaskCommitment
        builder.Property<DateTime?>("Deadline").HasColumnName("deadline");
        builder.Property<int>("PostponedCount").HasColumnName("postponed_count").HasDefaultValue(0);

        // EventCommitment
        builder.Property<DateTime?>("StartTime").HasColumnName("start_time").IsRequired(false); 
        builder.Property<DateTime?>("EndTime").HasColumnName("end_time").IsRequired(false); 
        builder.Property<string?>("LocationOrLink").HasColumnName("location_or_link").HasMaxLength(500).IsRequired(false);

        // HabitCommitment
        builder.Property<string?>("CronExpression").HasColumnName("cron_expression").HasMaxLength(100).IsRequired(false); 
        builder.Property<int>("CurrentStreak").HasColumnName("current_streak").HasDefaultValue(0);
        builder.Property<int>("BestStreak").HasColumnName("best_streak").HasDefaultValue(0);

        // NoteCommitment
        builder.Property<string?>("Content").HasColumnName("content");

        // --- Constraints de Integridade ---
        
        builder.ToTable(t => t.HasCheckConstraint("chk_event_time_validity", 
            "(type != 'event') OR (start_time IS NOT NULL AND end_time IS NOT NULL AND end_time > start_time)"));

        // --- Índices Parciais de Alta Performance (< 15ms) ---
        
        // 1. Motor de Decisão: Busca instantânea por itens ativos
        builder.HasIndex(c => new { c.UserId, c.Status, c.Type })
            .HasDatabaseName("idx_commitments_now_engine")
            .HasFilter("status IN ('pending', 'in_progress') AND type IN ('task', 'habit')");

        // 2. Busca por Eventos/Bloqueadores de agenda
        // CORRIGIDO: Valores do Enum em minúsculo
        builder.HasIndex(c => new { c.UserId })
            .HasDatabaseName("idx_commitments_events_lookup")
            .HasFilter("type = 'event' AND status != 'archived'");

        // 3. Otimização de busca por projeto
        builder.HasIndex(c => c.ProjectId)
            .HasDatabaseName("idx_commitments_project_id")
            .HasFilter("project_id IS NOT NULL");

        builder.Ignore(c => c.DomainEvents);
    }
}
    
