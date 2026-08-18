using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Calendar.Application.Profiles;
using Compass.Modules.Calendar.Application.Profiles.Commands;
using Compass.Modules.Calendar.Domain.Profiles;
using Compass.Modules.Calendar.Domain.Time;
using Compass.SharedKernel.Domain.Exceptions;

namespace Compass.Modules.Calendar.Infrastructure.Commands;

internal sealed class ScheduleProfileCommandService
    : IScheduleProfileCommandService
{
    private readonly IScheduleProfileRepository _repository;

    public ScheduleProfileCommandService(
        IScheduleProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task CreateProfileAsync(
        Guid profileId,
        CreateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (profileId == Guid.Empty)
        {
            throw new DomainException(
                "ScheduleProfile ID cannot be empty.");
        }

        if (request.WeeklySchedule is null)
        {
            throw new DomainException(
                "Weekly schedule is required.");
        }

        var profile = new ScheduleProfile(
            profileId,
            request.Timezone);

        var domainSchedule =
            new Dictionary<DayOfWeek, DaySchedule>();

        foreach (var dayEntry in request.WeeklySchedule)
        {
            if (string.IsNullOrWhiteSpace(dayEntry.Key) ||
                !Enum.TryParse<DayOfWeek>(
                    dayEntry.Key,
                    ignoreCase: true,
                    out var dayOfWeek) ||
                !Enum.IsDefined(
                    typeof(DayOfWeek),
                    dayOfWeek))
            {
                throw new DomainException(
                    $"Invalid day of week: '{dayEntry.Key}'.");
            }

            if (dayEntry.Value is null)
            {
                throw new DomainException(
                    $"Windows are required for '{dayEntry.Key}'.");
            }

            var windows = dayEntry.Value
                .Select(window =>
                {
                    var start = ParseTime(
                        window.StartTime,
                        "start");

                    var end = ParseTime(
                        window.EndTime,
                        "end");

                    return new TimeWindow(
                        new TimeOfDay(
                            start.Hours,
                            start.Minutes),
                        new TimeOfDay(
                            end.Hours,
                            end.Minutes));
                })
                .ToList();

            domainSchedule[dayOfWeek] =
                new DaySchedule(windows);
        }

        profile.UpdateWeeklySchedule(domainSchedule);

        await _repository.AddAsync(
            profile,
            cancellationToken);
    }

    private static TimeSpan ParseTime(
        string value,
        string fieldName)
    {
        if (!TimeSpan.TryParseExact(
                value,
                @"hh\:mm",
                CultureInfo.InvariantCulture,
                out var parsed) ||
            parsed < TimeSpan.Zero ||
            parsed >= TimeSpan.FromDays(1))
        {
            throw new DomainException(
                $"Invalid {fieldName} time: '{value}'. Expected HH:mm.");
        }

        return parsed;
    }
}
