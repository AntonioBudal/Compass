using Compass.Modules.Planning.Application.Tasks.Queries;
using Compass.Modules.Planning.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Compass.Modules.Planning.Infrastructure.Queries;

internal sealed class TaskQueryService : ITaskQueryService
{
    private readonly PlanningDbContext _dbContext;

    public TaskQueryService(PlanningDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TaskDetailsDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tasks
            .AsNoTracking()
            .Where(task => task.Id == id)
            .Select(task => new TaskDetailsDto(
                task.Id,
                task.Title,
                task.Status.ToString(),
                task.EstimatedDurationMinutes,
                task.HardDeadline,
                task.ProjectId))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
