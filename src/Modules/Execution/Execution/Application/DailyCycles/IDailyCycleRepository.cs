using System;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Execution.Domain.DailyCycles;

namespace Compass.Modules.Execution.Application.DailyCycles;

public interface IDailyCycleRepository
{
    Task AddAsync(DailyCycle cycle, CancellationToken cancellationToken = default);
    Task<DailyCycle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(DailyCycle cycle, CancellationToken cancellationToken = default);
}
