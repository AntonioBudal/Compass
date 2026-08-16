using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Execution.Application.DailyCycles.Queries;
using Compass.Modules.Execution.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Compass.Modules.Execution.Infrastructure.Queries;

internal sealed class DailyCycleQueryService : IDailyCycleQueryService
{
    private readonly ExecutionDbContext _dbContext;

    public DailyCycleQueryService(ExecutionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DailyCycleDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.DailyCycles
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new DailyCycleDetailsDto(
                c.Id,
                c.Date,
                c.Status.ToString(),
                c.Logs.Select(l => new ExecutionLogDto(
                    l.Id,
                    l.ReferenceId,
                    l.Type.ToString(),
                    l.Interval.Start,
                    l.Interval.End)).ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<DailyCycleDetailsDto?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        return await _dbContext.DailyCycles
            .AsNoTracking()
            .Where(c => c.Date == date)
            .Select(c => new DailyCycleDetailsDto(
                c.Id,
                c.Date,
                c.Status.ToString(),
                c.Logs.Select(l => new ExecutionLogDto(
                    l.Id,
                    l.ReferenceId,
                    l.Type.ToString(),
                    l.Interval.Start,
                    l.Interval.End)).ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
