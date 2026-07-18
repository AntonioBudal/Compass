namespace Compass.Application.DTOs;

public record UpdateStatusDto(
    string NewStatus // PENDING, IN_PROGRESS, COMPLETED, ARCHIVED, BLOCKED
);