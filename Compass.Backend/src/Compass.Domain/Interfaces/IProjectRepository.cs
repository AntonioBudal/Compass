using Compass.Domain.Entities;

namespace Compass.Domain.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Project>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Project project, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Project>> GetActiveCatalogAsync(Guid userId, CancellationToken cancellationToken = default);
    void Update(Project project);

    void Remove(Project project);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}