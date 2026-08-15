using Compass.SharedKernel.Domain;
using Compass.SharedKernel.Domain.Exceptions;

namespace Compass.Modules.Planning.Domain.Tasks;

public sealed class Task : Entity
{
    public string Title { get; private set; }
    public int? EstimatedDurationMinutes { get; private set; }
    public DateTimeOffset? HardDeadline { get; private set; }
    public Guid? ProjectId { get; private set; }
    public TaskStatus Status { get; private set; }

    private Task() { Title = string.Empty; }

    public Task(string title, Guid? projectId = null, DateTimeOffset? hardDeadline = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title cannot be empty.");

        Id = Guid.NewGuid();
        Title = title;
        ProjectId = projectId;
        HardDeadline = hardDeadline;
        Status = TaskStatus.Draft;
    }

    public void EstimateTime(int durationMinutes)
    {
        if (durationMinutes <= 0)
            throw new DomainException("Estimated duration must be greater than zero.");

        if (Status is TaskStatus.Completed or TaskStatus.Archived)
            throw new DomainException($"Cannot estimate time for a {Status} task.");

        EstimatedDurationMinutes = durationMinutes;

        if (Status == TaskStatus.Draft)
            Status = TaskStatus.Ready;
    }

    public void ChangeDeadline(DateTimeOffset? newDeadline)
    {
        if (Status is TaskStatus.Completed or TaskStatus.Archived)
            throw new DomainException($"Cannot change deadline of a {Status} task.");

        HardDeadline = newDeadline;
    }

    public void RegisterProgress()
    {
        if (Status == TaskStatus.Draft)
            throw new DomainException("Cannot start a Draft. Estimate time first.");

        if (Status is TaskStatus.Completed or TaskStatus.Archived)
            throw new DomainException($"Cannot register progress on a {Status} task.");

        Status = TaskStatus.InProgress;
    }

    public void Complete()
    {
        if (Status == TaskStatus.Draft)
            throw new DomainException("Cannot complete a Draft task. It requires an estimation.");

        if (Status == TaskStatus.Archived)
            throw new DomainException("Cannot complete an Archived task.");

        Status = TaskStatus.Completed;
    }

    public void Reopen()
    {
        if (Status != TaskStatus.Completed)
            throw new DomainException("Only Completed tasks can be reopened.");

        Status = TaskStatus.Ready;
    }

    public void Archive()
    {
        Status = TaskStatus.Archived;
    }

    public void Unarchive()
    {
        if (Status != TaskStatus.Archived)
            throw new DomainException("Task is not archived.");

        Status = EstimatedDurationMinutes.HasValue ? TaskStatus.Ready : TaskStatus.Draft;
    }
}
