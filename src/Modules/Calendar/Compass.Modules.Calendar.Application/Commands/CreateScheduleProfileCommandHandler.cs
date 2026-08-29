using Compass.Modules.Calendar.Application.Abstractions;
using Compass.Modules.Calendar.Contracts.DTOs;
using Compass.Modules.Calendar.Domain.Model;
using Compass.Modules.Calendar.Domain.Repositories;

namespace Compass.Modules.Calendar.Application.Commands;

public class CreateScheduleProfileCommandHandler : ICommandHandler<CreateScheduleProfileCommand, ScheduleProfileDto>
{
    private readonly IScheduleProfileRepository _repository;

    public CreateScheduleProfileCommandHandler(IScheduleProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<ScheduleProfileDto> HandleAsync(CreateScheduleProfileCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var timeZone = new TimeZoneId(command.TimeZoneId);

        List<DayAvailabilityRule>? rules = null;
        if (command.WeeklyAvailability != null)
        {
            rules = command.WeeklyAvailability.Select(dto =>
            {
                var windows = dto.Windows?.Select(w => new TimeWindow(w.StartTime, w.EndTime)) ?? [];
                return new DayAvailabilityRule(dto.DayOfWeek, windows);
            }).ToList();
        }

        var profile = ScheduleProfile.Create(timeZone, rules);

        await _repository.AddAsync(profile, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return MapToDto(profile);
    }

    public static ScheduleProfileDto MapToDto(ScheduleProfile profile)
    {
        var weekly = profile.WeeklyAvailability.Select(rule => new DayAvailabilityDto(
            rule.DayOfWeek,
            rule.Windows.Select(w => new TimeWindowDto(w.StartTime, w.EndTime)).ToList()
        )).ToList();

        return new ScheduleProfileDto(
            profile.Id,
            profile.TimeZone.Value,
            weekly,
            profile.CreatedAt,
            profile.UpdatedAt
        );
    }
}
