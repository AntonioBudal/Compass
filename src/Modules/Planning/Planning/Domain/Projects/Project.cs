using Compass.SharedKernel.Domain;
using Compass.SharedKernel.Domain.Exceptions;

namespace Compass.Modules.Planning.Domain.Projects;

public sealed class Project : Entity
{
    public string Title { get; private set; }
    public ProjectStatus Status { get; private set; }

    private Project() { Title = string.Empty; }

    public Project(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Project title cannot be empty.");

        Id = Guid.NewGuid();
        Title = title;
        Status = ProjectStatus.Planning;
    }

    public void Activate()
    {
        if (Status is ProjectStatus.Completed or ProjectStatus.Archived)
            throw new DomainException($"Cannot activate a {Status} project.");
            
        Status = ProjectStatus.Active;
    }

    public void Pause()
    {
        if (Status != ProjectStatus.Active)
            throw new DomainException("Only active projects can be paused.");

        Status = ProjectStatus.Paused;
    }

    public void Resume()
    {
        if (Status != ProjectStatus.Paused)
            throw new DomainException("Only paused projects can be resumed.");

        Status = ProjectStatus.Active;
    }

    public void Complete()
    {
        if (Status == ProjectStatus.Archived)
            throw new DomainException("Cannot complete an archived project.");

        Status = ProjectStatus.Completed;
    }

    public void Archive()
    {
        Status = ProjectStatus.Archived;
    }
}
