using Compass.Modules.Planning.Application.Abstractions;
using Compass.Modules.Planning.Application.Commands;
using Compass.Modules.Planning.Application.Queries;
using Compass.Modules.Planning.Contracts.DTOs;
using Microsoft.Extensions.DependencyInjection;

namespace Compass.Modules.Planning.Application;

public static class PlanningApplicationExtensions
{
    public static IServiceCollection AddPlanningApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateTaskCommand, TaskDto>, CreateTaskCommandHandler>();
        services.AddScoped<ICommandHandler<SetTaskEstimateCommand, TaskDto>, SetTaskEstimateCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateTaskDetailsCommand, TaskDto>, UpdateTaskDetailsCommandHandler>();
        services.AddScoped<ICommandHandler<StartTaskCommand, TaskDto>, StartTaskCommandHandler>();
        services.AddScoped<ICommandHandler<CompleteTaskCommand, TaskDto>, CompleteTaskCommandHandler>();

        services.AddScoped<IQueryHandler<GetTasksQuery, IReadOnlyList<TaskDto>>, GetTasksQueryHandler>();
        services.AddScoped<IQueryHandler<GetTaskByIdQuery, TaskDto?>, GetTaskByIdQueryHandler>();

        return services;
    }
}
