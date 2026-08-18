using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Compass.Modules.Calendar.Application.Profiles.Commands;

public record WindowInputDto(string StartTime, string EndTime);
public record CreateProfileRequest(string Timezone, Dictionary<string, List<WindowInputDto>> WeeklySchedule);

public interface IScheduleProfileCommandService
{
    Task CreateProfileAsync(Guid profileId, CreateProfileRequest request, CancellationToken cancellationToken = default);
}
