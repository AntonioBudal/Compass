using Compass.Modules.Calendar.Application.Abstractions;
using Compass.Modules.Calendar.Application.Commands;
using Compass.Modules.Calendar.Application.Queries;
using Compass.Modules.Calendar.Contracts.DTOs;
using Compass.Modules.Calendar.Domain.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Compass.Modules.Calendar.Presentation.Endpoints;

public static class CalendarEndpoints
{
    public static IEndpointRouteBuilder MapCalendarEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/calendar")
            .WithTags("Calendar");

        group.MapPost("/schedule-profiles", async (
            CreateScheduleProfileCommand command,
            ICommandHandler<CreateScheduleProfileCommand, ScheduleProfileDto> handler,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await handler.HandleAsync(command, cancellationToken);
                return Results.Created($"/api/calendar/schedule-profiles/{result.Id}", result);
            }
            catch (CalendarDomainException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }
        })
        .WithName("CreateScheduleProfile")
        .Produces<ScheduleProfileDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/schedule-profiles/{id:guid}", async (
            Guid id,
            IQueryHandler<GetScheduleProfileByIdQuery, ScheduleProfileDto?> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetScheduleProfileByIdQuery(id), cancellationToken);
            if (result == null)
            {
                return Results.Problem(
                    detail: $"Schedule profile with id '{id}' was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Schedule Profile Not Found"
                );
            }

            return Results.Ok(result);
        })
        .WithName("GetScheduleProfileById")
        .Produces<ScheduleProfileDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/timezones", () =>
        {
            var systemZones = TimeZoneInfo.GetSystemTimeZones();
            var result = systemZones.Select(tz => new TimeZoneItemDto(
                tz.Id,
                tz.DisplayName,
                tz.BaseUtcOffset
            )).OrderBy(z => z.BaseUtcOffset).ThenBy(z => z.DisplayName).ToList();

            return Results.Ok(result);
        })
        .WithName("GetSupportedTimeZones")
        .Produces<IReadOnlyList<TimeZoneItemDto>>(StatusCodes.Status200OK);

        return endpoints;
    }
}
