using Compass.Modules.Planning.Application;
using Compass.Modules.Planning.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Compass.Modules.Planning.Presentation.Extensions;

public static class PlanningModuleExtensions
{
    public static IServiceCollection AddPlanningPresentation(this IServiceCollection services)
    {
        services.AddPlanningApplication();
        return services;
    }

    public static IEndpointRouteBuilder MapPlanningModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPlanningEndpoints();
        return endpoints;
    }
}
