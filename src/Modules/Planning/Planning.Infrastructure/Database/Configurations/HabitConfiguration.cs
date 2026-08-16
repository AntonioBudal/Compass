using Compass.Modules.Planning.Domain.Habits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Compass.Modules.Planning.Infrastructure.Database.Configurations;

internal class HabitConfiguration : IEntityTypeConfiguration<Habit>
{
    public void Configure(EntityTypeBuilder<Habit> builder)
    {
        builder.ToTable("Habits");
        builder.HasKey(h => h.Id);
        
        builder.Property(h => h.Title).IsRequired().HasMaxLength(255);
        builder.Property(h => h.Status).HasConversion<string>().IsRequired();
        
        // Mapeamento Mágico do Value Object: Salva nas colunas da mesma tabela Habit
        builder.OwnsOne(h => h.Frequency, f => 
        {
            f.Property(x => x.Type).HasConversion<string>().HasColumnName("FrequencyType").IsRequired();
            f.Property(x => x.IntervalDays).HasColumnName("FrequencyIntervalDays");
            
            // Acessa o campo privado Bitmask via Reflection para persistência
            f.Property<int>("_daysOfWeekBitmask").HasColumnName("FrequencyDaysOfWeekBitmask").IsRequired();
        });
    }
}
