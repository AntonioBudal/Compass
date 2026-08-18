using Compass.Modules.Execution.Application.DailyCycles.CloseCycle;
using Compass.Modules.Execution.Application.DailyCycles.RecordExecution;
using Compass.Modules.Execution.Application.DailyCycles.StartCycle;
using Microsoft.Extensions.DependencyInjection;

namespace Compass.Modules.Execution;

public static class DependencyInjection
{
    public static IServiceCollection AddExecutionApplication(
        this IServiceCollection services)
    {
        services.AddScoped<StartDailyCycleCommandHandler>();
        services.AddScoped<RecordExecutionCommandHandler>();
        services.AddScoped<CloseDailyCycleCommandHandler>();

        return services;
    }
}