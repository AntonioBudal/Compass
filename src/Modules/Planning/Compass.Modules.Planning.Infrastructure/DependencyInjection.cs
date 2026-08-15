using Compass.Modules.Planning.Application.Habits;
using Compass.Modules.Planning.Application.Projects;
using Compass.Modules.Planning.Application.Tasks;
using Compass.Modules.Planning.Infrastructure.Database;
using Compass.Modules.Planning.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Compass.Modules.Planning.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPlanningInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PlanningDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IHabitRepository, HabitRepository>();

        return services;
    }
}
