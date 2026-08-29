using Compass.Modules.Calendar.Application;
using Compass.Modules.Calendar.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Compass.Modules.Calendar.Presentation.Extensions;

public static class CalendarModuleExtensions
{
    public static IServiceCollection AddCalendarPresentation(this IServiceCollection services)
    {
        services.AddCalendarApplication();
        return services;
    }

    public static IEndpointRouteBuilder MapCalendarModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapCalendarEndpoints();
        return endpoints;
    }
}
