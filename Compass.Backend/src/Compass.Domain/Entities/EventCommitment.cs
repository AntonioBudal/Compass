using Compass.Domain.Enums;
using Compass.Domain.Exceptions;

namespace Compass.Domain.Entities;

public class EventCommitment : Commitment
{
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public string? LocationOrLink { get; private set; }

    protected EventCommitment() { }

    public EventCommitment(
        Guid userId, 
        string title, 
        DateTime startTime, 
        DateTime endTime, 
        string? locationOrLink = null, 
        Guid? projectId = null) 
        : base(userId, title, CommitmentType.Event, projectId)
    {
        ValidateTimeRange(startTime, endTime);

        StartTime = startTime;
        EndTime = endTime;
        LocationOrLink = locationOrLink?.Trim();
    }

    public void Reschedule(DateTime newStartTime, DateTime newEndTime)
    {
        if (Status == CommitmentStatus.Archived)
            throw new DomainException("Não é possível reagendar um evento arquivado.");

        ValidateTimeRange(newStartTime, newEndTime);

        StartTime = newStartTime;
        EndTime = newEndTime;
    }

    private static void ValidateTimeRange(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new InvalidTimeRangeException(start, end);
    }

    public void UpdateEventDetails(DateTime start, DateTime end, string? location)
    {
        if (Status == CommitmentStatus.Archived)
            throw new DomainException("Não é possível alterar os detalhes de um evento arquivado.");
            
        if (end <= start)
            throw new InvalidTimeRangeException(start, end);

        StartTime = start;
        EndTime = end;
        LocationOrLink = location?.Trim();
    }
}