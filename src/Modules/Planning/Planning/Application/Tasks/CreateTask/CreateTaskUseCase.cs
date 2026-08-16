using Compass.Modules.Planning.Domain.Tasks;
using Task = Compass.Modules.Planning.Domain.Tasks.Task;

namespace Compass.Modules.Planning.Application.Tasks.CreateTask;

public class CreateTaskUseCase
{
    private readonly ITaskRepository _taskRepository;

    public CreateTaskUseCase(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async System.Threading.Tasks.Task<CreateTaskResult> ExecuteAsync(CreateTaskCommand command, CancellationToken cancellationToken = default)
    {
        // 1. Instancia o agregado (O Domínio valida Título e outras invariantes na criação)
        var task = new Task(command.Title, command.ProjectId, command.HardDeadline);

        // 2. Transição opcional de estado delegada ao Domínio
        if (command.EstimatedDurationMinutes.HasValue)
        {
            task.EstimateTime(command.EstimatedDurationMinutes.Value);
        }

        // 3. Persiste a intenção
        await _taskRepository.AddAsync(task, cancellationToken);

        // 4. Retorna a projeção do resultado
        return new CreateTaskResult(task.Id, task.Status);
    }
}
