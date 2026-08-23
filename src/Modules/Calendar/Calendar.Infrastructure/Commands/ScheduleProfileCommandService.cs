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
using Compass.Modules.Calendar.Infrastructure.Database;
using Compass.SharedKernel.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Compass.Modules.Calendar.Infrastructure.Commands;

internal sealed class ScheduleProfileCommandService
    : IScheduleProfileCommandService
{
    private readonly IScheduleProfileRepository _repository;
    private readonly CalendarDbContext _dbContext;

    public ScheduleProfileCommandService(
        IScheduleProfileRepository repository,
        CalendarDbContext dbContext)
    {
        _repository = repository;
        _dbContext = dbContext;
    }

    public async Task CreateProfileAsync(
        Guid profileId,
        CreateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (profileId == Guid.Empty)
        {
            throw new DomainException("ScheduleProfile ID cannot be empty.");
        }

        if (request.WeeklySchedule is null)
        {
            throw new DomainException("Weekly schedule is required.");
        }

        var profile = new ScheduleProfile(profileId, request.Timezone);
        var domainSchedule = new Dictionary<DayOfWeek, DaySchedule>();

        foreach (var dayEntry in request.WeeklySchedule)
        {
            if (string.IsNullOrWhiteSpace(dayEntry.Key) ||
                !Enum.TryParse<DayOfWeek>(dayEntry.Key, ignoreCase: true, out var dayOfWeek) ||
                !Enum.IsDefined(typeof(DayOfWeek), dayOfWeek))
            {
                throw new DomainException($"Invalid day of week: '{dayEntry.Key}'.");
            }

            if (dayEntry.Value is null)
            {
                throw new DomainException($"Windows are required for '{dayEntry.Key}'.");
            }

            var windows = dayEntry.Value
                .Select(window =>
                {
                    var start = ParseTime(window.StartTime, "start");
                    var end = ParseTime(window.EndTime, "end");

                    return new TimeWindow(
                        new TimeOfDay(start.Hours, start.Minutes),
                        new TimeOfDay(end.Hours, end.Minutes));
                })
                .ToList();

            domainSchedule[dayOfWeek] = new DaySchedule(windows);
        }

        profile.UpdateWeeklySchedule(domainSchedule);
        await _repository.AddAsync(profile, cancellationToken);
    }

    public async Task AddWindowAsync(
        Guid profileId, 
        AddWindowRequest request, 
        CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetByIdAsync(profileId, cancellationToken);
        if (profile is null)
        {
            throw new DomainException("Profile not found.");
        }

        if (!Enum.TryParse<DayOfWeek>(request.DayOfWeek, ignoreCase: true, out var dayOfWeek) ||
            !Enum.IsDefined(typeof(DayOfWeek), dayOfWeek))
        {
            throw new DomainException($"Invalid day of week: '{request.DayOfWeek}'.");
        }

        var start = ParseTime(request.StartTime, "start");
        var end = ParseTime(request.EndTime, "end");

        var newWindow = new TimeWindow(
            new TimeOfDay(start.Hours, start.Minutes),
            new TimeOfDay(end.Hours, end.Minutes));

        var currentSchedule = profile.WeeklySchedule.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var windows = currentSchedule.TryGetValue(dayOfWeek, out var daySchedule)
            ? daySchedule.Windows.ToList()
            : new List<TimeWindow>();

        windows.Add(newWindow);
        
        // A invariante do Domínio garantirá que não ocorram sobreposições ao criar o DaySchedule
        currentSchedule[dayOfWeek] = new DaySchedule(windows);

        profile.UpdateWeeklySchedule(currentSchedule);
        await _repository.UpdateAsync(profile, cancellationToken);
    }

    public async Task RemoveWindowAsync(
        Guid profileId, 
        Guid windowId, 
        CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetByIdAsync(profileId, cancellationToken);
        if (profile is null)
        {
            throw new DomainException("Profile not found.");
        }

        // Consultamos o DB apenas para saber qual era o horário do ID antes de comandar o domínio
        var windowData = await _dbContext.ScheduleWindows
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == windowId && w.ScheduleProfileId == profileId, cancellationToken);

        if (windowData is null)
        {
            return; // Idempotente
        }

        var currentSchedule = profile.WeeklySchedule.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (currentSchedule.TryGetValue(windowData.DayOfWeek, out var daySchedule))
        {
            var targetWindow = new TimeWindow(
                new TimeOfDay(windowData.StartTime.Hours, windowData.StartTime.Minutes),
                new TimeOfDay(windowData.EndTime.Hours, windowData.EndTime.Minutes));

            var updatedWindows = daySchedule.Windows
                .Where(w => !w.Equals(targetWindow))
                .ToList();

            currentSchedule[windowData.DayOfWeek] = new DaySchedule(updatedWindows);
            profile.UpdateWeeklySchedule(currentSchedule);
            
            await _repository.UpdateAsync(profile, cancellationToken);
        }
    }

    private static TimeSpan ParseTime(string value, string fieldName)
    {
        if (!TimeSpan.TryParseExact(
                value,
                @"hh\:mm",
                CultureInfo.InvariantCulture,
                out var parsed) ||
            parsed < TimeSpan.Zero ||
            parsed >= TimeSpan.FromDays(1))
        {
            throw new DomainException($"Invalid {fieldName} time: '{value}'. Expected HH:mm.");
        }

        return parsed;
    }
}