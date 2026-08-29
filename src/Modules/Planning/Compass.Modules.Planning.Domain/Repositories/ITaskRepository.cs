using Compass.Modules.Planning.Domain.Model;
using TaskModel = Compass.Modules.Planning.Domain.Model.Task;
using TaskStatus = Compass.Modules.Planning.Domain.Model.TaskStatus;

namespace Compass.Modules.Planning.Domain.Repositories;

public interface ITaskRepository
{
    System.Threading.Tasks.Task<TaskModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<IReadOnlyList<TaskModel>> ListAsync(TaskStatus? status = null, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task AddAsync(TaskModel task, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task UpdateAsync(TaskModel task, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
