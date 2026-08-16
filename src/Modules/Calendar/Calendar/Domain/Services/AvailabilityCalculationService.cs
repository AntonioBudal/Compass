using System;
using System.Collections.Generic;
using System.Linq;
using Compass.Modules.Calendar.Domain.Commitments;
using Compass.Modules.Calendar.Domain.Profiles;
using Compass.Modules.Calendar.Domain.Time;

namespace Compass.Modules.Calendar.Domain.Services;

public class AvailabilityCalculationService
{
    public IReadOnlyList<ExecutionWindow> Calculate(
        ScheduleProfile profile,
        IEnumerable<Commitment> commitments,
        TimeInterval queryWindow,
        TimeSpan minimumUsefulDuration)
    {
        // 1. Fase de Projeção
        var potentialSlots = ProjectSchedule(profile, queryWindow);

        // 2. Fase de Achatamento
        var overlappingCommitments = commitments
            .Select(c => c.Interval)
            .Where(i => i.OverlapsWith(queryWindow))
            .ToList();

        var mergedBlockings = TimeInterval.Merge(overlappingCommitments);

        // 3. Fase de Subtração
        var currentSlots = potentialSlots.ToList();
        
        foreach (var blocking in mergedBlockings)
        {
            var nextSlots = new List<TimeInterval>();
            foreach (var slot in currentSlots)
            {
                nextSlots.AddRange(slot.Subtract(blocking));
            }
            currentSlots = nextSlots;
        }

        // 4. Fase de Poda (Threshold) e Empacotamento
        return currentSlots
            .Where(slot => (slot.End - slot.Start) >= minimumUsefulDuration)
            .Select(slot => new ExecutionWindow(slot))
            .ToList()
            .AsReadOnly();
    }

    private IEnumerable<TimeInterval> ProjectSchedule(ScheduleProfile profile, TimeInterval queryWindow)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(profile.Timezone);
        var localStart = TimeZoneInfo.ConvertTime(queryWindow.Start, tz);
        var localEnd = TimeZoneInfo.ConvertTime(queryWindow.End, tz);
        var potentials = new List<TimeInterval>();

        for (var date = localStart.Date; date <= localEnd.Date; date = date.AddDays(1))
        {
            if (profile.WeeklySchedule.TryGetValue(date.DayOfWeek, out var daySchedule))
            {
                foreach (var window in daySchedule.Windows)
                {
                    var shiftStartLocal = new DateTime(date.Year, date.Month, date.Day, window.Start.Hour, window.Start.Minute, 0, DateTimeKind.Unspecified);
                    var shiftEndLocal = new DateTime(date.Year, date.Month, date.Day, window.End.Hour, window.End.Minute, 0, DateTimeKind.Unspecified);

                    var shiftStartUtc = TimeZoneInfo.ConvertTimeToUtc(shiftStartLocal, tz);
                    var shiftEndUtc = TimeZoneInfo.ConvertTimeToUtc(shiftEndLocal, tz);

                    var shiftInterval = new TimeInterval(
                        new DateTimeOffset(shiftStartUtc, TimeSpan.Zero), 
                        new DateTimeOffset(shiftEndUtc, TimeSpan.Zero)
                    );

                    var intersection = shiftInterval.Intersect(queryWindow);
                    if (intersection != null)
                    {
                        potentials.Add(intersection);
                    }
                }
            }
        }

        return potentials;
    }
}
