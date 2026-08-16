using System;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Calendar.Domain.Profiles;

namespace Compass.Modules.Calendar.Application.Profiles;

public interface IScheduleProfileRepository
{
    Task AddAsync(ScheduleProfile profile, CancellationToken cancellationToken = default);
    Task<ScheduleProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(ScheduleProfile profile, CancellationToken cancellationToken = default);
}
