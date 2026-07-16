using Compass.Domain.Enums;
using Compass.Domain.Exceptions;
using Compass.Domain.Interfaces;

namespace Compass.Domain.Entities;

public class Project
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? GoalId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public DateTime? Deadline { get; private set; }
    public CommitmentStatus Status { get; private set; }
    public int TotalEstimatedDurationMinutes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    protected Project() { }

    public Project(Guid userId, string title, Guid? goalId = null, DateTime? deadline = null)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length < 3)
            throw new DomainException("O título do projeto deve ter pelo menos 3 caracteres.");

        Id = Guid.NewGuid();
        UserId = userId;
        GoalId = goalId;
        Title = title.Trim();
        Deadline = deadline;
        Status = CommitmentStatus.Pending;
        TotalEstimatedDurationMinutes = 0;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddEstimatedDuration(int minutes)
    {
        if (minutes < 0)
            throw new DomainException("A duração a ser somada não pode ser negativa.");

        TotalEstimatedDurationMinutes += minutes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status == CommitmentStatus.Completed)
            return;

        Status = CommitmentStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reopen()
    {
        Status = CommitmentStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}