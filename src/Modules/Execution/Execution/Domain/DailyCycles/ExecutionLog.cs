using System;
using Compass.Modules.Execution.Domain.Time;

namespace Compass.Modules.Execution.Domain.DailyCycles;

public class ExecutionLog
{
    public Guid Id { get; private set; }
    
    // Transformado em Nullable para aceitar o tipo Break
    public Guid? ReferenceId { get; private set; } 
    
    public TimeInterval Interval { get; private set; } = null!;
    public ExecutionType Type { get; private set; }

    private ExecutionLog() { } // Requisito do ORM

    internal ExecutionLog(Guid? referenceId, TimeInterval interval, ExecutionType type)
    {
        Id = Guid.NewGuid();
        ReferenceId = referenceId;
        Interval = interval;
        Type = type;
    }
}