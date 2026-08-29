using System.Net;
using System.Net.Http.Json;
using Compass.Modules.Calendar.Application.Commands;
using Compass.Modules.Calendar.Contracts.DTOs;
using FluentAssertions;
using Xunit;

namespace Compass.Host.IntegrationTests;

public class CreateScheduleProfileApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CreateScheduleProfileApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostScheduleProfile_WithValidPayload_ShouldReturn201CreatedAndLocationHeader()
    {
        // Arrange
        var command = new CreateScheduleProfileCommand(
            "America/Sao_Paulo",
            [
                new DayAvailabilityDto(
                    DayOfWeek.Monday,
                    [
                        new TimeWindowDto(new TimeOnly(9, 0), new TimeOnly(12, 0)),
                        new TimeWindowDto(new TimeOnly(13, 0), new TimeOnly(18, 0))
                    ]
                ),
                new DayAvailabilityDto(
                    DayOfWeek.Tuesday,
                    [
                        new TimeWindowDto(new TimeOnly(9, 0), new TimeOnly(18, 0))
                    ]
                )
            ]
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/calendar/schedule-profiles", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var body = await response.Content.ReadFromJsonAsync<ScheduleProfileDto>();
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.TimeZoneId.Should().Be("America/Sao_Paulo");
        body.WeeklyAvailability.Should().HaveCount(2);

        response.Headers.Location!.ToString().Should().Contain(body.Id.ToString());
    }

    [Fact]
    public async Task GetSupportedTimeZones_ShouldReturn200WithListOfZones()
    {
        // Act
        var response = await _client.GetAsync("/api/calendar/timezones");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var timezones = await response.Content.ReadFromJsonAsync<List<TimeZoneItemDto>>();
        timezones.Should().NotBeNull();
        timezones.Should().NotBeEmpty();
        timezones!.Should().Contain(z => z.Id == "America/Sao_Paulo" || z.Id == "UTC");
    }
}
