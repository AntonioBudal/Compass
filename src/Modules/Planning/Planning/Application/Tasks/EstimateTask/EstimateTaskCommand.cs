using System;

namespace Compass.Modules.Planning.Application.Tasks.EstimateTask;

public record EstimateTaskCommand(
    Guid TaskId,
    int EstimatedDurationMinutes
);
