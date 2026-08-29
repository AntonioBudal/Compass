using Compass.Modules.Calendar.Domain.Exceptions;

namespace Compass.Modules.Calendar.Domain.Model;

public sealed record TimeWindow
{
    public TimeOnly StartTime { get; }
    public TimeOnly EndTime { get; }

    public TimeSpan Duration => EndTime - StartTime;

    public TimeWindow(TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime >= endTime)
        {
            throw new CalendarDomainException($"O horário inicial ({startTime:HH\\:mm}) deve ser estritamente anterior ao horário final ({endTime:HH\\:mm}).");
        }

        StartTime = startTime;
        EndTime = endTime;
    }

    public bool Contains(TimeOnly time)
    {
        return time >= StartTime && time < EndTime;
    }

    public bool OverlapsOrContiguousWith(TimeWindow other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return StartTime <= other.EndTime && other.StartTime <= EndTime;
    }

    public TimeWindow Merge(TimeWindow other)
    {
        if (!OverlapsOrContiguousWith(other))
        {
            throw new CalendarDomainException("Não é possível unificar intervalos que não se sobrepõem e não são contíguos.");
        }

        var minStart = StartTime < other.StartTime ? StartTime : other.StartTime;
        var maxEnd = EndTime > other.EndTime ? EndTime : other.EndTime;

        return new TimeWindow(minStart, maxEnd);
    }

    public static IReadOnlyList<TimeWindow> Normalize(IEnumerable<TimeWindow>? windows)
    {
        if (windows == null)
        {
            return Array.Empty<TimeWindow>();
        }

        var ordered = windows.OrderBy(w => w.StartTime).ThenBy(w => w.EndTime).ToList();
        if (ordered.Count <= 1)
        {
            return ordered;
        }

        var merged = new List<TimeWindow>();
        var current = ordered[0];

        for (int i = 1; i < ordered.Count; i++)
        {
            var next = ordered[i];
            if (current.OverlapsOrContiguousWith(next))
            {
                current = current.Merge(next);
            }
            else
            {
                merged.Add(current);
                current = next;
            }
        }

        merged.Add(current);
        return merged.AsReadOnly();
    }
}
