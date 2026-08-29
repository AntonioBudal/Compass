using Compass.Modules.Calendar.Domain.Exceptions;
using Compass.Modules.Calendar.Domain.Model;
using FluentAssertions;
using Xunit;

namespace Compass.Modules.Calendar.Domain.UnitTests;

public class TimeZoneIdTests
{
    [Theory]
    [InlineData("America/Sao_Paulo")]
    [InlineData("UTC")]
    [InlineData("Europe/London")]
    [InlineData("America/New_York")]
    [InlineData("Asia/Tokyo")]
    public void Constructor_WithValidIanaTimeZone_ShouldCreateInstance(string validTimeZone)
    {
        // Act
        var tz = new TimeZoneId(validTimeZone);

        // Assert
        tz.Value.Should().Be(validTimeZone);
        tz.ToTimeZoneInfo().Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Invalid/Timezone_Name_123")]
    [InlineData("Mars/Olympus_Mons")]
    public void Constructor_WithInvalidOrEmptyTimeZone_ShouldThrowCalendarDomainException(string invalidTimeZone)
    {
        // Act
        var act = () => new TimeZoneId(invalidTimeZone);

        // Assert
        act.Should().Throw<CalendarDomainException>();
    }
}
