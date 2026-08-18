using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Compass.API.IntegrationTests;

public sealed class DecisionEngineE2ETests
    : IClassFixture<CompassApiFactory>
{
    private readonly CompassApiFactory _factory;

    public DecisionEngineE2ETests(
        CompassApiFactory factory)
    {
        _factory = factory;
    }

    private sealed record CreateProfileRequest(
        string Timezone,
        Dictionary<string, List<WindowDto>> WeeklySchedule);

    private sealed record WindowDto(
        string StartTime,
        string EndTime);

    private sealed record CreateTaskRequest(
        string Title,
        int? EstimatedDurationMinutes,
        DateTimeOffset? HardDeadline,
        Guid? ProjectId);

    private sealed record CreateTaskResponse(
        Guid TaskId);

    private sealed record EstimateTaskRequest(
        int EstimatedDurationMinutes);

    private sealed record PlanResponse(
        string Date,
        List<PlanSuggestion> Suggestions);

    private sealed record PlanSuggestion(
        Guid ReferenceId,
        string Type,
        DateTimeOffset Start,
        DateTimeOffset End);

    [Fact]
    public async Task Build_Daily_Plan_Should_Resolve_UTC_And_America_Sao_Paulo()
    {
        using var client = _factory.CreateClient();

        var utcProfileId = Guid.NewGuid();
        var saoPauloProfileId = Guid.NewGuid();
        const string date = "2026-08-17";

        await CreateCalendarProfileAsync(
            client,
            utcProfileId,
            "UTC",
            "08:00",
            "12:00");

        await CreateCalendarProfileAsync(
            client,
            saoPauloProfileId,
            "America/Sao_Paulo",
            "08:00",
            "12:00");

        var taskId = await CreateExecutableTaskAsync(
            client,
            "Timezone E2E Task",
            120);

        var utcPlan = await GetDailyPlanAsync(
            client,
            utcProfileId,
            date);

        Assert.Equal(date, utcPlan.Date);

        var utcSuggestion =
            Assert.Single(utcPlan.Suggestions);

        Assert.Equal(
            taskId,
            utcSuggestion.ReferenceId);

        Assert.Equal(
            "Task",
            utcSuggestion.Type);

        Assert.Equal(
            new DateTimeOffset(
                2026,
                8,
                17,
                8,
                0,
                0,
                TimeSpan.Zero),
            utcSuggestion.Start);

        Assert.Equal(
            new DateTimeOffset(
                2026,
                8,
                17,
                10,
                0,
                0,
                TimeSpan.Zero),
            utcSuggestion.End);

        Assert.Equal(
            TimeSpan.Zero,
            utcSuggestion.Start.Offset);

        Assert.Equal(
            TimeSpan.Zero,
            utcSuggestion.End.Offset);

        var saoPauloPlan = await GetDailyPlanAsync(
            client,
            saoPauloProfileId,
            date);

        Assert.Equal(date, saoPauloPlan.Date);

        var saoPauloSuggestion =
            Assert.Single(saoPauloPlan.Suggestions);

        Assert.Equal(
            taskId,
            saoPauloSuggestion.ReferenceId);

        Assert.Equal(
            "Task",
            saoPauloSuggestion.Type);

        Assert.Equal(
            new DateTimeOffset(
                2026,
                8,
                17,
                11,
                0,
                0,
                TimeSpan.Zero),
            saoPauloSuggestion.Start);

        Assert.Equal(
            new DateTimeOffset(
                2026,
                8,
                17,
                13,
                0,
                0,
                TimeSpan.Zero),
            saoPauloSuggestion.End);

        Assert.Equal(
            TimeSpan.Zero,
            saoPauloSuggestion.Start.Offset);

        Assert.Equal(
            TimeSpan.Zero,
            saoPauloSuggestion.End.Offset);
    }

    private static async Task CreateCalendarProfileAsync(
        HttpClient client,
        Guid profileId,
        string timezone,
        string start,
        string end)
    {
        var weeklySchedule =
            new Dictionary<string, List<WindowDto>>
            {
                [DayOfWeek.Monday.ToString()] =
                    new List<WindowDto>
                    {
                        new(start, end)
                    }
            };

        using var response =
            await client.PostAsJsonAsync(
                $"/api/calendar/profiles/{profileId}",
                new CreateProfileRequest(
                    timezone,
                    weeklySchedule));

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);
    }

    private static async Task<Guid> CreateExecutableTaskAsync(
        HttpClient client,
        string title,
        int estimatedMinutes)
    {
        using var createResponse =
            await client.PostAsJsonAsync(
                "/api/planning/tasks",
                new CreateTaskRequest(
                    title,
                    null,
                    null,
                    null));

        Assert.True(
            createResponse.IsSuccessStatusCode);

        var created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateTaskResponse>();

        Assert.NotNull(created);

        using var estimateResponse =
            await client.PutAsJsonAsync(
                $"/api/planning/tasks/{created.TaskId}/estimate",
                new EstimateTaskRequest(
                    estimatedMinutes));

        Assert.True(
            estimateResponse.IsSuccessStatusCode);

        return created.TaskId;
    }

    private static async Task<PlanResponse> GetDailyPlanAsync(
        HttpClient client,
        Guid profileId,
        string date)
    {
        using var response =
            await client.GetAsync(
                $"/api/execution/daily-plan" +
                $"?profileId={profileId}" +
                $"&date={date}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var plan =
            await response.Content
                .ReadFromJsonAsync<PlanResponse>();

        Assert.NotNull(plan);

        return plan;
    }
}