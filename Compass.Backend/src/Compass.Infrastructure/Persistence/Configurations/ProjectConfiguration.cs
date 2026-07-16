using Compass.Domain.Entities;
using Compass.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.UserId).HasColumnName("user_id").IsRequired();
        builder.HasOne<User>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(p => p.GoalId).HasColumnName("goal_id");
        builder.HasOne<Goal>().WithMany().HasForeignKey(p => p.GoalId).OnDelete(DeleteBehavior.SetNull);

        builder.Property(p => p.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.ToTable(t => t.HasCheckConstraint("chk_project_title_length", "char_length(title) >= 3"));

        builder.Property(p => p.Deadline).HasColumnName("deadline");

        builder.Property(p => p.Status).HasColumnName("status");
            

        builder.Property(p => p.TotalEstimatedDurationMinutes)
            .HasColumnName("total_estimated_duration_minutes")
            .IsRequired()
            .HasDefaultValue(0);

        builder.ToTable(t => t.HasCheckConstraint("chk_project_duration", "total_estimated_duration_minutes >= 0"));

        builder.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Índice de performance: status de projetos por usuário
        builder.HasIndex(p => new { p.UserId, p.Status })
            .HasDatabaseName("idx_projects_user_status");

        builder.Ignore(p => p.DomainEvents);
    }
}