using Compass.Modules.Calendar.Domain.Model;

namespace Compass.Modules.Calendar.Domain.Repositories;

public interface IScheduleProfileRepository
{
    Task<ScheduleProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(ScheduleProfile profile, CancellationToken cancellationToken = default);
    Task UpdateAsync(ScheduleProfile profile, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
