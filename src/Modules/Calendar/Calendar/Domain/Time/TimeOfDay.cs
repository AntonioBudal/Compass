using System;
using Compass.SharedKernel.Domain.Exceptions;

namespace Compass.Modules.Calendar.Domain.Time;

public record TimeOfDay : IComparable<TimeOfDay>
{
    public int Hour { get; }
    public int Minute { get; }

    public TimeOfDay(int hour, int minute)
    {
        if (hour < 0 || hour > 23) throw new DomainException("Hour must be between 0 and 23.");
        if (minute < 0 || minute > 59) throw new DomainException("Minute must be between 0 and 59.");
        
        Hour = hour;
        Minute = minute;
    }

    public int CompareTo(TimeOfDay? other)
    {
        if (other is null) return 1;
        var thisMinutes = Hour * 60 + Minute;
        var otherMinutes = other.Hour * 60 + other.Minute;
        return thisMinutes.CompareTo(otherMinutes);
    }

    public static bool operator <(TimeOfDay left, TimeOfDay right) => left.CompareTo(right) < 0;
    public static bool operator >(TimeOfDay left, TimeOfDay right) => left.CompareTo(right) > 0;
    public static bool operator <=(TimeOfDay left, TimeOfDay right) => left.CompareTo(right) <= 0;
    public static bool operator >=(TimeOfDay left, TimeOfDay right) => left.CompareTo(right) >= 0;
}
