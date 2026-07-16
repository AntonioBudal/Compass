using Compass.Domain.Enums;
using Compass.Domain.Exceptions;

namespace Compass.Domain.Entities;

public class NoteCommitment : Commitment
{
    public string? Content { get; private set; }

    protected NoteCommitment() { }

    public NoteCommitment(Guid userId, string title, string? content = null, Guid? projectId = null) 
        : base(userId, title, CommitmentType.Note, projectId)
    {
        Content = content?.Trim();
    }

    public void UpdateContent(string? content)
    {
        if (Status == CommitmentStatus.Archived)
            throw new DomainException("Não é possível alterar o conteúdo de uma nota arquivada.");

        Content = content?.Trim();
    }

    public void MarkAsConverted(Guid newCommitmentId)
    {
        ConvertedToCommitmentId = newCommitmentId;
        Status = CommitmentStatus.Archived;
    }
}