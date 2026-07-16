using Compass.Domain.Enums;
using Compass.Domain.Exceptions;
using Compass.Domain.Interfaces;

namespace Compass.Domain.Entities;

public abstract class Commitment
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public Guid Id { get; protected set; }
    public Guid UserId { get; protected set; }
    public Guid? ProjectId { get; protected set; }
    public string Title { get; protected set; } = null!;
    public CommitmentType Type { get; protected set; }
    public CommitmentStatus Status { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? CompletedAt { get; protected set; }
    public Guid? ConvertedToCommitmentId { get; protected set; }

    // Construtor protegido para o EF Core
    protected Commitment() { }

    protected Commitment(Guid userId, string title, CommitmentType type, Guid? projectId = null)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length < 3)
            throw new DomainException("O título do compromisso deve ter pelo menos 3 caracteres.");

        Id = Guid.NewGuid();
        UserId = userId;
        Title = title.Trim();
        Type = type;
        ProjectId = projectId;
        Status = CommitmentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public virtual void SetProject(Guid? projectId)
    {
        if (Status == CommitmentStatus.Archived)
            throw new DomainException("Não é possível alterar o projeto de um item arquivado.");

        ProjectId = projectId;
    }

    public virtual void Archive()
    {
        Status = CommitmentStatus.Archived;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}