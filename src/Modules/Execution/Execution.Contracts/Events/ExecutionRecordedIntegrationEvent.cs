using System;
using MediatR;

namespace Compass.Modules.Execution.Contracts.Events;

/// <summary>
/// Evento de Integração publicado quando um log de execução é registrado com sucesso no ciclo diário.
/// Módulos externos (ex: Planning, Calendar) assinam este evento para atualizar o status de Tarefas, Hábitos ou Compromissos.
/// </summary>
public sealed record ExecutionRecordedIntegrationEvent(
    Guid ExecutionLogId,
    Guid DailyCycleId,
    Guid ReferenceId,
    string Type,
    DateTimeOffset Start,
    DateTimeOffset End
) : INotification;
