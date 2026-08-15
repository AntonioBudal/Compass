using System;
using System.Threading;
using Compass.Modules.Planning.Application.Habits;
using Compass.Modules.Planning.Domain.Habits;
using Compass.Modules.Planning.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Compass.Modules.Planning.Infrastructure.Repositories;

internal class HabitRepository : IHabitRepository
{
    private readonly PlanningDbContext _dbContext;

    public HabitRepository(PlanningDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async System.Threading.Tasks.Task AddAsync(Habit habit, CancellationToken cancellationToken = default)
    {
        await _dbContext.Habits.AddAsync(habit, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task<Habit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async System.Threading.Tasks.Task UpdateAsync(Habit habit, CancellationToken cancellationToken = default)
    {
        _dbContext.Habits.Update(habit);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
