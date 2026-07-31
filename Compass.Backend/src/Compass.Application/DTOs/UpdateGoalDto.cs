namespace Compass.Application.DTOs;

public record UpdateGoalDto(
    string Title, 
    string? WhyDescription, 
    DateTime? TargetDate
);