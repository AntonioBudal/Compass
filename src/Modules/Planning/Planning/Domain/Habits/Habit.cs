using Compass.SharedKernel.Domain;
using Compass.SharedKernel.Domain.Exceptions;

namespace Compass.Modules.Planning.Domain.Habits;

public sealed class Habit : Entity
{
    public string Title { get; private set; }
    public int EstimatedDurationMinutes { get; private set; }
    public HabitFrequency Frequency { get; private set; }
    public HabitStatus Status { get; private set; }

    private Habit() { Title = string.Empty; Frequency = null!; }

    public Habit(string title, int estimatedDurationMinutes, HabitFrequency frequency)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Habit title cannot be empty.");

        if (estimatedDurationMinutes <= 0)
            throw new DomainException("Habit estimated duration must be greater than zero.");

        Id = Guid.NewGuid();
        Title = title;
        EstimatedDurationMinutes = estimatedDurationMinutes;
        Frequency = frequency ?? throw new DomainException("Habit frequency is required.");
        Status = HabitStatus.Active;
    }

    public void Pause()
    {
        if (Status != HabitStatus.Active)
            throw new DomainException("Only active habits can be paused.");

        Status = HabitStatus.Paused;
    }

    public void Resume()
    {
        if (Status != HabitStatus.Paused)
            throw new DomainException("Only paused habits can be resumed.");

        Status = HabitStatus.Active;
    }

    public void Archive()
    {
        Status = HabitStatus.Archived;
    }

    public void ChangeFrequency(HabitFrequency newFrequency)
    {
        if (Status == HabitStatus.Archived)
            throw new DomainException("Cannot change frequency of an archived habit.");

        Frequency = newFrequency ?? throw new DomainException("New frequency cannot be null.");
    }
}
