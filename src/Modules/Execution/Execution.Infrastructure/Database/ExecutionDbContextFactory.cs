using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Compass.Modules.Execution.Infrastructure.Database;

internal class ExecutionDbContextFactory : IDesignTimeDbContextFactory<ExecutionDbContext>
{
    public ExecutionDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<ExecutionDbContext>();
        builder.UseNpgsql("Host=localhost;Database=compass_dev;Username=postgres;Password=postgres");
        return new ExecutionDbContext(builder.Options);
    }
}
