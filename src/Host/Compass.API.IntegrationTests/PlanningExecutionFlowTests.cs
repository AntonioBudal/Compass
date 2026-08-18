using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Compass.API.IntegrationTests;

public class PlanningExecutionFlowTests : IClassFixture<CompassApiFactory>
{
    private readonly CompassApiFactory _factory;

    public PlanningExecutionFlowTests(CompassApiFactory factory)
    {
        _factory = factory;
    }

    private sealed record CreateTaskResponse(Guid TaskId);

    private sealed record StartDailyCycleResponse(Guid DailyCycleId);

    private sealed record TaskDto(
        Guid Id,
        string Title,
        string Status,
        int? EstimatedDurationMinutes,
        DateTimeOffset? HardDeadline,
        Guid? ProjectId);

    private sealed record ExecutionLogDto(
        Guid Id,
        Guid ReferenceId,
        string Type,
        DateTimeOffset Start,
        DateTimeOffset End);

    private sealed record DailyCycleDto(
        Guid Id,
        DateOnly Date,
        string Status,
        IReadOnlyList<ExecutionLogDto> Logs);

    [Fact]
    public async Task Complete_Flow_Creates_Task_Records_Execution_And_Updates_Status()
    {
        var client = _factory.CreateClient();

        // ====================================================
        // 1. CREATE TASK
        // ====================================================

        var createResponse = await client.PostAsJsonAsync(
            "/api/planning/tasks",
            new
            {
                title = "Implement Decision Engine"
            });

        createResponse.EnsureSuccessStatusCode();

        var createdTask =
            await createResponse.Content
                .ReadFromJsonAsync<CreateTaskResponse>();

        Assert.NotNull(createdTask);

        var taskId = createdTask.TaskId;


        // ====================================================
        // 2. ESTIMATE TASK
        // Draft -> Ready
        // ====================================================

        var estimateResponse = await client.PutAsJsonAsync(
            $"/api/planning/tasks/{taskId}/estimate",
            new
            {
                estimatedDurationMinutes = 120
            });

        Assert.Equal(
            HttpStatusCode.NoContent,
            estimateResponse.StatusCode);


        // Confirmar a precondição do Integration Event
        var readyResponse = await client.GetAsync(
            $"/api/planning/tasks/{taskId}");

        readyResponse.EnsureSuccessStatusCode();

        var readyTask =
            await readyResponse.Content
                .ReadFromJsonAsync<TaskDto>();

        Assert.NotNull(readyTask);
        Assert.Equal("Ready", readyTask.Status);


        // ====================================================
        // 3. START DAILY CYCLE
        // ====================================================

        var date = new DateOnly(2026, 8, 17);

        var cycleResponse = await client.PostAsJsonAsync(
            "/api/execution/daily-cycles",
            new
            {
                date
            });

        cycleResponse.EnsureSuccessStatusCode();

        var cycle =
            await cycleResponse.Content
                .ReadFromJsonAsync<StartDailyCycleResponse>();

        Assert.NotNull(cycle);

        var cycleId = cycle.DailyCycleId;


        // ====================================================
        // 4. RECORD EXECUTION
        //
        // Deliberadamente enviamos -03:00 para também
        // proteger a normalização UTC.
        // ====================================================

        var recordResponse = await client.PostAsJsonAsync(
            $"/api/execution/daily-cycles/{cycleId}/executions",
            new
            {
                referenceId = taskId,
                start = "2026-08-17T08:00:00-03:00",
                end = "2026-08-17T09:00:00-03:00",
                type = "DeepWork"
            });

        Assert.Equal(
            HttpStatusCode.NoContent,
            recordResponse.StatusCode);


        // NÃO usar Task.Delay.
        // MediatR Publish é aguardado pela request.


        // ====================================================
        // 5. PLANNING DEVE ESTAR IN PROGRESS
        // ====================================================

        var taskResponse = await client.GetAsync(
            $"/api/planning/tasks/{taskId}");

        taskResponse.EnsureSuccessStatusCode();

        var task =
            await taskResponse.Content
                .ReadFromJsonAsync<TaskDto>();

        Assert.NotNull(task);

        Assert.Equal(taskId, task.Id);
        Assert.Equal("InProgress", task.Status);
        Assert.Equal(120, task.EstimatedDurationMinutes);


        // ====================================================
        // 6. EXECUTION LOG DEVE TER SIDO PERSISTIDO
        // ====================================================

        var persistedCycleResponse = await client.GetAsync(
            $"/api/execution/daily-cycles/{cycleId}");

        persistedCycleResponse.EnsureSuccessStatusCode();

        var persistedCycle =
            await persistedCycleResponse.Content
                .ReadFromJsonAsync<DailyCycleDto>();

        Assert.NotNull(persistedCycle);
        Assert.Equal("Active", persistedCycle.Status);

        var log = Assert.Single(persistedCycle.Logs);

        Assert.Equal(taskId, log.ReferenceId);
        Assert.Equal("DeepWork", log.Type);


        // ====================================================
        // 7. UTC REGRESSION
        //
        // 08:00 -03 -> 11:00 UTC
        // 09:00 -03 -> 12:00 UTC
        // ====================================================

        Assert.Equal(
            new DateTimeOffset(
                2026, 8, 17,
                11, 0, 0,
                TimeSpan.Zero),
            log.Start);

        Assert.Equal(
            new DateTimeOffset(
                2026, 8, 17,
                12, 0, 0,
                TimeSpan.Zero),
            log.End);

        Assert.Equal(TimeSpan.Zero, log.Start.Offset);
        Assert.Equal(TimeSpan.Zero, log.End.Offset);
    }
}
