using System;
using System.Collections.Generic;

namespace Compass.Modules.Calendar.Application.Profiles.Queries;

public record TimeWindowDto(TimeSpan Start, TimeSpan End);
public record DailyAvailabilityDto(Guid ProfileId, string Timezone, DateOnly Date, IReadOnlyList<TimeWindowDto> FreeWindows);
