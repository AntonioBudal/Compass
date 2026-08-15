using System;
using System.Threading;
using Compass.Modules.Planning.Domain.Tasks;
using Task = Compass.Modules.Planning.Domain.Tasks.Task;

namespace Compass.Modules.Planning.Application.Tasks.EstimateTask;

public class EstimateTaskUseCase
{
    private readonly ITaskRepository _taskRepository;

    public EstimateTaskUseCase(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async System.Threading.Tasks.Task ExecuteAsync(EstimateTaskCommand command, CancellationToken cancellationToken = default)
    {
        // 1. Busca a intenção (Tarefa) no repositório
        var task = await _taskRepository.GetByIdAsync(command.TaskId, cancellationToken);
        
        if (task == null)
        {
            // Erro de aplicação (não de domínio). A API traduzirá isso para 404 Not Found depois.
            throw new Exception($"Task with ID {command.TaskId} not found.");
        }

        // 2. Executa a regra de negócio (pode transitar de Draft para Ready)
        task.EstimateTime(command.EstimatedDurationMinutes);

        // 3. Salva a alteração
        await _taskRepository.UpdateAsync(task, cancellationToken);
    }
}
