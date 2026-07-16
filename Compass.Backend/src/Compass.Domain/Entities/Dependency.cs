using Compass.Domain.Exceptions;

namespace Compass.Domain.Entities;

public class Dependency
{
    public Guid ParentCommitmentId { get; private set; }
    public Guid ChildCommitmentId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected Dependency() { }

    public Dependency(Guid parentCommitmentId, Guid childCommitmentId)
    {
        if (parentCommitmentId == childCommitmentId)
            throw new DomainException("Uma tarefa não pode depender dela mesma (autodependência não permitida).");

        ParentCommitmentId = parentCommitmentId;
        ChildCommitmentId = childCommitmentId;
        CreatedAt = DateTime.UtcNow;
    }
}