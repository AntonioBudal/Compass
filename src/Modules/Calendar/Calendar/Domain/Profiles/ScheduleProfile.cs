using System;
using System.Collections.Generic;
using Compass.SharedKernel.Domain.Exceptions;

namespace Compass.Modules.Calendar.Domain.Profiles;

public class ScheduleProfile
{
    public Guid Id { get; private set; }
    public string Timezone { get; private set; } = string.Empty;
    
    private readonly Dictionary<DayOfWeek, DaySchedule> _weeklySchedule = new();
    public IReadOnlyDictionary<DayOfWeek, DaySchedule> WeeklySchedule => _weeklySchedule;

    private ScheduleProfile() { } // Requisito ORM

    public ScheduleProfile(Guid id, string timezone)
    {
        Id = id;
        SetTimezone(timezone);
    }

    public void SetTimezone(string timezone)
    {
        try
        {
            // Valida se o SO reconhece a string como um fuso horário válido (IANA ou Windows)
            TimeZoneInfo.FindSystemTimeZoneById(timezone);
        }
        catch (TimeZoneNotFoundException)
        {
            throw new DomainException($"The timezone '{timezone}' is invalid or not found.");
        }

        Timezone = timezone;
    }

    public void UpdateWeeklySchedule(Dictionary<DayOfWeek, DaySchedule> schedule)
    {
        _weeklySchedule.Clear();
        foreach (var kvp in schedule)
        {
            _weeklySchedule[kvp.Key] = kvp.Value;
        }
    }
}
