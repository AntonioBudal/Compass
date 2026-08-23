using System;
using System.Threading;
using System.Threading.Tasks;

namespace Compass.Modules.Execution.Application.DailyPlanning.Queries;

public interface IDailyPlanQueryService
{
    Task<DailyPlanDetailsDto?> GetByDateAsync(Guid profileId, DateOnly date, CancellationToken cancellationToken = default);
}