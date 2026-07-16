using Compass.Domain.Entities;

namespace Compass.Domain.Interfaces;

public interface ICommitmentRepository
{
    Task<Commitment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Commitment>> GetActiveCandidatesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventCommitment>> GetEventsByRangeAsync(Guid userId, DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task AddAsync(Commitment commitment, CancellationToken cancellationToken = default);
    void Update(Commitment commitment);
    void Remove(Commitment commitment);
}