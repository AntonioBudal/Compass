using System;
using System.Linq;
using Compass.SharedKernel.Domain.Exceptions;

namespace Compass.Modules.Calendar.Domain.Availability;

public static class CalendarTimeZoneResolver
{
    public static DateTimeOffset ResolveToUtc(DateOnly date, TimeSpan time, string timezoneId)
    {
        TimeZoneInfo tz;
        try
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException || ex is InvalidTimeZoneException)
        {
            throw new DomainException($"The timezone '{timezoneId}' is invalid or not found.");
        }

        var localDt = new DateTime(date.Year, date.Month, date.Day, time.Hours, time.Minutes, time.Seconds, DateTimeKind.Unspecified);

        if (tz.IsInvalidTime(localDt))
        {
            throw new DomainException($"The time {time} on {date} is invalid in timezone {timezoneId} (skips due to DST).");
        }

        if (tz.IsAmbiguousTime(localDt))
        {
            var offsets = tz.GetAmbiguousTimeOffsets(localDt);
            // Compass Policy: Select the standard offset (the minimum/least positive offset value on the fallback pass)
            var standardOffset = offsets.Min();
            var utcTicks = localDt.Ticks - standardOffset.Ticks;
            return new DateTimeOffset(utcTicks, TimeSpan.Zero);
        }

        var offset = tz.GetUtcOffset(localDt);
        var utcTicksNormal = localDt.Ticks - offset.Ticks;
        return new DateTimeOffset(utcTicksNormal, TimeSpan.Zero);
    }
}
