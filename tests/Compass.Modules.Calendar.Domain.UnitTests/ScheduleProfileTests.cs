using Compass.Modules.Calendar.Domain.Model;
using FluentAssertions;
using Xunit;

namespace Compass.Modules.Calendar.Domain.UnitTests;

public class ScheduleProfileTests
{
    [Fact]
    public void Create_WithValidTimeZoneAndAvailability_ShouldInitializeCorrectly()
    {
        // Arrange
        var tz = new TimeZoneId("America/Sao_Paulo");
        var window1 = new TimeWindow(new TimeOnly(9, 0), new TimeOnly(12, 0));
        var window2 = new TimeWindow(new TimeOnly(13, 0), new TimeOnly(18, 0));
        var ruleMonday = new DayAvailabilityRule(DayOfWeek.Monday, [window1, window2]);

        var fixedTime = new DateTimeOffset(2026, 8, 28, 22, 30, 0, TimeSpan.Zero);

        // Act
        var profile = ScheduleProfile.Create(tz, [ruleMonday], fixedTime);

        // Assert
        profile.Id.Should().NotBeEmpty();
        profile.TimeZone.Value.Should().Be("America/Sao_Paulo");
        profile.CreatedAt.Should().Be(fixedTime);
        profile.UpdatedAt.Should().Be(fixedTime);
        profile.WeeklyAvailability.Should().HaveCount(1);

        var savedRule = profile.WeeklyAvailability.First();
        savedRule.ScheduleProfileId.Should().Be(profile.Id);
        savedRule.DayOfWeek.Should().Be(DayOfWeek.Monday);
        savedRule.Windows.Should().HaveCount(2);
    }

    [Fact]
    public void Create_WithoutAvailability_ShouldInitializeWithEmptyList()
    {
        // Arrange
        var tz = new TimeZoneId("UTC");

        // Act
        var profile = ScheduleProfile.Create(tz, null);

        // Assert
        profile.Id.Should().NotBeEmpty();
        profile.TimeZone.Value.Should().Be("UTC");
        profile.WeeklyAvailability.Should().BeEmpty();
    }
}
