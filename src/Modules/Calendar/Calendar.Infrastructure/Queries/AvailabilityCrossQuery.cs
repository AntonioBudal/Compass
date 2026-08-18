using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Calendar.Contracts.Queries;
using Compass.Modules.Calendar.Application.Profiles.Queries;
using Compass.Modules.Calendar.Domain.Availability;

namespace Compass.Modules.Calendar.Infrastructure.Queries;

internal sealed class AvailabilityCrossQuery : IAvailabilityQuery
{
    private readonly IAvailabilityQueryService _queryService;

    public AvailabilityCrossQuery(IAvailabilityQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<IReadOnlyList<AvailabilityWindow>> GetAvailabilityAsync(Guid profileId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var dailyAvailability = await _queryService.GetAvailabilityAsync(profileId, date, cancellationToken);

        if (dailyAvailability == null || dailyAvailability.FreeWindows.Count == 0)
            return Array.Empty<AvailabilityWindow>();

        return dailyAvailability.FreeWindows.Select(w => {
            // A fronteira onde os tempos locais são canonizados para instantes absolutos.
            var startUtc = CalendarTimeZoneResolver.ResolveToUtc(date, w.Start, dailyAvailability.Timezone);
            var endUtc = CalendarTimeZoneResolver.ResolveToUtc(date, w.End, dailyAvailability.Timezone);
            return new AvailabilityWindow(startUtc, endUtc);
        }).ToList().AsReadOnly();
    }
}
