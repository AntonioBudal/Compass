namespace Compass.Application.DTOs;

public record CreateCommitmentDto(
    string Title,
    string Type, // TASK, EVENT, HABIT, NOTE
    Guid? ProjectId = null,
    int? EstimatedDurationMinutes = null,
    short? EnergyRequired = null,
    DateTime? Deadline = null,
    DateTime? StartTime = null,
    DateTime? EndTime = null,
    string? LocationOrLink = null,
    string? CronExpression = null,
    string? Content = null
);