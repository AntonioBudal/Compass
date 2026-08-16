using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Compass.Modules.Calendar.Infrastructure.Database;

internal class CalendarDbContextFactory : IDesignTimeDbContextFactory<CalendarDbContext>
{
    public CalendarDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<CalendarDbContext>();
        builder.UseNpgsql("Host=localhost;Database=compass_dev;Username=postgres;Password=postgres");
        return new CalendarDbContext(builder.Options);
    }
}
