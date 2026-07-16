namespace Compass.Domain.Exceptions;

public class EventOverlapException : DomainException
{
    public Guid ConflictingCommitmentId { get; }

    public EventOverlapException(string message, Guid conflictingCommitmentId) : base(message)
    {
        ConflictingCommitmentId = conflictingCommitmentId;
    }

    public EventOverlapException(Guid conflictingCommitmentId, DateTime start, DateTime end)
        : base($"Conflito de agenda detectado para o intervalo entre {start:t} e {end:t}.")
    {
        ConflictingCommitmentId = conflictingCommitmentId;
    }
}