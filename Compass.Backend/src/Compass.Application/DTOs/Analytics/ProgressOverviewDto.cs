namespace Compass.Application.DTOs.Analytics;

public record ProgressOverviewDto(
    int TotalCompleted,
    int TotalPlanned,
    double CompletionRatePercentage,
    double EstimationAccuracyIndex,
    bool HasImputedAccuracyData,
    int TotalDeepWorkMinutes,
    int TotalUsefulMinutes,
    int TotalPostponements,
    DateTime PeriodStartDateUtc,
    DateTime PeriodEndDateUtc
);