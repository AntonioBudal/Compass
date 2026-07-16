using Compass.Domain.Entities;
using Compass.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Infrastructure.Persistence.Configurations;

public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable("goals");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(g => g.UserId).HasColumnName("user_id").IsRequired();
        builder.HasOne<User>().WithMany().HasForeignKey(g => g.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(g => g.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.ToTable(t => t.HasCheckConstraint("chk_goal_title_length", "char_length(title) >= 3"));

        builder.Property(g => g.WhyDescription).HasColumnName("why_description");
        builder.Property(g => g.TargetDate).HasColumnName("target_date");

        builder.Property(g => g.Status).HasColumnName("status");

        builder.Property(g => g.ProgressPercentage)
            .HasColumnName("progress_percentage")
            .HasPrecision(5, 2)
            .IsRequired()
            .HasDefaultValue(0.00m);

        builder.ToTable(t => t.HasCheckConstraint("chk_goal_progress", "progress_percentage BETWEEN 0.00 AND 100.00"));

        builder.Property(g => g.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(g => g.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Ignora a propriedade de eventos de domínio no EF Core
        builder.Ignore(g => g.DomainEvents);
    }
}