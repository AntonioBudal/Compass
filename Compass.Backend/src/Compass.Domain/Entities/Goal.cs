using Compass.Domain.Enums;
using Compass.Domain.Exceptions;
using Compass.Domain.Interfaces;

namespace Compass.Domain.Entities;

public class Goal
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? WhyDescription { get; private set; }
    public DateTime? TargetDate { get; private set; }
    public GoalStatus Status { get; private set; }
    public decimal ProgressPercentage { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    protected Goal() { }

    public Goal(Guid userId, string title, string? whyDescription = null, DateTime? targetDate = null)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length < 3)
            throw new DomainException("O título do objetivo deve ter pelo menos 3 caracteres.");

        Id = Guid.NewGuid();
        UserId = userId;
        Title = title.Trim();
        WhyDescription = whyDescription;
        TargetDate = targetDate;
        Status = GoalStatus.Active;
        ProgressPercentage = 0.00m;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProgress(decimal newProgress)
    {
        if (newProgress < 0.00m || newProgress > 100.00m)
            throw new DomainException("A porcentagem de progresso deve estar entre 0 e 100.");

        ProgressPercentage = newProgress;
        UpdatedAt = DateTime.UtcNow;
        
        if (ProgressPercentage == 100.00m && Status != GoalStatus.Completed)
        {
            Status = GoalStatus.Completed;
        }
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}