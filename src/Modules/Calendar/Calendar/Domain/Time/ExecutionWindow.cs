using System;
using Compass.SharedKernel.Domain.Exceptions;

namespace Compass.Modules.Calendar.Domain.Time;

public record ExecutionWindow
{
    public TimeInterval Interval { get; }
    
    public TimeSpan Duration => Interval.End - Interval.Start;

    public ExecutionWindow(TimeInterval interval)
    {
        Interval = interval ?? throw new ArgumentNullException(nameof(interval));
    }
}
