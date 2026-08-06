using Compass.Domain.Entities;

namespace Compass.Domain.Interfaces;

public interface IGoalRepository
{
    Task<Goal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Goal>> GetActiveGoalsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Goal goal, CancellationToken cancellationToken = default);
    void Update(Goal goal);
    void Remove(Goal goal);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}