namespace Compass.Application.DTOs.Projects;

public record CreateProjectDto(string Name, Guid? GoalId);