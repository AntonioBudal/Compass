using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Compass.Modules.Planning.Application.Projects;
using Project = Compass.Modules.Planning.Domain.Projects.Project;

namespace Compass.Modules.Planning.Tests.Application.Projects;

public class FakeProjectRepository : IProjectRepository
{
    public readonly List<Project> SavedProjects = new();

    public System.Threading.Tasks.Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        SavedProjects.Add(project);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public System.Threading.Tasks.Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = SavedProjects.FirstOrDefault(p => p.Id == id);
        return System.Threading.Tasks.Task.FromResult(project);
    }

    public System.Threading.Tasks.Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        var existing = SavedProjects.FirstOrDefault(p => p.Id == project.Id);
        if (existing != null)
        {
            SavedProjects.Remove(existing);
            SavedProjects.Add(project);
        }
        return System.Threading.Tasks.Task.CompletedTask;
    }
}
