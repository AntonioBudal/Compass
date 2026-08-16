using System;
using System.Collections.Generic;
using System.Linq;
using Compass.SharedKernel.Domain.Exceptions;

namespace Compass.Modules.Execution.Domain.Time;

public record TimeInterval
{
    public DateTimeOffset Start { get; }
    public DateTimeOffset End { get; }

    public TimeInterval(DateTimeOffset start, DateTimeOffset end)
    {
        if (end <= start)
            throw new DomainException("Time interval must have an end time strictly greater than its start time.");
            
        Start = start;
        End = end;
    }

    public bool OverlapsWith(TimeInterval other)
    {
        return Start < other.End && other.Start < End;
    }

    public IReadOnlyList<TimeInterval> Subtract(TimeInterval other)
    {
        if (!OverlapsWith(other)) return new[] { this };

        var result = new List<TimeInterval>();

        if (Start < other.Start)
        {
            result.Add(new TimeInterval(Start, other.Start));
        }

        if (End > other.End)
        {
            result.Add(new TimeInterval(other.End, End));
        }

        return result;
    }

    public static IReadOnlyList<TimeInterval> Merge(IEnumerable<TimeInterval> intervals)
    {
        var sorted = intervals.OrderBy(i => i.Start).ToList();
        if (!sorted.Any()) return new List<TimeInterval>();

        var merged = new List<TimeInterval>();
        var current = sorted[0];

        for (int i = 1; i < sorted.Count; i++)
        {
            var next = sorted[i];
            
            if (current.End >= next.Start)
            {
                var maxEnd = current.End > next.End ? current.End : next.End;
                current = new TimeInterval(current.Start, maxEnd);
            }
            else
            {
                merged.Add(current);
                current = next;
            }
        }
        merged.Add(current);
        
        return merged;
    }
}
