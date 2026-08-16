using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Calendar.Application.Profiles;
using Compass.Modules.Calendar.Domain.Profiles;
using Compass.Modules.Calendar.Domain.Time;
using Compass.Modules.Calendar.Infrastructure.Database;
using Compass.Modules.Calendar.Infrastructure.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Compass.Modules.Calendar.Infrastructure.Repositories;

internal class EfScheduleProfileRepository : IScheduleProfileRepository
{
    private readonly CalendarDbContext _dbContext;

    public EfScheduleProfileRepository(CalendarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ScheduleProfile profile, CancellationToken cancellationToken = default)
    {
        var dataModel = MapToDataModel(profile);
        await _dbContext.ScheduleProfiles.AddAsync(dataModel, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ScheduleProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dataModel = await _dbContext.ScheduleProfiles
            .Include(p => p.Windows)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (dataModel == null) return null;

        return MapToDomain(dataModel);
    }

    public async Task UpdateAsync(ScheduleProfile profile, CancellationToken cancellationToken = default)
    {
        var dataModel = await _dbContext.ScheduleProfiles
            .Include(p => p.Windows)
            .FirstOrDefaultAsync(p => p.Id == profile.Id, cancellationToken);

        if (dataModel != null)
        {
            // Atualiza propriedades raiz
            dataModel.Timezone = profile.Timezone;
            
            // Remove janelas antigas e insere as novas (garante sincronia total)
            _dbContext.ScheduleWindows.RemoveRange(dataModel.Windows);
            dataModel.Windows = ExtractWindows(profile);

            _dbContext.ScheduleProfiles.Update(dataModel);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    // --- Tradutores ---

    private ScheduleProfileData MapToDataModel(ScheduleProfile profile)
    {
        return new ScheduleProfileData
        {
            Id = profile.Id,
            Timezone = profile.Timezone,
            Windows = ExtractWindows(profile)
        };
    }

    private List<ScheduleWindowData> ExtractWindows(ScheduleProfile profile)
    {
        var windows = new List<ScheduleWindowData>();
        foreach (var (day, daySchedule) in profile.WeeklySchedule)
        {
            foreach (var w in daySchedule.Windows)
            {
                windows.Add(new ScheduleWindowData
                {
                    Id = Guid.NewGuid(),
                    ScheduleProfileId = profile.Id,
                    DayOfWeek = day,
                    StartTime = new TimeSpan(w.Start.Hour, w.Start.Minute, 0),
                    EndTime = new TimeSpan(w.End.Hour, w.End.Minute, 0)
                });
            }
        }
        return windows;
    }

    private ScheduleProfile MapToDomain(ScheduleProfileData data)
    {
        // 1. Instancia agregado passando regras estruturais
        var profile = new ScheduleProfile(data.Id, data.Timezone);

        // 2. Reconstrói a geometria de tempo agrupada por dia
        var domainSchedule = new Dictionary<DayOfWeek, DaySchedule>();
        
        var groupedWindows = data.Windows.GroupBy(w => w.DayOfWeek);
        foreach (var group in groupedWindows)
        {
            var timeWindows = group.Select(w => new TimeWindow(
                new TimeOfDay(w.StartTime.Hours, w.StartTime.Minutes),
                new TimeOfDay(w.EndTime.Hours, w.EndTime.Minutes)
            )).ToList();

            // O construtor do DaySchedule re-valida as invariantes
            domainSchedule[group.Key] = new DaySchedule(timeWindows);
        }

        profile.UpdateWeeklySchedule(domainSchedule);
        return profile;
    }
}
