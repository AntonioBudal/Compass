using System;
using System.Threading;

namespace Compass.Modules.Planning.Application.Habits.ArchiveHabit;

public class ArchiveHabitUseCase
{
    private readonly IHabitRepository _habitRepository;

    public ArchiveHabitUseCase(IHabitRepository habitRepository)
    {
        _habitRepository = habitRepository;
    }

    public async System.Threading.Tasks.Task ExecuteAsync(ArchiveHabitCommand command, CancellationToken cancellationToken = default)
    {
        var habit = await _habitRepository.GetByIdAsync(command.HabitId, cancellationToken);
        if (habit == null) throw new Exception($"Habit with ID {command.HabitId} not found.");

        habit.Archive();
        
        await _habitRepository.UpdateAsync(habit, cancellationToken);
    }
}
