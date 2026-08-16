using Compass.Modules.Execution.Domain.DailyCycles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Modules.Execution.Infrastructure.Database.Configurations;

internal sealed class DailyCycleConfiguration
    : IEntityTypeConfiguration<DailyCycle>
{
    public void Configure(EntityTypeBuilder<DailyCycle> builder)
    {
        builder.ToTable("daily_cycles");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Date)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .IsRequired();

        // PostgreSQL xmin como optimistic concurrency token.
        // É uma shadow property para não contaminar o domínio.
        builder.Property<uint>("Version")
            .IsRowVersion();

        builder.HasMany(c => c.Logs)
            .WithOne()
            .HasForeignKey("DailyCycleId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Logs)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ExecutionLogConfiguration
    : IEntityTypeConfiguration<ExecutionLog>
{
    public void Configure(EntityTypeBuilder<ExecutionLog> builder)
    {
        builder.ToTable("execution_logs");

        builder.HasKey(log => log.Id);

        // O Guid é criado pelo domínio através de Guid.NewGuid().
        // O EF não deve tratar a chave como gerada pelo banco.
        builder.Property(log => log.Id)
            .ValueGeneratedNever();

        builder.Property(log => log.ReferenceId)
            .IsRequired();

        builder.Property(log => log.Type)
            .HasConversion<string>()
            .IsRequired();

        // TimeInterval é um Value Object sem identidade própria.
        // Start e End são persistidos diretamente na tabela execution_logs.
        builder.ComplexProperty(
            log => log.Interval,
            interval =>
            {
                interval.Property(i => i.Start)
                    .HasColumnName("StartTime")
                    .IsRequired();

                interval.Property(i => i.End)
                    .HasColumnName("EndTime")
                    .IsRequired();
            });
    }
}
