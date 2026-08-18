using System;

namespace Compass.Modules.Planning.Contracts.Queries;

public record WorkCandidate(
    Guid ReferenceId,
    string Title,
    string Type, // "Task" or "Habit"
    int EstimatedMinutes,
    DateTimeOffset? Deadline,
    int Priority // Menor número = maior prioridade
);
