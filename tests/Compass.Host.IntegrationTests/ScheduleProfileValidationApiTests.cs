using System.Net;
using System.Net.Http.Json;
using Compass.Modules.Calendar.Application.Commands;
using Compass.Modules.Calendar.Contracts.DTOs;
using FluentAssertions;
using Xunit;

namespace Compass.Host.IntegrationTests;

public class ScheduleProfileValidationApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ScheduleProfileValidationApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostScheduleProfile_WithInvalidTimeZone_ShouldReturn400BadRequest()
    {
        // Arrange
        var command = new CreateScheduleProfileCommand(
            "Invalid/Timezone_123",
            [
                new DayAvailabilityDto(
                    DayOfWeek.Monday,
                    [new TimeWindowDto(new TimeOnly(9, 0), new TimeOnly(18, 0))]
                )
            ]
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/calendar/schedule-profiles", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostScheduleProfile_WithInvertedTimeWindow_ShouldReturn400BadRequest()
    {
        // Arrange
        var command = new CreateScheduleProfileCommand(
            "America/Sao_Paulo",
            [
                new DayAvailabilityDto(
                    DayOfWeek.Monday,
                    [new TimeWindowDto(new TimeOnly(18, 0), new TimeOnly(9, 0))]
                )
            ]
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/calendar/schedule-profiles", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostScheduleProfile_WithOverlappingWindows_ShouldUnifyIntoSingleWindowInResponse()
    {
        // Arrange: 09:00-12:00 e 11:00-17:00
        var command = new CreateScheduleProfileCommand(
            "America/Sao_Paulo",
            [
                new DayAvailabilityDto(
                    DayOfWeek.Thursday,
                    [
                        new TimeWindowDto(new TimeOnly(9, 0), new TimeOnly(12, 0)),
                        new TimeWindowDto(new TimeOnly(11, 0), new TimeOnly(17, 0))
                    ]
                )
            ]
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/calendar/schedule-profiles", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ScheduleProfileDto>();
        created.Should().NotBeNull();
        created!.WeeklyAvailability.Should().HaveCount(1);

        var thursdayRule = created.WeeklyAvailability.First();
        thursdayRule.Windows.Should().HaveCount(1);
        thursdayRule.Windows[0].StartTime.Should().Be(new TimeOnly(9, 0));
        thursdayRule.Windows[0].EndTime.Should().Be(new TimeOnly(17, 0));
    }
}
