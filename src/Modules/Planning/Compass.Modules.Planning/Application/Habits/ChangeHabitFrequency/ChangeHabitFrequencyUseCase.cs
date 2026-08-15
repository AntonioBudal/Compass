using System;
using System.Threading;
using Compass.Modules.Planning.Domain.Habits;

namespace Compass.Modules.Planning.Application.Habits.ChangeHabitFrequency;

public class ChangeHabitFrequencyUseCase
{
    private readonly IHabitRepository _habitRepository;

    public ChangeHabitFrequencyUseCase(IHabitRepository habitRepository)
    {
        _habitRepository = habitRepository;
    }

    public async System.Threading.Tasks.Task ExecuteAsync(ChangeHabitFrequencyCommand command, CancellationToken cancellationToken = default)
    {
        var habit = await _habitRepository.GetByIdAsync(command.HabitId, cancellationToken);
        if (habit == null) throw new Exception($"Habit with ID {command.HabitId} not found.");

        HabitFrequency frequency;
        
        if (command.IntervalDays.HasValue)
        {
            frequency = HabitFrequency.CreateInterval(command.IntervalDays.Value);
        }
        else if (command.DaysOfWeek != null)
        {
            frequency = HabitFrequency.CreateWeekly(command.DaysOfWeek);
        }
        else
        {
            throw new ArgumentException("Frequency must be provided (either IntervalDays or DaysOfWeek).");
        }

        habit.ChangeFrequency(frequency);
        
        await _habitRepository.UpdateAsync(habit, cancellationToken);
    }
}
