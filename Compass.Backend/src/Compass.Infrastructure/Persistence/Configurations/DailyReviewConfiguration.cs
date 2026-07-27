using Compass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Infrastructure.Persistence.Configurations;

public class DailyReviewConfiguration : IEntityTypeConfiguration<DailyReview>
{
    public void Configure(EntityTypeBuilder<DailyReview> builder)
    {
        builder.ToTable("daily_reviews");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        
        builder.Property(d => d.UserId).HasColumnName("user_id").IsRequired();
        
        builder.Property(d => d.ReviewDate)
            .HasColumnName("review_date")
            .HasColumnType("date")
            .IsRequired();
            
        builder.Property(d => d.CompletedCount).HasColumnName("completed_count").IsRequired();
        builder.Property(d => d.PostponedCount).HasColumnName("postponed_count").IsRequired();
        builder.Property(d => d.TotalFocusMinutes).HasColumnName("total_focus_minutes").IsRequired();
        builder.Property(d => d.Notes).HasColumnName("notes").HasColumnType("text").IsRequired();
        builder.Property(d => d.CreatedAt).HasColumnName("created_at").IsRequired();

        // ÍNDICE ÚNICO: Garante no nível do banco que o usuário só pode ter 1 revisão por data
        builder.HasIndex(d => new { d.UserId, d.ReviewDate })
            .HasDatabaseName("idx_daily_reviews_user_date")
            .IsUnique();
    }
}