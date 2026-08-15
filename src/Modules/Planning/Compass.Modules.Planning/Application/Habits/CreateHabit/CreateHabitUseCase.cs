using System;
using System.Threading;
using Compass.Modules.Planning.Application.Habits;
using Compass.Modules.Planning.Domain.Habits;

namespace Compass.Modules.Planning.Application.Habits.CreateHabit;

public class CreateHabitUseCase
{
    private readonly IHabitRepository _habitRepository;

    public CreateHabitUseCase(IHabitRepository habitRepository)
    {
        _habitRepository = habitRepository;
    }

    public async System.Threading.Tasks.Task<CreateHabitResult> ExecuteAsync(CreateHabitCommand command, CancellationToken cancellationToken = default)
    {
        // 1. Tradução da representação externa para o Value Object do Domínio
        HabitFrequency frequency;
        
        if (command.IntervalDays.HasValue)
        {
            // A Application não sabe se "0" ou "-1" é válido. Ela delega para o Domínio.
            frequency = HabitFrequency.CreateInterval(command.IntervalDays.Value);
        }
        else if (command.DaysOfWeek != null)
        {
            // A Application não sabe se uma lista vazia é válida. Delega para o Domínio.
            frequency = HabitFrequency.CreateWeekly(command.DaysOfWeek);
        }
        else
        {
            throw new ArgumentException("Frequency must be provided (either IntervalDays or DaysOfWeek).");
        }

        // 2. Instanciação do Agregado
        var habit = new Habit(command.Title, command.EstimatedDurationMinutes, frequency);

        // 3. Persistência
        await _habitRepository.AddAsync(habit, cancellationToken);

        // 4. Projeção de Retorno
        return new CreateHabitResult(habit.Id, habit.Status);
    }
}
