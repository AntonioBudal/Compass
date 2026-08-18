using Compass.Modules.Execution.Domain.DecisionEngine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Modules.Execution.Infrastructure.Database.Configurations;

internal sealed class DailyPlanConfiguration : IEntityTypeConfiguration<DailyPlan>
{
    public void Configure(EntityTypeBuilder<DailyPlan> builder)
    {
        builder.ToTable("DailyPlans");
        builder.HasKey(p => p.Id);

        // Regra de Ouro: Um plano por pessoa por dia
        builder.HasIndex(p => new { p.ProfileId, p.Date }).IsUnique();

        builder.Property(p => p.ProfileId).IsRequired();
        builder.Property(p => p.Date).HasColumnType("date").IsRequired();

        builder.HasMany(p => p.Suggestions)
            .WithOne()
            .HasForeignKey(s => s.DailyPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class SuggestedExecutionConfiguration : IEntityTypeConfiguration<SuggestedExecution>
{
    public void Configure(EntityTypeBuilder<SuggestedExecution> builder)
    {
        builder.ToTable("DailyPlanItems");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ReferenceId).IsRequired();
        builder.Property(s => s.Type).HasMaxLength(50).IsRequired();
        builder.Property(s => s.Title).HasMaxLength(255).IsRequired();
        builder.Property(s => s.Start).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(s => s.End).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(s => s.Reason).HasMaxLength(255).IsRequired();
    }
}
