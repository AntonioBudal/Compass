namespace Compass.Modules.Calendar.Contracts.DTOs;

public sealed record TimeWindowDto(TimeOnly StartTime, TimeOnly EndTime);

public sealed record DayAvailabilityDto(DayOfWeek DayOfWeek, IReadOnlyList<TimeWindowDto> Windows);

public sealed record ScheduleProfileDto(
    Guid Id,
    string TimeZoneId,
    IReadOnlyList<DayAvailabilityDto> WeeklyAvailability,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public sealed record TimeZoneItemDto(
    string Id,
    string DisplayName,
    TimeSpan BaseUtcOffset
);
