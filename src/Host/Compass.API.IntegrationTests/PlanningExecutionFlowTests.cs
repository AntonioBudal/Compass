using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Testcontainers.PostgreSql;

namespace Compass.API.IntegrationTests;

public sealed class PlanningExecutionFlowTests
{
    [Fact]
    public async Task Recording_execution_for_ready_task_should_move_task_to_in_progress()
    {
        // ----------------------------------------------------
        // DATABASE
        // ----------------------------------------------------

        await using var postgres =
            new PostgreSqlBuilder("postgres:16-alpine")
                .WithDatabase("compass_api_e2e_test")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

        await postgres.StartAsync();


        // ----------------------------------------------------
        // API
        // ----------------------------------------------------

        using var factory =
            new CompassApiFactory(
                postgres.GetConnectionString());

        using var client =
            factory.CreateClient(
                new Microsoft.AspNetCore.Mvc.Testing
                    .WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false,
                    BaseAddress = new Uri("http://localhost")
                });


        // ====================================================
        // 1. CREATE TASK
        // ====================================================

        using var createTaskResponse =
            await client.PostAsJsonAsync(
                "/api/planning/tasks",
                new
                {
                    title = "Fluxo E2E Monólito Modular"
                });

        await AssertSuccessAsync(createTaskResponse);

        using var createdTaskJson =
            await ReadJsonAsync(createTaskResponse);

        var taskId =
            createdTaskJson.RootElement
                .GetProperty("taskId")
                .GetGuid();


        // ====================================================
        // 2. ESTIMATE TASK
        // Draft -> Ready
        // ====================================================

        using var estimateResponse =
            await client.PutAsJsonAsync(
                $"/api/planning/tasks/{taskId}/estimate",
                new
                {
                    estimatedDurationMinutes = 120
                });

        Assert.Equal(
            HttpStatusCode.NoContent,
            estimateResponse.StatusCode);


        // ----------------------------------------------------
        // Confirmar precondição do evento
        // ----------------------------------------------------

        using var readyTaskResponse =
            await client.GetAsync(
                $"/api/planning/tasks/{taskId}");

        await AssertSuccessAsync(readyTaskResponse);

        using var readyTaskJson =
            await ReadJsonAsync(readyTaskResponse);

        Assert.Equal(
            "Ready",
            readyTaskJson.RootElement
                .GetProperty("status")
                .GetString());


        // ====================================================
        // 3. START DAILY CYCLE
        // ====================================================

        const string cycleDate = "2026-08-16";

        using var cycleResponse =
            await client.PostAsJsonAsync(
                "/api/execution/daily-cycles",
                new
                {
                    date = cycleDate
                });

        await AssertSuccessAsync(cycleResponse);

        using var cycleJson =
            await ReadJsonAsync(cycleResponse);

        var cycleId =
            cycleJson.RootElement
                .GetProperty("dailyCycleId")
                .GetGuid();


        // ====================================================
        // 4. RECORD EXECUTION
        //
        // Importante:
        // enviamos -03:00 deliberadamente.
        //
        // A fronteira HTTP deve normalizar:
        //
        // 10:00 -03:00 -> 13:00 UTC
        // 11:00 -03:00 -> 14:00 UTC
        // ====================================================

        using var recordResponse =
            await client.PostAsJsonAsync(
                $"/api/execution/daily-cycles/{cycleId}/executions",
                new
                {
                    referenceId = taskId,
                    start = "2026-08-16T10:00:00-03:00",
                    end = "2026-08-16T11:00:00-03:00",
                    type = "DeepWork"
                });

        Assert.Equal(
            HttpStatusCode.NoContent,
            recordResponse.StatusCode);


        // ====================================================
        // 5. PLANNING DEVE TER REAGIDO AO EVENTO
        //
        // ExecutionRecordedIntegrationEvent
        //          ↓
        // MediatR
        //          ↓
        // Planning
        //          ↓
        // Ready -> InProgress
        // ====================================================

        using var taskResponse =
            await client.GetAsync(
                $"/api/planning/tasks/{taskId}");

        await AssertSuccessAsync(taskResponse);

        using var taskJson =
            await ReadJsonAsync(taskResponse);

        var task =
            taskJson.RootElement;

        Assert.Equal(
            taskId,
            task.GetProperty("id").GetGuid());

        Assert.Equal(
            "InProgress",
            task.GetProperty("status").GetString());

        Assert.Equal(
            120,
            task.GetProperty(
                "estimatedDurationMinutes")
                .GetInt32());


        // ====================================================
        // 6. EXECUTION LOG DEVE EXISTIR
        // ====================================================

        using var persistedCycleResponse =
            await client.GetAsync(
                $"/api/execution/daily-cycles/{cycleId}");

        await AssertSuccessAsync(
            persistedCycleResponse);

        using var persistedCycleJson =
            await ReadJsonAsync(
                persistedCycleResponse);

        var persistedCycle =
            persistedCycleJson.RootElement;

        Assert.Equal(
            "Active",
            persistedCycle
                .GetProperty("status")
                .GetString());

        var logs =
            persistedCycle.GetProperty("logs");

        Assert.Equal(
            1,
            logs.GetArrayLength());

        var log =
            logs[0];

        Assert.Equal(
            taskId,
            log.GetProperty(
                "referenceId")
                .GetGuid());

        Assert.Equal(
            "DeepWork",
            log.GetProperty(
                "type")
                .GetString());


        // ====================================================
        // 7. REGRESSÃO UTC
        // ====================================================

        var persistedStart =
            log.GetProperty("start")
                .GetDateTimeOffset();

        var persistedEnd =
            log.GetProperty("end")
                .GetDateTimeOffset();

        Assert.Equal(
            new DateTimeOffset(
                2026,
                8,
                16,
                13,
                0,
                0,
                TimeSpan.Zero),
            persistedStart);

        Assert.Equal(
            new DateTimeOffset(
                2026,
                8,
                16,
                14,
                0,
                0,
                TimeSpan.Zero),
            persistedEnd);

        Assert.Equal(
            TimeSpan.Zero,
            persistedStart.Offset);

        Assert.Equal(
            TimeSpan.Zero,
            persistedEnd.Offset);
    }


    private static async Task AssertSuccessAsync(
        HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body =
            await response.Content
                .ReadAsStringAsync();

        Assert.True(
            response.IsSuccessStatusCode,
            $"HTTP {(int)response.StatusCode} " +
            $"{response.StatusCode}. Body: {body}");
    }


    private static async Task<JsonDocument> ReadJsonAsync(
        HttpResponseMessage response)
    {
        var body =
            await response.Content
                .ReadAsStringAsync();

        return JsonDocument.Parse(body);
    }
}
