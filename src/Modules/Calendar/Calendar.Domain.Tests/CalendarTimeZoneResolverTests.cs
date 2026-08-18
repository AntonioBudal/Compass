using System;
using Compass.SharedKernel.Domain.Exceptions;
using Compass.Modules.Calendar.Domain.Availability;
using Xunit;

namespace Compass.Modules.Calendar.Domain.Tests;

public class CalendarTimeZoneResolverTests
{
    [Fact]
    public void ResolveToUtc_With_UTC_Should_Keep_Offset_Zero()
    {
        var result = CalendarTimeZoneResolver.ResolveToUtc(new DateOnly(2026, 8, 17), new TimeSpan(8, 0, 0), "UTC");
        Assert.Equal(new DateTimeOffset(2026, 8, 17, 8, 0, 0, TimeSpan.Zero), result);
        Assert.Equal(TimeSpan.Zero, result.Offset);
    }

    [Fact]
    public void ResolveToUtc_With_America_Sao_Paulo_Should_Apply_Correct_Offset()
    {
        // 08:00 em SP = 11:00 UTC
        var result = CalendarTimeZoneResolver.ResolveToUtc(new DateOnly(2026, 8, 17), new TimeSpan(8, 0, 0), "America/Sao_Paulo");
        Assert.Equal(new DateTimeOffset(2026, 8, 17, 11, 0, 0, TimeSpan.Zero), result);
        Assert.Equal(TimeSpan.Zero, result.Offset);
    }

    [Fact]
    public void ResolveToUtc_With_Invalid_Time_SpringForward_Should_Throw()
    {
        // New York spring forward: March 8, 2026. 02:00 skips to 03:00.
        // So 02:30 is invalid.
        var ex = Assert.Throws<DomainException>(() => 
            CalendarTimeZoneResolver.ResolveToUtc(new DateOnly(2026, 3, 8), new TimeSpan(2, 30, 0), "America/New_York"));
        Assert.Contains("invalid", ex.Message);
    }

    [Fact]
    public void ResolveToUtc_With_Ambiguous_Time_FallBack_Should_Use_Standard_Offset()
    {
        // New York fall back: Nov 1, 2026. 02:00 goes back to 01:00.
        // 01:30 happens twice (EDT -04:00 and EST -05:00).
        // Our policy says pick Standard (-05:00).
        var result = CalendarTimeZoneResolver.ResolveToUtc(new DateOnly(2026, 11, 1), new TimeSpan(1, 30, 0), "America/New_York");
        // Local: 01:30. Offset: -05:00. UTC: 06:30Z.
        Assert.Equal(new DateTimeOffset(2026, 11, 1, 6, 30, 0, TimeSpan.Zero), result);
        Assert.Equal(TimeSpan.Zero, result.Offset);
    }

    [Fact]
    public void ResolveToUtc_With_Invalid_Timezone_Should_Throw()
    {
        Assert.Throws<DomainException>(() => 
            CalendarTimeZoneResolver.ResolveToUtc(new DateOnly(2026, 8, 17), new TimeSpan(8, 0, 0), "Mars/City"));
    }
}
