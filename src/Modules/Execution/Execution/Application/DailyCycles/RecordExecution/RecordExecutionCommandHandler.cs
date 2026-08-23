using System;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Execution.Application.DailyCycles;
using Compass.Modules.Execution.Contracts.Events;
using Compass.Modules.Execution.Domain.Time;
using Compass.Modules.Execution.Domain.DailyCycles;
using MediatR;

namespace Compass.Modules.Execution.Application.DailyCycles.RecordExecution;

public class RecordExecutionCommandHandler
{
    private readonly IDailyCycleRepository _repository;
    private readonly IPublisher _publisher;

    public RecordExecutionCommandHandler(IDailyCycleRepository repository, IPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task HandleAsync(RecordExecutionCommand command, CancellationToken cancellationToken = default)
    {
        var cycle = await _repository.GetByIdAsync(command.DailyCycleId, cancellationToken);
        
        if (cycle == null)
            throw new Exception($"DailyCycle with ID {command.DailyCycleId} not found.");

        // Validação estrita: ReferenceId é obrigatório para trabalho, mas proibido para pausas.
        if (command.Type != ExecutionType.Break && command.ReferenceId == null)
            throw new ArgumentException("ReferenceId is required for DeepWork and Routine.");

        var interval = new TimeInterval(command.Start, command.End);
        
        cycle.RecordExecution(command.ReferenceId, interval, command.Type);

        await _repository.UpdateAsync(cycle, cancellationToken);

        // INTEGRAÇÃO BLINDADA: Publicamos o evento SOMENTE se NÃO for uma pausa.
        // Isso protege o Planning de interpretar descansos como produtividade.
        if (command.Type != ExecutionType.Break && command.ReferenceId.HasValue)
        {
            var integrationEvent = new ExecutionRecordedIntegrationEvent(
                ExecutionLogId: Guid.NewGuid(),
                DailyCycleId: cycle.Id,
                ReferenceId: command.ReferenceId.Value, 
                Type: command.Type.ToString(),
                Start: command.Start,
                End: command.End
            );

            await _publisher.Publish(integrationEvent, cancellationToken);
        }
    }
}