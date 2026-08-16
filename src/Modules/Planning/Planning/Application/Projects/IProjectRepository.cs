using System;
using System.Threading;
using Compass.Modules.Planning.Domain.Projects;
using Project = Compass.Modules.Planning.Domain.Projects.Project;

namespace Compass.Modules.Planning.Application.Projects;

public interface IProjectRepository
{
    System.Threading.Tasks.Task AddAsync(Project project, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task UpdateAsync(Project project, CancellationToken cancellationToken = default);
}
