using System;
using Compass.Modules.Execution.Domain.DailyCycles;

namespace Compass.Modules.Execution.Application.DailyCycles.RecordExecution;

public record RecordExecutionCommand(
    Guid DailyCycleId, 
    Guid ReferenceId, 
    DateTimeOffset Start, 
    DateTimeOffset End, 
    ExecutionType Type);
