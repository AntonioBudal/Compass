namespace Compass.Application.DTOs;

public record UpdateCommitmentDto(
    string Title,
    Guid? ProjectId,
    int? EstimatedDurationMinutes,
    short? EnergyRequired,
    DateTime? Deadline,
    string? CronExpression,
    DateTime? StartTime,
    DateTime? EndTime,
    string? LocationOrLink,
    string? Content
);