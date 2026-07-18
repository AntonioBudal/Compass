namespace Compass.Application.DTOs;

public record CascadedDomainEventDto(
    string EventType,
    Guid EntityId,
    string Message,
    decimal? NewProgressPercentage = null
);

public record StatusTransitionResponseDto(
    Guid CommitmentId,
    string PreviousStatus,
    string CurrentStatus,
    DateTime Timestamp,
    IReadOnlyList<CascadedDomainEventDto> CascadedDomainEvents
);