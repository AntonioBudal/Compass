using Compass.Modules.Execution.Application.DailyCycles.Queries;
using Compass.Modules.Execution.Application.DailyCycles.CloseCycle;
using Compass.Modules.Execution.Application.DailyCycles.RecordExecution;
using Compass.Modules.Execution.Application.DailyCycles.StartCycle;
using Compass.Modules.Execution.Domain.DailyCycles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

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

        return app;
    }
}


public sealed record StartDailyCycleRequest(
    DateOnly Date);


public sealed record StartDailyCycleResponse(
    Guid DailyCycleId);


public sealed record RecordExecutionRequest(
    Guid ReferenceId,
    DateTimeOffset Start,
    DateTimeOffset End,
    string Type);

