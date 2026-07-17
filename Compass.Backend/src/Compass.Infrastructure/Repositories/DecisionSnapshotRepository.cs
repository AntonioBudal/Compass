using Compass.Domain.Entities;
using Compass.Domain.Interfaces;
using Compass.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compass.Infrastructure.Repositories;

public class DecisionSnapshotRepository : IDecisionSnapshotRepository
{
    private readonly CompassDbContext _context;

    public DecisionSnapshotRepository(CompassDbContext context)
    {
        _context = context;
    }

    public async Task<DecisionSnapshot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DecisionSnapshots.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<DecisionSnapshot>> GetByUserIdAsync(
        Guid userId, 
        int limit = 50, 
        CancellationToken cancellationToken = default)
    {
        return await _context.DecisionSnapshots
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(DecisionSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        await _context.DecisionSnapshots.AddAsync(snapshot, cancellationToken);
    }

    public void Update(DecisionSnapshot snapshot)
    {
        _context.DecisionSnapshots.Update(snapshot);
    }
}