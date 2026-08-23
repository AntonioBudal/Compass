using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Calendar.Application.Profiles.Queries;
using Compass.Modules.Calendar.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Compass.Modules.Calendar.Infrastructure.Queries;

internal sealed class ScheduleProfileQueryService : IScheduleProfileQueryService
{
    private readonly CalendarDbContext _dbContext;

    public ScheduleProfileQueryService(CalendarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ScheduleProfileDetailsDto?> GetProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var profile = await _dbContext.ScheduleProfiles
            .AsNoTracking()
            .Where(p => p.Id == profileId)
            .Select(p => new { p.Id, p.Timezone })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return null;
        }

        var windowsData = await _dbContext.ScheduleWindows
            .AsNoTracking()
            .Where(w => w.ScheduleProfileId == profileId)
            .Select(w => new { w.Id, w.DayOfWeek, w.StartTime, w.EndTime })
            .ToListAsync(cancellationToken);

        // O ".ToString()" garante o cast correto da string ou Enum para o DTO sem estourar no LINQ-to-SQL
        var windows = windowsData
            .Select(w => new ScheduleWindowDto(w.Id, w.DayOfWeek.ToString()!, w.StartTime, w.EndTime))
            .ToList();

        return new ScheduleProfileDetailsDto(profile.Id, profile.Timezone, windows);
    }
}