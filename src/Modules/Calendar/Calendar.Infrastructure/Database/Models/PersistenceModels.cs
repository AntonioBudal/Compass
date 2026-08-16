using System;
using System.Collections.Generic;

namespace Compass.Modules.Calendar.Infrastructure.Database.Models;

internal class ScheduleProfileData
{
    public Guid Id { get; set; }
    public string Timezone { get; set; } = null!;
    public List<ScheduleWindowData> Windows { get; set; } = new();
}

internal class ScheduleWindowData
{
    public Guid Id { get; set; }
    public Guid ScheduleProfileId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public ScheduleProfileData Profile { get; set; } = null!;
}
