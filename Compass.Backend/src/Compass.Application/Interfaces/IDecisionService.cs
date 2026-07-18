using Compass.Application.DTOs;

namespace Compass.Application.Interfaces;

public interface IDecisionService
{
    Task<DecisionResponseDto> GetNowDecisionAsync(Guid userId, string timeZoneId, CancellationToken cancellationToken = default);
    Task RegisterChoiceAsync(Guid snapshotId, Guid chosenCommitmentId, CancellationToken cancellationToken = default);
}