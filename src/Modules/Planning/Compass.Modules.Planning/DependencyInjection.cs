using Compass.Modules.Planning.Application.Habits.ArchiveHabit;
using Compass.Modules.Planning.Application.Habits.ChangeHabitFrequency;
using Compass.Modules.Planning.Application.Habits.CreateHabit;
using Compass.Modules.Planning.Application.Habits.PauseHabit;
using Compass.Modules.Planning.Application.Habits.ResumeHabit;
using Compass.Modules.Planning.Application.Projects.ActivateProject;
using Compass.Modules.Planning.Application.Projects.CompleteProject;
using Compass.Modules.Planning.Application.Projects.CreateProject;
using Compass.Modules.Planning.Application.Projects.PauseProject;
using Compass.Modules.Planning.Application.Tasks.CompleteTask;
using Compass.Modules.Planning.Application.Tasks.CreateTask;
using Compass.Modules.Planning.Application.Tasks.EstimateTask;
using Compass.Modules.Planning.Application.Tasks.StartTask;
using Microsoft.Extensions.DependencyInjection;

namespace Compass.Modules.Planning;

public static class DependencyInjection
{
    public static IServiceCollection AddPlanningApplication(this IServiceCollection services)
    {
        // Tasks
        services.AddScoped<CreateTaskUseCase>();
        services.AddScoped<EstimateTaskUseCase>();
        services.AddScoped<StartTaskUseCase>();
        services.AddScoped<CompleteTaskUseCase>();

        // Projects
        services.AddScoped<CreateProjectUseCase>();
        services.AddScoped<ActivateProjectUseCase>();
        services.AddScoped<PauseProjectUseCase>();
        services.AddScoped<CompleteProjectUseCase>();

        // Habits
        services.AddScoped<CreateHabitUseCase>();
        services.AddScoped<PauseHabitUseCase>();
        services.AddScoped<ResumeHabitUseCase>();
        services.AddScoped<ArchiveHabitUseCase>();
        services.AddScoped<ChangeHabitFrequencyUseCase>();

        return services;
    }
}
