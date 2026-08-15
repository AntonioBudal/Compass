using System;
using System.Threading;
using Compass.Modules.Planning.Application.Tasks;
using Compass.Modules.Planning.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Task = Compass.Modules.Planning.Domain.Tasks.Task;

namespace Compass.Modules.Planning.Infrastructure.Repositories;

internal class TaskRepository : ITaskRepository
{
    private readonly PlanningDbContext _dbContext;

    public TaskRepository(PlanningDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async System.Threading.Tasks.Task AddAsync(Task task, CancellationToken cancellationToken = default)
    {
        await _dbContext.Tasks.AddAsync(task, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task<Task?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async System.Threading.Tasks.Task UpdateAsync(Task task, CancellationToken cancellationToken = default)
    {
        _dbContext.Tasks.Update(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
