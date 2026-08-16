namespace Compass.Modules.Planning.Application.Tasks.Queries;

public sealed record TaskDetailsDto(
    Guid Id,
    string Title,
    string Status,
    int? EstimatedDurationMinutes,
    DateTimeOffset? HardDeadline,
    Guid? ProjectId);

