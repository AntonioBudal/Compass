namespace Compass.Application.DTOs.Decisions;


public record AdaptiveProfileDto(
    bool IsCalibrated,
    int SampleCount,
    double EaiMultiplier,
    double MorningEnergyBias,
    double AfternoonEnergyBias,
    double EveningEnergyBias
);

// Contrato da ação recomendada enriquecido com telemetria do motor
public record ScoredActionDto(
    Guid CommitmentId,
    string Title,
    string Type,
    int NominalDurationMinutes,
    int EffectiveDurationMinutes, // Tempo real projetado pelo EAI
    short EnergyRequired,
    double ScorePercentage,
    string Reason,
    bool WasTimeAdjustedByEai,    // Flag para ícone na UI
    string? ProjectName
);

// Resposta raiz do GET /now
public record DecisionResponseDto(
    DateTime GeneratedAtUtc,
    int AvailableWindowMinutes,
    short OperatorEnergyLevel,
    AdaptiveProfileDto AdaptiveProfile,
    IReadOnlyList<ScoredActionDto> TopActions
);