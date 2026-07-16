using Compass.Domain.Enums;
using Compass.Domain.Exceptions;

namespace Compass.Domain.Entities;

public class HabitCommitment : Commitment
{
    public string CronExpression { get; private set; } = null!;
    public int CurrentStreak { get; private set; }
    public int BestStreak { get; private set; }
    public int EstimatedDurationMinutes { get; private set; }
    public short EnergyRequired { get; private set; }

    protected HabitCommitment() { }

    public HabitCommitment(
        Guid userId, 
        string title, 
        string cronExpression, 
        int estimatedDurationMinutes = 15, 
        short energyRequired = 2, 
        Guid? projectId = null) 
        : base(userId, title, CommitmentType.Habit, projectId)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
            throw new DomainException("Um hábito deve possuir uma expressão CRON válida.");

        CronExpression = cronExpression.Trim();
        EstimatedDurationMinutes = estimatedDurationMinutes;
        EnergyRequired = energyRequired;
        CurrentStreak = 0;
        BestStreak = 0;
    }

    public void RegisterCompletion()
    {
        if (Status == CommitmentStatus.Archived)
            return;

        CurrentStreak++;
        if (CurrentStreak > BestStreak)
        {
            BestStreak = CurrentStreak;
        }

        Status = CommitmentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void BreakStreak()
    {
        CurrentStreak = 0;
        Status = CommitmentStatus.Pending;
    }
}