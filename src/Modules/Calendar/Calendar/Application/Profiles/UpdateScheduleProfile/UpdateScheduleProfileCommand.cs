using System;
using System.Collections.Generic;

namespace Compass.Modules.Calendar.Application.Profiles.UpdateScheduleProfile;

public record TimeWindowDto(TimeSpan Start, TimeSpan End);

public record UpdateScheduleProfileCommand(
    Guid ProfileId,
    string Timezone,
    Dictionary<DayOfWeek, IEnumerable<TimeWindowDto>> WeeklySchedule
);
