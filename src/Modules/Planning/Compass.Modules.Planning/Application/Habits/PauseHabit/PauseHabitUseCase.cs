using System;
using System.Threading;

namespace Compass.Modules.Planning.Application.Habits.PauseHabit;

public class PauseHabitUseCase
{
    private readonly IHabitRepository _habitRepository;

    public PauseHabitUseCase(IHabitRepository habitRepository)
    {
        _habitRepository = habitRepository;
    }

    public async System.Threading.Tasks.Task ExecuteAsync(PauseHabitCommand command, CancellationToken cancellationToken = default)
    {
        var habit = await _habitRepository.GetByIdAsync(command.HabitId, cancellationToken);
        if (habit == null) throw new Exception($"Habit with ID {command.HabitId} not found.");

        habit.Pause();
        
        await _habitRepository.UpdateAsync(habit, cancellationToken);
    }
}
