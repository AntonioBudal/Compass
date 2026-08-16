using System;
using System.Threading;
using Compass.Modules.Planning.Domain.Habits;

namespace Compass.Modules.Planning.Application.Habits;

public interface IHabitRepository
{
    // Apenas as operações vitais do repositório
    System.Threading.Tasks.Task AddAsync(Habit habit, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<Habit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task UpdateAsync(Habit habit, CancellationToken cancellationToken = default);
}
