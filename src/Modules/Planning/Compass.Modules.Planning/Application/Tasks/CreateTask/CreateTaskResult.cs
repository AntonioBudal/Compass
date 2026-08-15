using System;
using TaskStatus = Compass.Modules.Planning.Domain.Tasks.TaskStatus;

namespace Compass.Modules.Planning.Application.Tasks.CreateTask;

public record CreateTaskResult(
    Guid TaskId,
    TaskStatus Status
);
