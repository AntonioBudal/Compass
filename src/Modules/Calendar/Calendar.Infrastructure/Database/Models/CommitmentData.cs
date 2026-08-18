using System;

namespace Compass.Modules.Calendar.Infrastructure.Database.Models;

internal sealed class CommitmentData
{
    public Guid Id { get; set; }
    public Guid ScheduleProfileId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public string Status { get; set; } = null!;
    public ScheduleProfileData Profile { get; set; } = null!;
}
