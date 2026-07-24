namespace Compass.Application.DTOs.Analytics;

public record DailyTimeSliceDto(
    string DateIso, // Formato YYYY-MM-DD (resolvido no fuso local do operador)
    int CompletedCount,
    int EstimatedMinutes,
    int ActualMinutes,
    int DeepWorkMinutes,
    int PostponedCount
);