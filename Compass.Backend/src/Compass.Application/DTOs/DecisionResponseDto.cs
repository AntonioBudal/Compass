namespace Compass.Application.DTOs;

public record DecisionContextDto(
    short EffectiveEnergy,
    int AvailableWindowMinutes,
    ActiveHardBlockerDto? ActiveHardBlocker
);

public record ActiveHardBlockerDto(
    string Title,
    DateTime StartsAt
);

public record ScoredActionDto(
    Guid Id,
    string Title,
    string Type,
    int EstimatedDurationMinutes,
    short EnergyRequired,
    string? ProjectName,
    double ScorePercentage,
    string Reason
);

public record DecisionResponseDto(
    DateTime GeneratedAt,
    DecisionContextDto Context,
    ScoredActionDto? TopFocus,
    IReadOnlyList<ScoredActionDto> Alternatives
);