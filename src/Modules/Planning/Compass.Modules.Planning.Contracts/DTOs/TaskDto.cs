namespace Compass.Modules.Planning.Contracts.DTOs;

public sealed record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    int? DurationMinutes,
    DateTimeOffset? Deadline,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? CompletedAt
);

public sealed record CreateTaskRequest(
    string Title,
    string? Description,
    int? DurationMinutes,
    DateTimeOffset? Deadline
);

public sealed record UpdateTaskRequest(
    string Title,
    string? Description,
    int? DurationMinutes,
    DateTimeOffset? Deadline
);

public sealed record SetTaskEstimateRequest(
    int? DurationMinutes
);
