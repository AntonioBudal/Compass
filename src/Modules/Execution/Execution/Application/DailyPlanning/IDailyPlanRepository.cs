using System;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Execution.Domain.DecisionEngine;

namespace Compass.Modules.Execution.Application.DailyPlanning;

public interface IDailyPlanRepository
{
    Task AddAsync(DailyPlan plan, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid profileId, DateOnly date, CancellationToken cancellationToken = default);
}
