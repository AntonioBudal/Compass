using Compass.Modules.Calendar.Contracts.DTOs;

namespace Compass.Modules.Calendar.Contracts;

public interface ICalendarModule
{
    Task<ScheduleProfileDto?> GetProfileByIdAsync(Guid profileId, CancellationToken cancellationToken = default);
}
