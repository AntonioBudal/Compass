using System;
using System.Threading;
using Compass.Modules.Planning.Domain.Tasks;
using Task = Compass.Modules.Planning.Domain.Tasks.Task;

namespace Compass.Modules.Planning.Application.Tasks;

public interface ITaskRepository
{
    System.Threading.Tasks.Task AddAsync(Task task, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<Task?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task UpdateAsync(Task task, CancellationToken cancellationToken = default);
}
