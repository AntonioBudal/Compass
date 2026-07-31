using Compass.Domain.Entities;
using Compass.Domain.Interfaces;
using Compass.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compass.Infrastructure.Repositories;

public class GoalRepository : IGoalRepository
{
    private readonly CompassDbContext _context;

    public GoalRepository(CompassDbContext context)
    {
        _context = context;
    }

    public async Task<Goal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Goals.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public void Update(Goal goal)
    {
        _context.Goals.Update(goal);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}