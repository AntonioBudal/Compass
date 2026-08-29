using System.Net;
using System.Net.Http.Json;
using Compass.Modules.Planning.Contracts.DTOs;
using FluentAssertions;
using Xunit;

namespace Compass.Host.IntegrationTests;

public class TaskLifecycleApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TaskLifecycleApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task StartAndCompleteTask_ShouldProgressLifecycleCorrectly()
    {
        // Arrange - Create Ready task
        var createResponse = await _client.PostAsJsonAsync(
            "/api/planning/tasks",
            new CreateTaskRequest("Desenvolver módulo", null, 60, null)
        );
        var created = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

        // Act 1 - Start task
        var startResponse = await _client.PostAsync($"/api/planning/tasks/{created!.Id}/start", null);

        // Assert 1
        startResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var started = await startResponse.Content.ReadFromJsonAsync<TaskDto>();
        started!.Status.Should().Be("InProgress");

        // Act 2 - Complete task
        var completeResponse = await _client.PostAsync($"/api/planning/tasks/{created.Id}/complete", null);

        // Assert 2
        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var completed = await completeResponse.Content.ReadFromJsonAsync<TaskDto>();
        completed!.Status.Should().Be("Done");
        completed.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task StartDraftTask_ShouldReturn400BadRequest()
    {
        // Arrange - Create Draft task
        var createResponse = await _client.PostAsJsonAsync(
            "/api/planning/tasks",
            new CreateTaskRequest("Rascunho de ideia", null, null, null)
        );
        var created = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

        // Act - Attempt start
        var startResponse = await _client.PostAsync($"/api/planning/tasks/{created!.Id}/start", null);

        // Assert
        startResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
