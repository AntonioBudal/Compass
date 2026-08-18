using System;
using System.Threading;
using System.Threading.Tasks;

namespace Compass.Modules.Calendar.Application.Profiles.Queries;

public interface IAvailabilityQueryService
{
    Task<DailyAvailabilityDto?> GetAvailabilityAsync(Guid profileId, DateOnly date, CancellationToken cancellationToken = default);
}
