using System;

namespace Compass.Modules.Calendar.Infrastructure.Database.Models;

internal sealed class ScheduleExceptionData
{
    public Guid Id { get; set; }
    public Guid ScheduleProfileId { get; set; }
    public DateOnly Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Reason { get; set; } = null!;
    public ScheduleProfileData Profile { get; set; } = null!;
}
