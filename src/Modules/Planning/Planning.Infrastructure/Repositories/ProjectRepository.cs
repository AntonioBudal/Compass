using System;
using System.Threading;
using Compass.Modules.Planning.Application.Projects;
using Compass.Modules.Planning.Domain.Projects;
using Compass.Modules.Planning.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Compass.Modules.Planning.Infrastructure.Repositories;

internal class ProjectRepository : IProjectRepository
{
    private readonly PlanningDbContext _dbContext;

    public ProjectRepository(PlanningDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async System.Threading.Tasks.Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        await _dbContext.Projects.AddAsync(project, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async System.Threading.Tasks.Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        _dbContext.Projects.Update(project);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
