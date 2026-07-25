namespace Compass.Application.DTOs.Projects;

public record ProjectCatalogDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime? LastUsedAtUtc
);