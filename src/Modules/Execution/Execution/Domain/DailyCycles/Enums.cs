namespace Compass.Modules.Execution.Domain.DailyCycles;

public enum CycleStatus
{
    NotStarted,
    Active,
    Closed
}

public enum ExecutionType
{
    DeepWork,
    Routine,
    Break
}
