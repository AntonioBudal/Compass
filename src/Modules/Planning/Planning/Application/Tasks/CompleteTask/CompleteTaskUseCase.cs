using System;
using System.Threading;
using Compass.Modules.Planning.Application.Tasks;
using Task = Compass.Modules.Planning.Domain.Tasks.Task;

namespace Compass.Modules.Planning.Application.Tasks.CompleteTask;

public class CompleteTaskUseCase
{
    private readonly ITaskRepository _taskRepository;

    public CompleteTaskUseCase(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async System.Threading.Tasks.Task ExecuteAsync(CompleteTaskCommand command, CancellationToken cancellationToken = default)
    {
        // 1. Busca a tarefa
        var task = await _taskRepository.GetByIdAsync(command.TaskId, cancellationToken);
        
        if (task == null)
        {
            throw new Exception($"Task with ID {command.TaskId} not found.");
        }

        // 2. Invoca o comportamento de negócio (Transita para Completed)
        // O domínio rejeitará tarefas em Draft ou Archived.
        task.Complete();

        // 3. Persiste o novo estado
        await _taskRepository.UpdateAsync(task, cancellationToken);
    }
}
