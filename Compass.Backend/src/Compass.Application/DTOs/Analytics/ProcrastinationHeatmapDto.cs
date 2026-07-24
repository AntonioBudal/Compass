namespace Compass.Application.DTOs.Analytics;

public record ProcrastinationHeatmapDto(
    string CommitmentType, // TASK, HABIT, EVENT, NOTE
    short EnergyRequired,  // 1, 2 ou 3
    int TotalCount,
    int PostponedCount,
    double PostponementRatePercentage
);