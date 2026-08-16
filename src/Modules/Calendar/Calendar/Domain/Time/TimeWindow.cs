using Compass.SharedKernel.Domain.Exceptions;

namespace Compass.Modules.Calendar.Domain.Time;

public record TimeWindow
{
    public TimeOfDay Start { get; }
    public TimeOfDay End { get; }

    public TimeWindow(TimeOfDay start, TimeOfDay end)
    {
        if (start >= end)
        {
            throw new DomainException("A time window's start time must be strictly before its end time.");
        }
        
        Start = start;
        End = end;
    }

    public bool OverlapsWith(TimeWindow other)
    {
        // Fronteiras encostadas (ex: 10h-12h e 12h-14h) NÃO se sobrepõem.
        return Start < other.End && other.Start < End;
    }
}
