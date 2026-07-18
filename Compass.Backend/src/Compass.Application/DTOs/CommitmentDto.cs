namespace Compass.Application.DTOs;

public record CommitmentDto(
    Guid Id,
    string Title,
    string Type,
    string Status,
    int EstimatedDurationMinutes,
    short EnergyRequired,
    DateTime? Deadline,
    DateTime? StartTime,
    DateTime? EndTime,
    string? LocationOrLink,
    string? CronExpression,
    int CurrentStreak,
    int BestStreak,
    int PostponedCount,
    string? Content,
    Guid? ProjectId,
    string? ProjectName
);