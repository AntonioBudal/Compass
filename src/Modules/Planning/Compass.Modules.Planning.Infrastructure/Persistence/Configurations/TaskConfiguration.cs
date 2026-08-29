using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskModel = Compass.Modules.Planning.Domain.Model.Task;

namespace Compass.Modules.Planning.Infrastructure.Persistence.Configurations;

public class TaskConfiguration : IEntityTypeConfiguration<TaskModel>
{
    public void Configure(EntityTypeBuilder<TaskModel> builder)
    {
        builder.ToTable("tasks", PlanningDbContext.SchemaName);

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.Title)
            .HasColumnName("title")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnName("description");

        builder.Property(t => t.DurationMinutes)
            .HasColumnName("duration_minutes");

        builder.Property(t => t.Deadline)
            .HasColumnName("deadline");

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(t => t.CompletedAt)
            .HasColumnName("completed_at");

        builder.HasIndex(t => t.Status)
            .HasDatabaseName("IX_tasks_status");

        builder.HasIndex(t => t.CreatedAt)
            .HasDatabaseName("IX_tasks_created_at");
    }
}
