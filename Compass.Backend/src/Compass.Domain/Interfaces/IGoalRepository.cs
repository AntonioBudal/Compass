using Compass.Domain.Entities;

namespace Compass.Domain.Interfaces;

public interface IGoalRepository
{
    Task<Goal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Update(Goal goal);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}