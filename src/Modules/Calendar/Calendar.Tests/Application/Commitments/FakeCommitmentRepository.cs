using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Calendar.Application.Commitments;
using Compass.Modules.Calendar.Domain.Commitments;

namespace Compass.Modules.Calendar.Tests.Application.Commitments;

public class FakeCommitmentRepository : ICommitmentRepository
{
    public readonly List<Commitment> Saved = new();

    public Task AddAsync(Commitment commitment, CancellationToken cancellationToken = default)
    {
        Saved.Add(commitment);
        return Task.CompletedTask;
    }

    public Task<Commitment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Saved.FirstOrDefault(c => c.Id == id));
    }

    public Task UpdateAsync(Commitment commitment, CancellationToken cancellationToken = default)
    {
        var existing = Saved.FirstOrDefault(c => c.Id == commitment.Id);
        if (existing != null)
        {
            Saved.Remove(existing);
            Saved.Add(commitment);
        }
        return Task.CompletedTask;
    }
}
