using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Compass.Modules.Planning.Application.Tasks;
using Task = Compass.Modules.Planning.Domain.Tasks.Task;

namespace Compass.Modules.Planning.Tests.Application.Tasks;

public class FakeTaskRepository : ITaskRepository
{
    public readonly List<Task> SavedTasks = new();

    public System.Threading.Tasks.Task AddAsync(Task task, CancellationToken cancellationToken = default)
    {
        SavedTasks.Add(task);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public System.Threading.Tasks.Task<Task?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = SavedTasks.FirstOrDefault(t => t.Id == id);
        return System.Threading.Tasks.Task.FromResult(task);
    }

    public System.Threading.Tasks.Task UpdateAsync(Task task, CancellationToken cancellationToken = default)
    {
        var existing = SavedTasks.FirstOrDefault(t => t.Id == task.Id);
        if (existing != null)
        {
            SavedTasks.Remove(existing);
            SavedTasks.Add(task); // Simula atualização em memória
        }
        return System.Threading.Tasks.Task.CompletedTask;
    }
}
