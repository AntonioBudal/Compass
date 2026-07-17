using Compass.Domain.Entities;
using Compass.Domain.Enums;
using Compass.Domain.Interfaces;
using Compass.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compass.Infrastructure.Repositories;

public class CommitmentRepository : ICommitmentRepository
{
    private readonly CompassDbContext _context;

    public CommitmentRepository(CompassDbContext context)
    {
        _context = context;
    }

    public async Task<Commitment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Commitments.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Commitment>> GetActiveCandidatesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Esta consulta engatilha o índice parcial idx_commitments_now_engine no PostgreSQL (< 15ms)
        return await _context.Commitments
            .AsNoTracking()
            .Where(c => c.UserId == userId &&
                        (c.Status == CommitmentStatus.Pending || c.Status == CommitmentStatus.InProgress) &&
                        (c.Type == CommitmentType.Task || c.Type == CommitmentType.Habit))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EventCommitment>> GetEventsByRangeAsync(
        Guid userId, 
        DateTime start, 
        DateTime end, 
        CancellationToken cancellationToken = default)
    {
        // Engatilha o índice parcial idx_commitments_events_lookup
        return await _context.Commitments
            .OfType<EventCommitment>()
            .AsNoTracking()
            .Where(e => e.UserId == userId &&
                        e.Status != CommitmentStatus.Archived &&
                        e.StartTime < end &&
                        e.EndTime > start)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Commitment commitment, CancellationToken cancellationToken = default)
    {
        await _context.Commitments.AddAsync(commitment, cancellationToken);
    }

    public void Update(Commitment commitment)
    {
        _context.Commitments.Update(commitment);
    }

    public void Remove(Commitment commitment)
    {
        _context.Commitments.Remove(commitment);
    }
}