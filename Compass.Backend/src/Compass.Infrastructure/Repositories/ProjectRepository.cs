using Compass.Domain.Entities;
using Compass.Domain.Interfaces;
using Compass.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Compass.Domain.Enums;

namespace Compass.Infrastructure.Repositories;



public class ProjectRepository : IProjectRepository
{
    private readonly CompassDbContext _context;

    public ProjectRepository(CompassDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        await _context.Projects.AddAsync(project, cancellationToken);
    }

    public void Update(Project project)
    {
        _context.Projects.Update(project);
    }

    public async Task<IReadOnlyList<Project>> GetActiveCatalogAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        // O banco continuará usando o nosso novo índice LRU (idx_projects_user_catalog_lru) em < 5ms!
        return await _context.Projects
            .AsNoTracking()
            .Where(p => p.UserId == userId 
                     && p.Status != CommitmentStatus.Completed 
                     && p.Status != CommitmentStatus.Archived)
            .OrderByDescending(p => p.LastUsedAt ?? p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}