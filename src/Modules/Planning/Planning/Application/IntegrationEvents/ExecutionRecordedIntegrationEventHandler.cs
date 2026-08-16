using System.Threading;
using MediatR;
using Microsoft.Extensions.Logging;
using Compass.Modules.Execution.Contracts.Events;
using Compass.Modules.Planning.Domain.Tasks;
using Compass.Modules.Planning.Domain.Habits;
using Compass.Modules.Planning.Application.Tasks;
using Compass.Modules.Planning.Application.Habits;
using SystemTask = System.Threading.Tasks.Task;
using DomainTaskStatus = Compass.Modules.Planning.Domain.Tasks.TaskStatus; // Alias para evitar colisão com System.Threading.Tasks

namespace Compass.Modules.Planning.Application.IntegrationEvents;

public class ExecutionRecordedIntegrationEventHandler : INotificationHandler<ExecutionRecordedIntegrationEvent>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IHabitRepository _habitRepository;
    private readonly ILogger<ExecutionRecordedIntegrationEventHandler> _logger;

    public ExecutionRecordedIntegrationEventHandler(
        ITaskRepository taskRepository,
        IHabitRepository habitRepository,
        ILogger<ExecutionRecordedIntegrationEventHandler> logger)
    {
        _taskRepository = taskRepository;
        _habitRepository = habitRepository;
        _logger = logger;
    }

    public async SystemTask Handle(ExecutionRecordedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processando ExecutionRecordedIntegrationEvent no módulo Planning para ReferenceId: {ReferenceId}", 
            notification.ReferenceId);

        // 1. Verifica se o ReferenceId pertence a uma Task
        var task = await _taskRepository.GetByIdAsync(notification.ReferenceId, cancellationToken);

        if (task != null)
        {
            _logger.LogInformation("ReferenceId {ReferenceId} reconhecido como Task. Atualizando progresso...", task.Id);
            
            try 
            {
                if (task.Status == DomainTaskStatus.Ready)
                {
                    task.RegisterProgress(); // Move de Ready para InProgress
                    await _taskRepository.UpdateAsync(task, cancellationToken);
                    _logger.LogInformation("Task {TaskId} atualizada para InProgress.", task.Id);
                }
                else
                {
                    _logger.LogInformation("Task {TaskId} já está em status {Status}. Nenhuma mudança necessária.", task.Id, task.Status);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning("Não foi possível atualizar a Task {TaskId}. Motivo: {Message}", task.Id, ex.Message);
            }
            return;
        }

        // 2. Verifica se o ReferenceId pertence a um Habit
        var habit = await _habitRepository.GetByIdAsync(notification.ReferenceId, cancellationToken);

        if (habit != null)
        {
            _logger.LogInformation("ReferenceId {ReferenceId} reconhecido como Habit. Nenhuma mudança de status raiz necessária.", habit.Id);
            return;
        }

        _logger.LogWarning("ReferenceId {ReferenceId} não foi encontrado nem como Task, nem como Habit no módulo Planning.", notification.ReferenceId);
    }
}
