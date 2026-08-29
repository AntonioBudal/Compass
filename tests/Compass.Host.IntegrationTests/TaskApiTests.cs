using System.Net;
using System.Net.Http.Json;
using Compass.Modules.Planning.Contracts.DTOs;
using FluentAssertions;
using Xunit;

namespace Compass.Host.IntegrationTests;

public class TaskApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TaskApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostTask_WithoutEstimate_ShouldReturn201WithDraftStatus()
    {
        // Arrange
        var request = new CreateTaskRequest(
            Title: "Comprar teclado mecânico",
            Description: "Switch brown",
            DurationMinutes: null,
            Deadline: null
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/planning/tasks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<TaskDto>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Title.Should().Be("Comprar teclado mecânico");
        created.Status.Should().Be("Draft");
        created.DurationMinutes.Should().BeNull();

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"/api/planning/tasks/{created.Id}");
    }

    [Fact]
    public async Task PostTask_WithEstimate_ShouldReturn201WithReadyStatus()
    {
        // Arrange
        var request = new CreateTaskRequest(
            Title: "Escrever testes de integração",
            Description: null,
            DurationMinutes: 60,
            Deadline: DateTimeOffset.UtcNow.AddDays(1)
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/planning/tasks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<TaskDto>();
        created.Should().NotBeNull();
        created!.Status.Should().Be("Ready");
        created.DurationMinutes.Should().Be(60);
    }

    [Fact]
    public async Task GetTasks_ShouldReturnListOfTasks()
    {
        // Arrange
        var uniqueTitle = $"Task-{Guid.NewGuid():N}";
        await _client.PostAsJsonAsync("/api/planning/tasks", new CreateTaskRequest(uniqueTitle, null, null, null));

        // Act
        var response = await _client.GetAsync("/api/planning/tasks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
        tasks.Should().NotBeNull();
        tasks!.Should().Contain(t => t.Title == uniqueTitle);
    }
}
