using Compass.Modules.Planning.Contracts.DTOs;

namespace Compass.Modules.Planning.Contracts;

public interface IPlanningModule
{
    Task<TaskDto?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskDto>> GetReadyTasksAsync(CancellationToken cancellationToken = default);
}
