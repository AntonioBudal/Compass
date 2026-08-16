using System;
using System.Threading;

namespace Compass.Modules.Planning.Application.Habits.ResumeHabit;

public class ResumeHabitUseCase
{
    private readonly IHabitRepository _habitRepository;

    public ResumeHabitUseCase(IHabitRepository habitRepository)
    {
        _habitRepository = habitRepository;
    }

    public async System.Threading.Tasks.Task ExecuteAsync(ResumeHabitCommand command, CancellationToken cancellationToken = default)
    {
        var habit = await _habitRepository.GetByIdAsync(command.HabitId, cancellationToken);
        if (habit == null) throw new Exception($"Habit with ID {command.HabitId} not found.");

        habit.Resume();
        
        await _habitRepository.UpdateAsync(habit, cancellationToken);
    }
}
