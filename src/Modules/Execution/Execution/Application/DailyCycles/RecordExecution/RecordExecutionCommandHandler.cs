using System;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Execution.Application.DailyCycles;
using Compass.Modules.Execution.Contracts.Events;
using Compass.Modules.Execution.Domain.Time;
using MediatR;

namespace Compass.Modules.Execution.Application.DailyCycles.RecordExecution;

public class RecordExecutionCommandHandler
{
    private readonly IDailyCycleRepository _repository;
    private readonly IPublisher _publisher; // Injetando o barramento de eventos

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

        var interval = new TimeInterval(command.Start, command.End);
        
        // 1. O Domínio executa a regra de negócio
        cycle.RecordExecution(command.ReferenceId, interval, command.Type);

        // 2. A Infraestrutura salva o estado no banco de dados (Transação)
        await _repository.UpdateAsync(cycle, cancellationToken);

        // 3. Integração: Publicamos o evento para que outros módulos (Planning, Calendar) reajam.
        // Importante: Isso só executa se o UpdateAsync for bem-sucedido.
        var integrationEvent = new ExecutionRecordedIntegrationEvent(
            ExecutionLogId: Guid.NewGuid(), // Idealmente buscaríamos o Id do log recém inserido, mas o domínio oculta isso por coleção
            DailyCycleId: cycle.Id,
            ReferenceId: command.ReferenceId,
            Type: command.Type.ToString(),
            Start: command.Start,
            End: command.End
        );

        await _publisher.Publish(integrationEvent, cancellationToken);
    }
}
