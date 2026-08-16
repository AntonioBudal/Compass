namespace Compass.Modules.Planning.Application.Tasks.Queries;

public interface ITaskQueryService
{
    Task<TaskDetailsDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
