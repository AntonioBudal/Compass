using System;
using System.Collections.Generic;
using System.Linq;
using Compass.Modules.Execution.Domain.Time;
using Compass.SharedKernel.Domain.Exceptions;

namespace Compass.Modules.Execution.Domain.DailyCycles;

public class DailyCycle
{
    public Guid Id { get; private set; }
    public DateOnly Date { get; private set; }
    public CycleStatus Status { get; private set; }
private readonly List<ExecutionLog> _logs = new();
    public IReadOnlyCollection<ExecutionLog> Logs => _logs.AsReadOnly();

    private DailyCycle() { } // Requisito ORM

    public DailyCycle(DateOnly date)
    {
        Id = Guid.NewGuid();
        Date = date;
        Status = CycleStatus.NotStarted;
    }

    // --- Transições de Estado ---

    public void Start()
    {
        if (Status != CycleStatus.NotStarted)
            throw new DomainException("Cycle can only be started if it is NotStarted.");

        Status = CycleStatus.Active;
    }

    public void Close()
    {
        if (Status == CycleStatus.Closed)
            throw new DomainException("Cycle is already closed.");

        Status = CycleStatus.Closed;
    }

    // --- Mutação Interna (A Fronteira de Transação) ---

    public void RecordExecution(Guid referenceId, TimeInterval interval, ExecutionType type)
    {
        // 1. Invariante de Estado (Somente dias abertos recebem logs)
        if (Status != CycleStatus.Active)
            throw new DomainException("Cannot record logs unless the cycle is active.");

        // 2. Invariante de Data (O Log não pode vazar o limite do dia do ciclo civil)
        // Simplificação: Verifica se ao menos o Start do log pertence ao dia do ciclo no timezone UTC/local abstraído.
        // Aqui assumiremos que a Application envia um DateOffset local ao contexto do usuário.
        var logDate = DateOnly.FromDateTime(interval.Start.Date);
        if (logDate != Date)
            throw new DomainException($"Cannot record a log for {logDate} inside a cycle meant for {Date}.");

        // 3. Invariante Matemática Absoluta (A Ubiquidade é probida)
        if (_logs.Any(existingLog => existingLog.Interval.OverlapsWith(interval)))
            throw new DomainException("Execution interval overlaps with an existing log.");

        // Tudo aprovado, emite o recibo.
        var log = new ExecutionLog(referenceId, interval, type);
        _logs.Add(log);
    }

    public void DeleteExecution(Guid logId)
    {
        if (Status != CycleStatus.Active)
            throw new DomainException("Cannot delete logs unless the cycle is active.");

        var log = _logs.FirstOrDefault(l => l.Id == logId);
        if (log != null)
        {
            _logs.Remove(log);
        }
    }
}





