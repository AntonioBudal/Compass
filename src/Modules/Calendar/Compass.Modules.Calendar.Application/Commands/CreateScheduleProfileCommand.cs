using Compass.Modules.Calendar.Application.Abstractions;
using Compass.Modules.Calendar.Contracts.DTOs;

namespace Compass.Modules.Calendar.Application.Commands;

public sealed record CreateScheduleProfileCommand(
    string TimeZoneId,
    IReadOnlyList<DayAvailabilityDto>? WeeklyAvailability
) : ICommand<ScheduleProfileDto>;
