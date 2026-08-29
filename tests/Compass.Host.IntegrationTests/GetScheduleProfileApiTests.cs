using System.Net;
using System.Net.Http.Json;
using Compass.Modules.Calendar.Application.Commands;
using Compass.Modules.Calendar.Contracts.DTOs;
using FluentAssertions;
using Xunit;

namespace Compass.Host.IntegrationTests;

public class GetScheduleProfileApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GetScheduleProfileApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetScheduleProfileById_WhenProfileExists_ShouldReturn200OK()
    {
        // Arrange - Create profile first
        var command = new CreateScheduleProfileCommand(
            "America/Sao_Paulo",
            [
                new DayAvailabilityDto(
                    DayOfWeek.Wednesday,
                    [new TimeWindowDto(new TimeOnly(8, 30), new TimeOnly(17, 30))]
                )
            ]
        );

        var createResponse = await _client.PostAsJsonAsync("/api/calendar/schedule-profiles", command);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ScheduleProfileDto>();

        // Act
        var getResponse = await _client.GetAsync($"/api/calendar/schedule-profiles/{created!.Id}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<ScheduleProfileDto>();
        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(created.Id);
        fetched.TimeZoneId.Should().Be("America/Sao_Paulo");
        fetched.WeeklyAvailability.Should().HaveCount(1);
        fetched.WeeklyAvailability[0].DayOfWeek.Should().Be(DayOfWeek.Wednesday);
    }

    [Fact]
    public async Task GetScheduleProfileById_WhenProfileDoesNotExist_ShouldReturn404NotFound()
    {
        // Arrange
        var nonExistentId = Guid.CreateVersion7();

        // Act
        var response = await _client.GetAsync($"/api/calendar/schedule-profiles/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
