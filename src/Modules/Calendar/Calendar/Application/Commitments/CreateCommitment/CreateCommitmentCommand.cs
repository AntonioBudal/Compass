using System;

namespace Compass.Modules.Calendar.Application.Commitments.CreateCommitment;

public record CreateCommitmentCommand(
    string Title, 
    string? Description, 
    DateTimeOffset Start, 
    DateTimeOffset End
);

public record CreateCommitmentResult(Guid CommitmentId);
