using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Calendar.Application.Profiles;
using Compass.Modules.Calendar.Domain.Profiles;
using Compass.Modules.Calendar.Infrastructure.Database;
using Compass.Modules.Calendar.Infrastructure.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Compass.Modules.Calendar.Infrastructure.Repositories;

internal sealed class ScheduleProfileRepository : IScheduleProfileRepository
{
    private readonly CalendarDbContext _dbContext;

    public ScheduleProfileRepository(CalendarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ScheduleProfile profile, CancellationToken cancellationToken = default)
    {
        var dataModel = new ScheduleProfileData
        {
            Id = profile.Id,
            Timezone = profile.Timezone
        };

        await _dbContext.ScheduleProfiles.AddAsync(dataModel, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ScheduleProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dataModel = await _dbContext.ScheduleProfiles
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (dataModel == null)
            return null;

        // Reconstruindo a entidade rica de Domínio a partir do Data Model
        return new ScheduleProfile(dataModel.Id, dataModel.Timezone);
    }

    public async Task UpdateAsync(ScheduleProfile profile, CancellationToken cancellationToken = default)
    {
        var dataModel = await _dbContext.ScheduleProfiles
            .FirstOrDefaultAsync(p => p.Id == profile.Id, cancellationToken);

        if (dataModel != null)
        {
            dataModel.Timezone = profile.Timezone;
            _dbContext.ScheduleProfiles.Update(dataModel);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
