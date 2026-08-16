using System;
using System.Threading;
using System.Threading.Tasks;

namespace Compass.Modules.Execution.Application.DailyCycles.Queries;

public interface IDailyCycleQueryService
{
    Task<DailyCycleDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DailyCycleDetailsDto?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
}
