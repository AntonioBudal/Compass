using Microsoft.EntityFrameworkCore;
using Compass.Modules.Calendar.Infrastructure.Database.Models;

namespace Compass.Modules.Calendar.Infrastructure.Database;

internal class CalendarDbContext : DbContext
{
    public DbSet<ScheduleProfileData> ScheduleProfiles { get; set; } = null!;
    public DbSet<ScheduleWindowData> ScheduleWindows { get; set; } = null!;

    public CalendarDbContext(DbContextOptions<CalendarDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("calendar");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CalendarDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
