using System;
using System.Threading;

namespace Compass.Modules.Planning.Application.Projects.PauseProject;

public class PauseProjectUseCase
{
    private readonly IProjectRepository _projectRepository;

    public PauseProjectUseCase(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async System.Threading.Tasks.Task ExecuteAsync(PauseProjectCommand command, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project == null) throw new Exception($"Project with ID {command.ProjectId} not found.");

        project.Pause();
        
        await _projectRepository.UpdateAsync(project, cancellationToken);
    }
}
