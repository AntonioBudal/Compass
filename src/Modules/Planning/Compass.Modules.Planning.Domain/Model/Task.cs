using Compass.Modules.Planning.Domain.Exceptions;

namespace Compass.Modules.Planning.Domain.Model;

public class Task
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public int? DurationMinutes { get; private set; }
    public DateTimeOffset? Deadline { get; private set; }
    public TaskStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    // EF Core parameterless constructor
    private Task() { }

    private Task(
        Guid id,
        string title,
        string? description,
        int? durationMinutes,
        DateTimeOffset? deadline,
        TaskStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        Id = id;
        Title = title;
        Description = description;
        DurationMinutes = durationMinutes;
        Deadline = deadline?.ToUniversalTime();
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Task Create(
        string title,
        string? description = null,
        int? durationMinutes = null,
        DateTimeOffset? deadline = null)
    {
        ValidateTitle(title);

        if (durationMinutes.HasValue && durationMinutes.Value <= 0)
        {
            throw new PlanningDomainException("Duration estimate must be a positive integer.");
        }

        var now = DateTimeOffset.UtcNow;
        var initialStatus = durationMinutes.HasValue && durationMinutes.Value > 0
            ? TaskStatus.Ready
            : TaskStatus.Draft;

        return new Task(
            Guid.CreateVersion7(),
            title.Trim(),
            description?.Trim(),
            durationMinutes,
            deadline?.ToUniversalTime(),
            initialStatus,
            now,
            now
        );
    }

    public void SetEstimate(int? durationMinutes)
    {
        if (Status == TaskStatus.Done)
        {
            throw new PlanningDomainException("Cannot change duration estimate of a completed task.");
        }

        if (durationMinutes.HasValue && durationMinutes.Value <= 0)
        {
            throw new PlanningDomainException("Duration estimate must be a positive integer.");
        }

        DurationMinutes = durationMinutes;

        if (durationMinutes.HasValue && durationMinutes.Value > 0)
        {
            if (Status == TaskStatus.Draft)
            {
                Status = TaskStatus.Ready;
            }
        }
        else
        {
            if (Status == TaskStatus.Ready)
            {
                Status = TaskStatus.Draft;
            }
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDetails(
        string title,
        string? description,
        int? durationMinutes,
        DateTimeOffset? deadline)
    {
        if (Status == TaskStatus.Done)
        {
            throw new PlanningDomainException("Cannot update details of a completed task.");
        }

        ValidateTitle(title);

        Title = title.Trim();
        Description = description?.Trim();
        Deadline = deadline?.ToUniversalTime();

        SetEstimate(durationMinutes);
    }

    public void Start()
    {
        if (Status == TaskStatus.Draft)
        {
            throw new PlanningDomainException("A task must have a duration estimate and be in Ready status before starting.");
        }

        if (Status == TaskStatus.Done)
        {
            throw new PlanningDomainException("Completed tasks cannot be restarted directly.");
        }

        Status = TaskStatus.InProgress;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Complete()
    {
        if (Status == TaskStatus.Done)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        Status = TaskStatus.Done;
        CompletedAt = now;
        UpdatedAt = now;
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new PlanningDomainException("Task title cannot be empty or contain only whitespace.");
        }

        if (title.Trim().Length > 255)
        {
            throw new PlanningDomainException("Task title cannot exceed 255 characters.");
        }
    }
}
