namespace Compass.Application.DTOs.Analytics;

public record ChronologyPeakDto(
    string TimeBucket, // "Morning" (06-12h), "Afternoon" (12-18h), "Evening" (18-00h), "Night" (00-06h)
    int CompletedCount,
    int DeepWorkMinutes,
    double EfficiencyPercentage
);