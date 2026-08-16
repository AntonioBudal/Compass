using System;
using Compass.Modules.Calendar.Domain.Time;
using Compass.SharedKernel.Domain.Exceptions;

namespace Compass.Modules.Calendar.Domain.Commitments;

public class Commitment
{
    public Guid Id { get; private set; }

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public TimeInterval Interval { get; private set; } = null!;

    public CommitmentStatus Status { get; private set; }

    private Commitment() { } // Requisito para ORM

    public Commitment(
        string title,
        string? description,
        TimeInterval interval)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException(
                "Commitment title cannot be empty.");

        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        Interval = interval;
        Status = CommitmentStatus.Confirmed;
    }
}