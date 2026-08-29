using System.Net;
using System.Net.Http.Json;
using Compass.Modules.Planning.Contracts.DTOs;
using FluentAssertions;
using Xunit;

namespace Compass.Host.IntegrationTests;

public class TaskEstimateApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TaskEstimateApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PatchTask_WithPositiveEstimate_ShouldPromoteToReady()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(
            "/api/planning/tasks",
            new CreateTaskRequest("Criar documentação de APIs", null, null, null)
        );
        var created = await createResponse.Content.ReadFromJsonAsync<TaskDto>();
        created!.Status.Should().Be("Draft");

        // Act
        var patchResponse = await _client.PatchAsJsonAsync(
            $"/api/planning/tasks/{created.Id}",
            new UpdateTaskRequest("Criar documentação de APIs", null, 90, null)
        );

        // Assert
        patchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await patchResponse.Content.ReadFromJsonAsync<TaskDto>();
        updated.Should().NotBeNull();
        updated!.Status.Should().Be("Ready");
        updated.DurationMinutes.Should().Be(90);
    }

    [Fact]
    public async Task PatchTask_WithInvalidEstimate_ShouldReturn400BadRequest()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(
            "/api/planning/tasks",
            new CreateTaskRequest("Tarefa com estimativa zero", null, null, null)
        );
        var created = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

        // Act
        var patchResponse = await _client.PatchAsJsonAsync(
            $"/api/planning/tasks/{created!.Id}",
            new UpdateTaskRequest("Tarefa com estimativa zero", null, -5, null)
        );

        // Assert
        patchResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
