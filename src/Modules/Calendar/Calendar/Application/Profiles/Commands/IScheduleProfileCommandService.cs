using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Compass.Modules.Calendar.Application.Profiles.Commands;

public record WindowInputDto(string StartTime, string EndTime);
public record CreateProfileRequest(string Timezone, Dictionary<string, List<WindowInputDto>> WeeklySchedule);
public record AddWindowRequest(string DayOfWeek, string StartTime, string EndTime);

public interface IScheduleProfileCommandService
{
    Task CreateProfileAsync(Guid profileId, CreateProfileRequest request, CancellationToken cancellationToken = default);
    Task AddWindowAsync(Guid profileId, AddWindowRequest request, CancellationToken cancellationToken = default);
    Task RemoveWindowAsync(Guid profileId, Guid windowId, CancellationToken cancellationToken = default);
}