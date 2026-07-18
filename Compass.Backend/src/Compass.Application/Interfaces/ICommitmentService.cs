using Compass.Application.DTOs;

namespace Compass.Application.Interfaces;

public interface ICommitmentService
{
    Task<CommitmentDto?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CommitmentDto>> GetAllActiveAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CommitmentDto> CreateAsync(Guid userId, CreateCommitmentDto dto, CancellationToken cancellationToken = default);
    Task<StatusTransitionResponseDto> UpdateStatusAsync(Guid userId, Guid commitmentId, UpdateStatusDto dto, CancellationToken cancellationToken = default);
}