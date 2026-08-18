using Compass.SharedKernel.Domain.Exceptions;

namespace Compass.Modules.Calendar.Domain.ScheduleExceptions;

public sealed class ScheduleException
{
    public Guid Id { get; }
    public Guid ScheduleProfileId { get; }
    public DateOnly Date { get; }
    public TimeSpan StartTime { get; }
    public TimeSpan EndTime { get; }
    public string Reason { get; }

    public ScheduleException(
        Guid scheduleProfileId,
        DateOnly date,
        TimeSpan startTime,
        TimeSpan endTime,
        string reason)
    {
        if (scheduleProfileId == Guid.Empty)
        {
            throw new DomainException(
                "ScheduleProfile ID cannot be empty.");
        }

        if (startTime < TimeSpan.Zero ||
            startTime >= TimeSpan.FromDays(1))
        {
            throw new DomainException(
                "Exception start time must be inside a single day.");
        }

        if (endTime <= startTime ||
            endTime >= TimeSpan.FromDays(1))
        {
            throw new DomainException(
                "Exception end time must be after start time and inside the same day.");
        }

        if (startTime.Ticks % TimeSpan.TicksPerMinute != 0 ||
            endTime.Ticks % TimeSpan.TicksPerMinute != 0)
        {
            throw new DomainException(
                "ScheduleException only supports minute precision.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException(
                "ScheduleException reason cannot be empty.");
        }

        if (reason.Trim().Length > 500)
        {
            throw new DomainException(
                "ScheduleException reason cannot exceed 500 characters.");
        }

        Id = Guid.NewGuid();
        ScheduleProfileId = scheduleProfileId;
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        Reason = reason.Trim();
    }
}
