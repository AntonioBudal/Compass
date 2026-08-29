using Compass.Modules.Calendar.Application.Abstractions;
using Compass.Modules.Calendar.Application.Commands;
using Compass.Modules.Calendar.Application.Queries;
using Compass.Modules.Calendar.Contracts.DTOs;
using Microsoft.Extensions.DependencyInjection;

namespace Compass.Modules.Calendar.Application;

public static class CalendarApplicationExtensions
{
    public static IServiceCollection AddCalendarApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateScheduleProfileCommand, ScheduleProfileDto>, CreateScheduleProfileCommandHandler>();
        services.AddScoped<IQueryHandler<GetScheduleProfileByIdQuery, ScheduleProfileDto?>, GetScheduleProfileByIdQueryHandler>();
        return services;
    }
}
