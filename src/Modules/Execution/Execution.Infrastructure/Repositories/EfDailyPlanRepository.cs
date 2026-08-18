using System;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Execution.Application.DailyPlanning;
using Compass.Modules.Execution.Domain.DecisionEngine;
using Compass.Modules.Execution.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Compass.Modules.Execution.Infrastructure.Repositories;

internal sealed class EfDailyPlanRepository : IDailyPlanRepository
{
    private readonly ExecutionDbContext _dbContext;

    public EfDailyPlanRepository(ExecutionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(DailyPlan plan, CancellationToken cancellationToken = default)
    {
        await _dbContext.DailyPlans.AddAsync(plan, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid profileId, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await _dbContext.DailyPlans.AnyAsync(p => p.ProfileId == profileId && p.Date == date, cancellationToken);
    }
}
