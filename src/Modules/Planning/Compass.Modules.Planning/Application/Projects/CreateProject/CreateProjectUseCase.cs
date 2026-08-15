using System.Threading;
using Compass.Modules.Planning.Application.Projects;
using Project = Compass.Modules.Planning.Domain.Projects.Project;

namespace Compass.Modules.Planning.Application.Projects.CreateProject;

public class CreateProjectUseCase
{
    private readonly IProjectRepository _projectRepository;

    public CreateProjectUseCase(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async System.Threading.Tasks.Task<CreateProjectResult> ExecuteAsync(CreateProjectCommand command, CancellationToken cancellationToken = default)
    {
        // 1. Instancia o agregado (Domínio valida o título e nasce em "Planning")
        var project = new Project(command.Title);

        // 2. Persiste a intenção estratégica
        await _projectRepository.AddAsync(project, cancellationToken);

        // 3. Retorna a projeção
        return new CreateProjectResult(project.Id, project.Status);
    }
}
