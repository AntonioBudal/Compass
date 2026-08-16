using System;
using System.Threading;

namespace Compass.Modules.Planning.Application.Projects.CompleteProject;

public class CompleteProjectUseCase
{
    private readonly IProjectRepository _projectRepository;

    public CompleteProjectUseCase(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async System.Threading.Tasks.Task ExecuteAsync(CompleteProjectCommand command, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project == null) throw new Exception($"Project with ID {command.ProjectId} not found.");

        project.Complete();
        
        await _projectRepository.UpdateAsync(project, cancellationToken);
    }
}
