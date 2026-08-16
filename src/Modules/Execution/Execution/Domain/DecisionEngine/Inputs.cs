using System;
using Compass.Modules.Execution.Domain.Time;

namespace Compass.Modules.Execution.Domain.DecisionEngine;

public enum TaskPriority { Low = 1, Medium = 2, High = 3 }

public record TaskCandidate(Guid Id, TaskPriority Priority, TimeSpan RemainingDuration);

public record AvailableSlot(TimeInterval Interval);

public record ExecutionHistory(System.Collections.Generic.IEnumerable<TimeInterval> Intervals);
