using System;
using Compass.Modules.Execution.Domain.DailyCycles;

namespace Compass.Modules.Execution.Application.DailyCycles.RecordExecution;

public sealed record RecordExecutionCommand(
    Guid DailyCycleId,
    Guid? ReferenceId, // <-- Agora recebe nulo
    DateTimeOffset Start,
    DateTimeOffset End,
    ExecutionType Type);