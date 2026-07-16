using Compass.Domain.Exceptions;

namespace Compass.Domain.Entities;

public class FocusSession
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CommitmentId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public int ActualDurationMinutes { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected FocusSession() { }

    public FocusSession(
        Guid userId, 
        Guid commitmentId, 
        DateTime startTime, 
        DateTime endTime, 
        int actualDurationMinutes, 
        string? notes = null)
    {
        if (endTime <= startTime)
            throw new InvalidTimeRangeException(startTime, endTime);

        if (actualDurationMinutes <= 0)
            throw new DomainException("A duração real de uma sessão de foco deve ser maior que zero minutos.");

        Id = Guid.NewGuid();
        UserId = userId;
        CommitmentId = commitmentId;
        StartTime = startTime;
        EndTime = endTime;
        ActualDurationMinutes = actualDurationMinutes;
        Notes = notes?.Trim();
        CreatedAt = DateTime.UtcNow;
    }
}