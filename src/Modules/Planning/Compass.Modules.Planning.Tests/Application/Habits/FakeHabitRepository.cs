using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Compass.Modules.Planning.Application.Habits;
using Compass.Modules.Planning.Domain.Habits;

namespace Compass.Modules.Planning.Tests.Application.Habits;

public class FakeHabitRepository : IHabitRepository
{
    public readonly List<Habit> SavedHabits = new();

    public System.Threading.Tasks.Task AddAsync(Habit habit, CancellationToken cancellationToken = default)
    {
        SavedHabits.Add(habit);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public System.Threading.Tasks.Task<Habit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return System.Threading.Tasks.Task.FromResult(SavedHabits.FirstOrDefault(h => h.Id == id));
    }

    public System.Threading.Tasks.Task UpdateAsync(Habit habit, CancellationToken cancellationToken = default)
    {
        var existing = SavedHabits.FirstOrDefault(h => h.Id == habit.Id);
        if (existing != null)
        {
            SavedHabits.Remove(existing);
            SavedHabits.Add(habit);
        }
        return System.Threading.Tasks.Task.CompletedTask;
    }
}
