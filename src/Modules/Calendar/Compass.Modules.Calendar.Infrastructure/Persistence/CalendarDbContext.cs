using Compass.Modules.Calendar.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace Compass.Modules.Calendar.Infrastructure.Persistence;

public class CalendarDbContext : DbContext
{
    public const string SchemaName = "calendar";

    public DbSet<ScheduleProfile> ScheduleProfiles => Set<ScheduleProfile>();
    public DbSet<DayAvailabilityRule> DayAvailabilityRules => Set<DayAvailabilityRule>();

    public CalendarDbContext(DbContextOptions<CalendarDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CalendarDbContext).Assembly);
    }
}
