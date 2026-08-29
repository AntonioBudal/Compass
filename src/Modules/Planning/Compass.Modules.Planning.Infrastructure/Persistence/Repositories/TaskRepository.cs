using Compass.Modules.Planning.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using TaskModel = Compass.Modules.Planning.Domain.Model.Task;
using TaskStatus = Compass.Modules.Planning.Domain.Model.TaskStatus;

namespace Compass.Modules.Planning.Infrastructure.Persistence.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly PlanningDbContext _context;

    public TaskRepository(PlanningDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<TaskModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async System.Threading.Tasks.Task<IReadOnlyList<TaskModel>> ListAsync(TaskStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Tasks.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        return await query.OrderBy(t => t.CreatedAt).ToListAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task AddAsync(TaskModel task, CancellationToken cancellationToken = default)
    {
        await _context.Tasks.AddAsync(task, cancellationToken);
    }

    public System.Threading.Tasks.Task UpdateAsync(TaskModel task, CancellationToken cancellationToken = default)
    {
        _context.Tasks.Update(task);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public async System.Threading.Tasks.Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
