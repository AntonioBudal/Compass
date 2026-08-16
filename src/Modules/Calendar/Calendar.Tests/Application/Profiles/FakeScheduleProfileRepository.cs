using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Calendar.Application.Profiles;
using Compass.Modules.Calendar.Domain.Profiles;

namespace Compass.Modules.Calendar.Tests.Application.Profiles;

public class FakeScheduleProfileRepository : IScheduleProfileRepository
{
    public readonly List<ScheduleProfile> Saved = new();

    public Task AddAsync(ScheduleProfile profile, CancellationToken cancellationToken = default)
    {
        Saved.Add(profile);
        return Task.CompletedTask;
    }

    public Task<ScheduleProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Saved.FirstOrDefault(p => p.Id == id));
    }

    public Task UpdateAsync(ScheduleProfile profile, CancellationToken cancellationToken = default)
    {
        var existing = Saved.FirstOrDefault(p => p.Id == profile.Id);
        if (existing != null)
        {
            Saved.Remove(existing);
            Saved.Add(profile);
        }
        return Task.CompletedTask;
    }
}
