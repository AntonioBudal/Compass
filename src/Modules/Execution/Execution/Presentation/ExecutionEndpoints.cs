using Compass.Modules.Execution.Application.DailyCycles.Queries;
using Compass.Modules.Execution.Application.DailyCycles.CloseCycle;
using Compass.Modules.Execution.Application.DailyCycles.RecordExecution;
using Compass.Modules.Execution.Application.DailyCycles.StartCycle;
using Compass.Modules.Execution.Domain.DailyCycles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading;

namespace Compass.Modules.Execution.Presentation;

public static class ExecutionEndpoints
{
    public static IEndpointRouteBuilder MapExecutionEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/execution")
            .WithTags("Execution");

        // ----------------------------------------------------
        // START DAILY CYCLE
        // ----------------------------------------------------
        group.MapPost(
            "/daily-cycles",
            async (
                [FromBody] StartDailyCycleRequest request,
                [FromServices] StartDailyCycleCommandHandler handler,
                CancellationToken cancellationToken) =>
            {
                var id = await handler.HandleAsync(
                    new StartDailyCycleCommand(request.Date),
                    cancellationToken);

                return Results.Ok(
                    new StartDailyCycleResponse(id));
            });

        // ----------------------------------------------------
        // RECORD EXECUTION
        // ----------------------------------------------------
        group.MapPost(
            "/daily-cycles/{id:guid}/executions",
            async (
                Guid id,
                [FromBody] RecordExecutionRequest request,
                [FromServices] RecordExecutionCommandHandler handler,
                CancellationToken cancellationToken) =>
            {
                if (!Enum.TryParse<ExecutionType>(
                        request.Type,
                        ignoreCase: true,
                        out var executionType))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid execution type.",
                        allowedValues = Enum.GetNames<ExecutionType>()
                    });
                }

                await handler.HandleAsync(
                new RecordExecutionCommand(
                    DailyCycleId: id,
                    ReferenceId: request.ReferenceId,
                    Start: request.Start.ToUniversalTime(),
                    End: request.End.ToUniversalTime(),
                    Type: executionType),
                cancellationToken);

                return Results.NoContent();
            });

        // ----------------------------------------------------
        // CLOSE DAILY CYCLE
        // ----------------------------------------------------
        group.MapPut(
            "/daily-cycles/{id:guid}/close",
            async (
                Guid id,
                [FromServices] CloseDailyCycleCommandHandler handler,
                CancellationToken cancellationToken) =>
            {
                await handler.HandleAsync(
                    new CloseDailyCycleCommand(id),
                    cancellationToken);

                return Results.NoContent();
            });

        // ----------------------------------------------------
        // GET DAILY CYCLE BY ID
        // ----------------------------------------------------
        group.MapGet(
            "/daily-cycles/{id:guid}",
            async (
                Guid id,
                [FromServices] IDailyCycleQueryService queryService,
                CancellationToken cancellationToken) =>
            {
                var cycle = await queryService.GetByIdAsync(
                    id,
                    cancellationToken);

                return cycle is null
                    ? Results.NotFound()
                    : Results.Ok(cycle);
            });

        // ----------------------------------------------------
        // GET DAILY CYCLE BY DATE
        // ----------------------------------------------------
        group.MapGet(
            "/daily-cycles/by-date/{date}",
            async (
                DateOnly date,
                [FromServices] IDailyCycleQueryService queryService,
                CancellationToken cancellationToken) =>
            {
                var cycle = await queryService.GetByDateAsync(
                    date,
                    cancellationToken);

                return cycle is null
                    ? Results.NotFound()
                    : Results.Ok(cycle);
            });

        // ----------------------------------------------------
        // GET DAILY PLAN (DECISION ENGINE PREVIEW)
        // ----------------------------------------------------
        group.MapGet(
            "/daily-plan",
            async (
                [Microsoft.AspNetCore.Mvc.FromQuery] Guid profileId,
                [Microsoft.AspNetCore.Mvc.FromQuery] DateOnly date,
                [Microsoft.AspNetCore.Mvc.FromServices] MediatR.IMediator mediator) =>
            {
                var query = new Compass.Modules.Execution.Application.DailyPlanning.BuildDailyPlanQuery(profileId, date);
                var result = await mediator.Send(query);
                return Microsoft.AspNetCore.Http.Results.Ok(result);
            })
            .WithName("GetDailyPlan");

        // ----------------------------------------------------
        // ACCEPT DAILY PLAN (MATERIALIZAÇÃO)
        // ----------------------------------------------------
        group.MapPost(
            "/daily-plans",
            async (
                [Microsoft.AspNetCore.Mvc.FromBody] Compass.Modules.Execution.Application.DailyPlanning.AcceptDailyPlanCommand command,
                [Microsoft.AspNetCore.Mvc.FromServices] MediatR.IMediator mediator) =>
            {
                try
                {
                    var planId = await mediator.Send(command);
                    return Microsoft.AspNetCore.Http.Results.Ok(new { DailyPlanId = planId });
                }
                catch (Exception ex) when (ex.Message.Contains("already been accepted"))
                {
                    return Microsoft.AspNetCore.Http.Results.Conflict(new { error = ex.Message });
                }
            })
            .WithName("AcceptDailyPlan");
            group.MapGet(
            "/daily-plans/by-date/{date}",
            async (
                [Microsoft.AspNetCore.Mvc.FromQuery] Guid profileId, // Passado na querystring
                DateOnly date,                                       // Passado no path
                [Microsoft.AspNetCore.Mvc.FromServices] Compass.Modules.Execution.Application.DailyPlanning.Queries.IDailyPlanQueryService queryService,
                CancellationToken cancellationToken) =>
            {
                var plan = await queryService.GetByDateAsync(
                    profileId,
                    date,
                    cancellationToken);

                return plan is null
                    ? Microsoft.AspNetCore.Http.Results.NotFound()
                    : Microsoft.AspNetCore.Http.Results.Ok(plan);
            })
            .WithName("GetAcceptedDailyPlan");
            group.MapGet(
            "/daily-adherence",
            async (
                [Microsoft.AspNetCore.Mvc.FromQuery] Guid profileId,
                [Microsoft.AspNetCore.Mvc.FromQuery] DateOnly date,
                [Microsoft.AspNetCore.Mvc.FromServices] MediatR.IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var query = new Compass.Modules.Execution.Application.Analytics.Queries.GetDailyAdherenceQuery(profileId, date);
                var result = await mediator.Send(query, cancellationToken);

                return result is null
                    ? Microsoft.AspNetCore.Http.Results.NotFound(new { error = "No daily plan found for this profile and date." })
                    : Microsoft.AspNetCore.Http.Results.Ok(result);
            })
            .WithName("GetDailyAdherence");

        return app;
    }
}

public sealed record StartDailyCycleRequest(DateOnly Date);
public sealed record StartDailyCycleResponse(Guid DailyCycleId);

public sealed record RecordExecutionRequest(Guid? ReferenceId, DateTimeOffset Start, DateTimeOffset End, string Type);