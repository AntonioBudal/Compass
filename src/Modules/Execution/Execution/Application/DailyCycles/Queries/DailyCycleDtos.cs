using System;
using System.Collections.Generic;

namespace Compass.Modules.Execution.Application.DailyCycles.Queries;

public sealed record ExecutionLogDto(
    Guid Id,
    Guid? ReferenceId, 
    string Type,
    DateTimeOffset Start,
    DateTimeOffset End);

public sealed record DailyCycleDetailsDto(
    Guid Id,
    DateOnly Date,
    string Status,
    IReadOnlyList<ExecutionLogDto> Logs);
