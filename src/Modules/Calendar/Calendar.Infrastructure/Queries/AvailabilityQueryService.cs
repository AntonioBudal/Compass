using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Calendar.Application.Profiles.Queries;
using Compass.Modules.Calendar.Domain.Availability;
using Compass.Modules.Calendar.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Compass.Modules.Calendar.Infrastructure.Queries;

internal sealed class AvailabilityQueryService : IAvailabilityQueryService
{
    private readonly CalendarDbContext _dbContext;
    private readonly AvailabilityCalculator _calculator;

    public AvailabilityQueryService(CalendarDbContext dbContext)
    {
        _dbContext = dbContext;
        _calculator = new AvailabilityCalculator();
    }

    public async Task<DailyAvailabilityDto?> GetAvailabilityAsync(Guid profileId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var profile = await _dbContext.ScheduleProfiles
            .Include(p => p.Windows)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == profileId, cancellationToken);

        if (profile == null) return null; // Delega o retorno vazio para a ponta

        var baseWindows = profile.Windows
            .Where(w => w.DayOfWeek == date.DayOfWeek)
            .Select(w => new Compass.Modules.Calendar.Domain.Availability.TimeWindow(w.StartTime, w.EndTime))
            .ToList();

        if (!baseWindows.Any())
            return new DailyAvailabilityDto(profileId, profile.Timezone, date, new List<TimeWindowDto>());

        // Busca as exceções apenas da data selecionada
        var exceptions = await _dbContext.ScheduleExceptions
            .Where(e => e.ScheduleProfileId == profileId && e.Date == date)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var blockedWindows = exceptions.Select(e => new Compass.Modules.Calendar.Domain.Availability.TimeWindow(e.StartTime, e.EndTime)).ToList();
        // Busca Commitments que intersectam o dia civil do Profile
        var tz = TimeZoneInfo.FindSystemTimeZoneById(profile.Timezone);
        var localDayStart = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified);
        var localDayEnd = localDayStart.AddDays(1);
        
        var utcDayStart = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localDayStart, tz), TimeSpan.Zero);
        var utcDayEnd = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localDayEnd, tz), TimeSpan.Zero);

        var commitmentsData = await _dbContext.Commitments
            .Where(c => c.ScheduleProfileId == profileId && c.StartTime < utcDayEnd && c.EndTime > utcDayStart)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        foreach (var com in commitmentsData)
        {
            var comLocalStart = TimeZoneInfo.ConvertTime(com.StartTime, tz);
            var comLocalEnd = TimeZoneInfo.ConvertTime(com.EndTime, tz);

            // Se o commitment pertencer a este dia civil, adiciona como TimeWindow local
            if (comLocalStart.Date == date.ToDateTime(TimeOnly.MinValue) || comLocalEnd.Date == date.ToDateTime(TimeOnly.MinValue))
            {
                var startSpan = comLocalStart.TimeOfDay;
                var endSpan = comLocalEnd.TimeOfDay;
                blockedWindows.Add(new Compass.Modules.Calendar.Domain.Availability.TimeWindow(startSpan, endSpan));
            }
        }

        var calculatedWindows = _calculator.Calculate(baseWindows, blockedWindows);

        var windowDtos = calculatedWindows.Select(w => new TimeWindowDto(w.Start, w.End)).ToList();
        return new DailyAvailabilityDto(profileId, profile.Timezone, date, windowDtos);
    }
}



