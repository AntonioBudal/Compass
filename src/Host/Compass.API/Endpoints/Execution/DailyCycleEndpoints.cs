using System;
using System.Threading;
using Compass.Modules.Execution.Application.DailyCycles.CloseCycle;
using Compass.Modules.Execution.Application.DailyCycles.Queries;
using Compass.Modules.Execution.Application.DailyCycles.RecordExecution;
using Compass.Modules.Execution.Application.DailyCycles.StartCycle;
using Compass.Modules.Execution.Domain.DailyCycles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Compass.API.Endpoints.Execution;

public static class DailyCycleEndpoints
{
    public static IEndpointRouteBuilder MapExecutionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/execution/daily-cycles")
                       .WithTags("Execution - Daily Cycles");

        // --- QUERIES (GET) ---

        group.MapGet("/{id:guid}", async (
            Guid id,
            [FromServices] IDailyCycleQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var result = await queryService.GetByIdAsync(id, cancellationToken);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetDailyCycleById")
        .Produces<DailyCycleDetailsDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/by-date/{date}", async (
            DateOnly date,
            [FromServices] IDailyCycleQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var result = await queryService.GetByDateAsync(date, cancellationToken);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetDailyCycleByDate")
        .Produces<DailyCycleDetailsDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);


        // --- COMMANDS (POST) ---

        group.MapPost("/", async (
            [FromBody] StartDailyCycleRequest request,
            [FromServices] StartDailyCycleCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new StartDailyCycleCommand(request.Date);
            var cycleId = await handler.HandleAsync(command, cancellationToken);
            
            return Results.Created($"/api/execution/daily-cycles/{cycleId}", new { Id = cycleId });
        })
        .WithName("StartDailyCycle")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/executions", async (
            Guid id,
            [FromBody] RecordExecutionRequest request,
            [FromServices] RecordExecutionCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RecordExecutionCommand(
                id, 
                request.ReferenceId, 
                request.Start, 
                request.End, 
                request.Type);

            await handler.HandleAsync(command, cancellationToken);
            return Results.Ok();
        })
        .WithName("RecordExecution")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/close", async (
            Guid id,
            [FromServices] CloseDailyCycleCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CloseDailyCycleCommand(id);
            await handler.HandleAsync(command, cancellationToken);
            return Results.Ok();
        })
        .WithName("CloseDailyCycle")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}

// DTOs de Entrada da API
public record StartDailyCycleRequest(DateOnly Date);
public record RecordExecutionRequest(Guid ReferenceId, DateTimeOffset Start, DateTimeOffset End, ExecutionType Type);
