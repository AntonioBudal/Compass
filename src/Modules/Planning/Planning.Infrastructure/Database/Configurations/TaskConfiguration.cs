using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task = Compass.Modules.Planning.Domain.Tasks.Task;

namespace Compass.Modules.Planning.Infrastructure.Database.Configurations;

internal class TaskConfiguration : IEntityTypeConfiguration<Task>
{
    public void Configure(EntityTypeBuilder<Task> builder)
    {
        builder.ToTable("Tasks");
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Title).IsRequired().HasMaxLength(255);
        builder.Property(t => t.Status).HasConversion<string>().IsRequired();
        
        // Propriedades nulas como ProjectId e HardDeadline são mapeadas naturalmente
    }
}
