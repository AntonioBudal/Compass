using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Compass.Modules.Calendar.Contracts.Queries;

public interface IAvailabilityQuery
{
    Task<IReadOnlyList<AvailabilityWindow>> GetAvailabilityAsync(Guid profileId, DateOnly date, CancellationToken cancellationToken = default);
}
