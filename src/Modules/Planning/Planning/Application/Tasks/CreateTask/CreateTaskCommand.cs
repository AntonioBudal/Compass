namespace Compass.Modules.Planning.Application.Tasks.CreateTask;

public record CreateTaskCommand(
    string Title,
    Guid? ProjectId = null,
    DateTimeOffset? HardDeadline = null,
    int? EstimatedDurationMinutes = null
);
