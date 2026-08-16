using Compass.Modules.Planning.Application.Tasks.Queries;
using System;
using Compass.Modules.Planning.Application.Habits.ArchiveHabit;
using Compass.Modules.Planning.Application.Habits.ChangeHabitFrequency;
using Compass.Modules.Planning.Application.Habits.CreateHabit;
using Compass.Modules.Planning.Application.Habits.PauseHabit;
using Compass.Modules.Planning.Application.Habits.ResumeHabit;
using Compass.Modules.Planning.Application.Projects.ActivateProject;
using Compass.Modules.Planning.Application.Projects.CompleteProject;
using Compass.Modules.Planning.Application.Projects.CreateProject;
using Compass.Modules.Planning.Application.Projects.PauseProject;
using Compass.Modules.Planning.Application.Tasks.CompleteTask;
using Compass.Modules.Planning.Application.Tasks.CreateTask;
using Compass.Modules.Planning.Application.Tasks.EstimateTask;
using Compass.Modules.Planning.Application.Tasks.StartTask;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Compass.Modules.Planning.Presentation;

public static class PlanningEndpoints
{
    public static IEndpointRouteBuilder MapPlanningEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/planning").WithTags("Planning");

        // --- TASKS ---
        group.MapPost("/tasks", async ([FromBody] CreateTaskCommand command, [FromServices] CreateTaskUseCase useCase) => 
        {
            var result = await useCase.ExecuteAsync(command);
            return Results.Ok(result);
        });

        group.MapPut("/tasks/{id:guid}/estimate", async (Guid id, [FromBody] EstimateTaskRequest request, [FromServices] EstimateTaskUseCase useCase) => 
        {
            await useCase.ExecuteAsync(new EstimateTaskCommand(id, request.EstimatedDurationMinutes));
            return Results.NoContent();
        });

        group.MapPut("/tasks/{id:guid}/start", async (Guid id, [FromServices] StartTaskUseCase useCase) => 
        {
            await useCase.ExecuteAsync(new StartTaskCommand(id));
            return Results.NoContent();
        });

        group.MapPut("/tasks/{id:guid}/complete", async (Guid id, [FromServices] CompleteTaskUseCase useCase) => 
        {
            await useCase.ExecuteAsync(new CompleteTaskCommand(id));
            return Results.NoContent();
        });



        group.MapGet(
            "/tasks/{id:guid}",
            async (
                Guid id,
                [FromServices] ITaskQueryService queryService,
                CancellationToken cancellationToken) =>
            {
                var task = await queryService.GetByIdAsync(
                    id,
                    cancellationToken);

                return task is null
                    ? Results.NotFound()
                    : Results.Ok(task);
            });

        // --- PROJECTS ---
        group.MapPost("/projects", async ([FromBody] CreateProjectCommand command, [FromServices] CreateProjectUseCase useCase) => 
        {
            var result = await useCase.ExecuteAsync(command);
            return Results.Ok(result);
        });

        group.MapPut("/projects/{id:guid}/activate", async (Guid id, [FromServices] ActivateProjectUseCase useCase) => 
        {
            await useCase.ExecuteAsync(new ActivateProjectCommand(id));
            return Results.NoContent();
        });

        group.MapPut("/projects/{id:guid}/pause", async (Guid id, [FromServices] PauseProjectUseCase useCase) => 
        {
            await useCase.ExecuteAsync(new PauseProjectCommand(id));
            return Results.NoContent();
        });

        group.MapPut("/projects/{id:guid}/complete", async (Guid id, [FromServices] CompleteProjectUseCase useCase) => 
        {
            await useCase.ExecuteAsync(new CompleteProjectCommand(id));
            return Results.NoContent();
        });


        // --- HABITS ---
        group.MapPost("/habits", async ([FromBody] CreateHabitCommand command, [FromServices] CreateHabitUseCase useCase) => 
        {
            var result = await useCase.ExecuteAsync(command);
            return Results.Ok(result);
        });

        group.MapPut("/habits/{id:guid}/pause", async (Guid id, [FromServices] PauseHabitUseCase useCase) => 
        {
            await useCase.ExecuteAsync(new PauseHabitCommand(id));
            return Results.NoContent();
        });

        group.MapPut("/habits/{id:guid}/resume", async (Guid id, [FromServices] ResumeHabitUseCase useCase) => 
        {
            await useCase.ExecuteAsync(new ResumeHabitCommand(id));
            return Results.NoContent();
        });

        group.MapPut("/habits/{id:guid}/archive", async (Guid id, [FromServices] ArchiveHabitUseCase useCase) => 
        {
            await useCase.ExecuteAsync(new ArchiveHabitCommand(id));
            return Results.NoContent();
        });

        group.MapPut("/habits/{id:guid}/frequency", async (Guid id, [FromBody] ChangeHabitFrequencyRequest request, [FromServices] ChangeHabitFrequencyUseCase useCase) => 
        {
            await useCase.ExecuteAsync(new ChangeHabitFrequencyCommand(id, request.IntervalDays, request.DaysOfWeek));
            return Results.NoContent();
        });

        return app;
    }
}

// DTOs auxiliares para rotas que não recebem o objeto completo do Command (o ID vem da URL)
public record EstimateTaskRequest(int EstimatedDurationMinutes);
public record ChangeHabitFrequencyRequest(int? IntervalDays, IEnumerable<DayOfWeek>? DaysOfWeek);

