using Compass.Domain.Entities;

namespace Compass.Domain.Interfaces;

public interface IDecisionSnapshotRepository
{
    Task<DecisionSnapshot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DecisionSnapshot>> GetByUserIdAsync(Guid userId, int limit = 50, CancellationToken cancellationToken = default);
    Task AddAsync(DecisionSnapshot snapshot, CancellationToken cancellationToken = default);
    void Update(DecisionSnapshot snapshot);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}