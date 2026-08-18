using System;

namespace Compass.Modules.Calendar.Domain.Availability;

public record TimeWindow(TimeSpan Start, TimeSpan End)
{
    public double DurationMinutes => (End - Start).TotalMinutes;
}
