namespace Compass.Application.DTOs.DailyCycle;

public record MorningBriefingDto(
    DateOnly Date,
    int PendingTasksCount,
    int OverdueTasksCount,
    int HabitsToCheckCount,
    int TotalEstimatedFocusMinutes,
    string TopFocusTitle,
    string GreetingMessage
);

public record DailyShutdownRequestDto(
    int CompletedCount,
    int PostponedCount,
    int TotalFocusMinutes,
    string Notes,
    List<string> DivergenceTags 
);

public record DailyShutdownResponseDto(
    Guid ReviewId,
    DateOnly ReviewDate,
    string StatusMessage,
    bool AnalyticalLogUpdated
);