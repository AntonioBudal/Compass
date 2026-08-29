using Compass.Modules.Planning.Application.Abstractions;
using Compass.Modules.Planning.Application.Commands;
using Compass.Modules.Planning.Application.Queries;
using Compass.Modules.Planning.Contracts.DTOs;
using Compass.Modules.Planning.Domain.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TaskStatus = Compass.Modules.Planning.Domain.Model.TaskStatus;

namespace Compass.Modules.Planning.Presentation.Endpoints;

public static class PlanningEndpoints
{
    public static IEndpointRouteBuilder MapPlanningEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/planning")
            .WithTags("Planning");

        group.MapPost("/tasks", async (
            CreateTaskCommand command,
            ICommandHandler<CreateTaskCommand, TaskDto> handler,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await handler.HandleAsync(command, cancellationToken);
                return Results.Created($"/api/planning/tasks/{result.Id}", result);
            }
            catch (PlanningDomainException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }
        })
        .WithName("CreateTask")
        .Produces<TaskDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/tasks", async (
            string? status,
            IQueryHandler<GetTasksQuery, IReadOnlyList<TaskDto>> handler,
            CancellationToken cancellationToken) =>
        {
            TaskStatus? filterStatus = null;
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TaskStatus>(status, true, out var parsed))
            {
                filterStatus = parsed;
            }

            var result = await handler.HandleAsync(new GetTasksQuery(filterStatus), cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetTasks")
        .Produces<IReadOnlyList<TaskDto>>(StatusCodes.Status200OK);

        group.MapGet("/tasks/{id:guid}", async (
            Guid id,
            IQueryHandler<GetTaskByIdQuery, TaskDto?> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetTaskByIdQuery(id), cancellationToken);
            if (result == null)
            {
                return Results.Problem(
                    detail: $"Task with id '{id}' was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Task Not Found"
                );
            }

            return Results.Ok(result);
        })
        .WithName("GetTaskById")
        .Produces<TaskDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/tasks/{id:guid}", async (
            Guid id,
            UpdateTaskRequest request,
            ICommandHandler<UpdateTaskDetailsCommand, TaskDto> handler,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var command = new UpdateTaskDetailsCommand(
                    id,
                    request.Title,
                    request.Description,
                    request.DurationMinutes,
                    request.Deadline
                );

                var result = await handler.HandleAsync(command, cancellationToken);
                return Results.Ok(result);
            }
            catch (PlanningDomainException ex) when (ex.Message.Contains("not found"))
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Task Not Found"
                );
            }
            catch (PlanningDomainException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error"
                );
            }
        })
        .WithName("UpdateTaskDetails")
        .Produces<TaskDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/tasks/{id:guid}/start", async (
            Guid id,
            ICommandHandler<StartTaskCommand, TaskDto> handler,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await handler.HandleAsync(new StartTaskCommand(id), cancellationToken);
                return Results.Ok(result);
            }
            catch (PlanningDomainException ex) when (ex.Message.Contains("not found"))
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Task Not Found"
                );
            }
            catch (PlanningDomainException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Lifecycle Error"
                );
            }
        })
        .WithName("StartTask")
        .Produces<TaskDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/tasks/{id:guid}/complete", async (
            Guid id,
            ICommandHandler<CompleteTaskCommand, TaskDto> handler,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await handler.HandleAsync(new CompleteTaskCommand(id), cancellationToken);
                return Results.Ok(result);
            }
            catch (PlanningDomainException ex) when (ex.Message.Contains("not found"))
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Task Not Found"
                );
            }
            catch (PlanningDomainException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Lifecycle Error"
                );
            }
        })
        .WithName("CompleteTask")
        .Produces<TaskDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }
}
