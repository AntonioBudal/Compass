using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Compass.Modules.Calendar.Application.Profiles.Queries;

public record ScheduleWindowDto(Guid Id, string DayOfWeek, TimeSpan StartTime, TimeSpan EndTime);
public record ScheduleProfileDetailsDto(Guid Id, string Timezone, IReadOnlyList<ScheduleWindowDto> Windows);

public interface IScheduleProfileQueryService
{
    Task<ScheduleProfileDetailsDto?> GetProfileAsync(Guid profileId, CancellationToken cancellationToken = default);
}