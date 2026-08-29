using Compass.Modules.Calendar.Domain.Exceptions;
using Compass.Modules.Calendar.Domain.Model;
using FluentAssertions;
using Xunit;

namespace Compass.Modules.Calendar.Domain.UnitTests;

public class TimeWindowTests
{
    [Fact]
    public void Constructor_WhenStartTimeIsEarlierThanEndTime_ShouldCreateInstance()
    {
        // Arrange
        var start = new TimeOnly(9, 0);
        var end = new TimeOnly(17, 0);

        // Act
        var window = new TimeWindow(start, end);

        // Assert
        window.StartTime.Should().Be(start);
        window.EndTime.Should().Be(end);
        window.Duration.Should().Be(TimeSpan.FromHours(8));
    }

    [Fact]
    public void Constructor_WhenStartTimeIsGreaterThanOrEqualToEndTime_ShouldThrowCalendarDomainException()
    {
        // Act & Assert
        var actEqual = () => new TimeWindow(new TimeOnly(12, 0), new TimeOnly(12, 0));
        var actInverted = () => new TimeWindow(new TimeOnly(18, 0), new TimeOnly(9, 0));

        actEqual.Should().Throw<CalendarDomainException>()
            .WithMessage("*estritamente anterior*");

        actInverted.Should().Throw<CalendarDomainException>()
            .WithMessage("*estritamente anterior*");
    }

    [Fact]
    public void Normalize_WithOverlappingWindows_ShouldMergeIntoSingleContinuousWindow()
    {
        // Arrange: 09:00-12:00 e 11:30-15:00
        var w1 = new TimeWindow(new TimeOnly(9, 0), new TimeOnly(12, 0));
        var w2 = new TimeWindow(new TimeOnly(11, 30), new TimeOnly(15, 0));

        // Act
        var normalized = TimeWindow.Normalize([w1, w2]);

        // Assert
        normalized.Should().HaveCount(1);
        normalized[0].StartTime.Should().Be(new TimeOnly(9, 0));
        normalized[0].EndTime.Should().Be(new TimeOnly(15, 0));
    }

    [Fact]
    public void Normalize_WithContiguousWindows_ShouldMergeIntoSingleWindow()
    {
        // Arrange: 08:00-12:00 e 12:00-16:00
        var w1 = new TimeWindow(new TimeOnly(8, 0), new TimeOnly(12, 0));
        var w2 = new TimeWindow(new TimeOnly(12, 0), new TimeOnly(16, 0));

        // Act
        var normalized = TimeWindow.Normalize([w1, w2]);

        // Assert
        normalized.Should().HaveCount(1);
        normalized[0].StartTime.Should().Be(new TimeOnly(8, 0));
        normalized[0].EndTime.Should().Be(new TimeOnly(16, 0));
    }

    [Fact]
    public void Normalize_WithDisjointWindows_ShouldPreserveBothOrdered()
    {
        // Arrange: 14:00-18:00 e 09:00-12:00
        var w1 = new TimeWindow(new TimeOnly(14, 0), new TimeOnly(18, 0));
        var w2 = new TimeWindow(new TimeOnly(9, 0), new TimeOnly(12, 0));

        // Act
        var normalized = TimeWindow.Normalize([w1, w2]);

        // Assert
        normalized.Should().HaveCount(2);
        normalized[0].StartTime.Should().Be(new TimeOnly(9, 0));
        normalized[0].EndTime.Should().Be(new TimeOnly(12, 0));
        normalized[1].StartTime.Should().Be(new TimeOnly(14, 0));
        normalized[1].EndTime.Should().Be(new TimeOnly(18, 0));
    }
}
