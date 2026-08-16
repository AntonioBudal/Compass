using System;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Calendar.Domain.Commitments;

namespace Compass.Modules.Calendar.Application.Commitments;

public interface ICommitmentRepository
{
    Task AddAsync(Commitment commitment, CancellationToken cancellationToken = default);
    Task<Commitment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Commitment commitment, CancellationToken cancellationToken = default);
}
