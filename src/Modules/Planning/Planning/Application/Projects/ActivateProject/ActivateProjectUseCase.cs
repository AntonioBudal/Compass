using System;
using System.Threading;

namespace Compass.Modules.Planning.Application.Projects.ActivateProject;

public class ActivateProjectUseCase
{
    private readonly IProjectRepository _projectRepository;

    public ActivateProjectUseCase(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async System.Threading.Tasks.Task ExecuteAsync(ActivateProjectCommand command, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project == null) throw new Exception($"Project with ID {command.ProjectId} not found.");

        project.Activate();
        
        await _projectRepository.UpdateAsync(project, cancellationToken);
    }
}
