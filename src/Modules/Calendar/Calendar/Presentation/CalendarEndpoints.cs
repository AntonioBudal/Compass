using Compass.Modules.Calendar.Application.Profiles.Commands;
using Compass.Modules.Calendar.Application.Profiles.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading;

namespace Compass.Modules.Calendar.Presentation;

public static class CalendarEndpoints
{
    public static void MapCalendarEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/calendar")
            .WithTags("Calendar");

        // -----------------------------------------------------------------
        // PROFILES (Read & Write Base)
        // -----------------------------------------------------------------
        
        group.MapGet(
            "/profiles/{profileId:guid}",
            async (
                Guid profileId,
                [FromServices] IScheduleProfileQueryService queryService,
                CancellationToken cancellationToken) =>
            {
                var profile = await queryService.GetProfileAsync(
                    profileId,
                    cancellationToken);

                return profile is null
                    ? Results.NotFound("Profile not found.")
                    : Results.Ok(profile);
            })
            .WithName("GetScheduleProfile");

        group.MapPost(
            "/profiles/{profileId:guid}",
            async (
                Guid profileId,
                [FromBody] CreateProfileRequest request,
                [FromServices] IScheduleProfileCommandService commandService,
                CancellationToken cancellationToken) =>
            {
                await commandService.CreateProfileAsync(
                    profileId,
                    request,
                    cancellationToken);

                return Results.NoContent();
            })
            .WithName("CreateScheduleProfile");

        // -----------------------------------------------------------------
        // SCHEDULE WINDOWS (Configuração da Semana Padrão)
        // -----------------------------------------------------------------
        
        group.MapPost(
            "/profiles/{profileId:guid}/windows",
            async (
                Guid profileId,
                [FromBody] AddWindowRequest request,
                [FromServices] IScheduleProfileCommandService commandService,
                CancellationToken cancellationToken) =>
            {
                await commandService.AddWindowAsync(profileId, request, cancellationToken);
                return Results.NoContent();
            })
            .WithName("AddScheduleWindow");

        group.MapDelete(
            "/profiles/{profileId:guid}/windows/{windowId:guid}",
            async (
                Guid profileId,
                Guid windowId,
                [FromServices] IScheduleProfileCommandService commandService,
                CancellationToken cancellationToken) =>
            {
                await commandService.RemoveWindowAsync(profileId, windowId, cancellationToken);
                return Results.NoContent();
            })
            .WithName("RemoveScheduleWindow");

        // -----------------------------------------------------------------
        // EXCEPTIONS
        // -----------------------------------------------------------------

        group.MapPost(
            "/profiles/{profileId:guid}/exceptions",
            async (
                Guid profileId,
                [FromBody] CreateExceptionRequest request,
                [FromServices] IAddScheduleExceptionCommandService commandService,
                CancellationToken cancellationToken) =>
            {
                await commandService.AddExceptionAsync(profileId, request, cancellationToken);
                return Results.NoContent();
            })
            .WithName("AddScheduleException");

        // -----------------------------------------------------------------
        // AVAILABILITY (Temporal Projection for Engine)
        // -----------------------------------------------------------------
        
        group.MapGet(
            "/availability",
            async (
                [FromQuery] Guid profileId,
                [FromQuery] DateOnly date,
                [FromServices] IAvailabilityQueryService queryService,
                CancellationToken cancellationToken) =>
            {
                if (profileId == Guid.Empty)
                {
                    return Results.BadRequest("ProfileId is required.");
                }

                var availability = await queryService.GetAvailabilityAsync(
                    profileId,
                    date,
                    cancellationToken);

                return availability is null
                    ? Results.NotFound("Profile not found.")
                    : Results.Ok(availability);
            })
            .WithName("GetDailyAvailability");
    }
}