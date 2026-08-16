using Compass.Modules.Planning.Domain.Projects;
using Compass.SharedKernel.Domain.Exceptions;
using Xunit;

namespace Compass.Modules.Planning.Tests.Domain.Projects;

public class ProjectTests
{
    [Fact]
    public void Should_Start_In_Planning_Status()
    {
        var project = new Project("New System");
        Assert.Equal("New System", project.Title);
        Assert.Equal(ProjectStatus.Planning, project.Status);
    }

    [Fact]
    public void Should_Activate_Planning_Project()
    {
        var project = new Project("New System");
        project.Activate();
        Assert.Equal(ProjectStatus.Active, project.Status);
    }

    [Fact]
    public void Should_Transition_Active_To_Paused_And_Back()
    {
        var project = new Project("New System");
        project.Activate();
        
        project.Pause();
        Assert.Equal(ProjectStatus.Paused, project.Status);
        
        project.Resume();
        Assert.Equal(ProjectStatus.Active, project.Status);
    }

    [Fact]
    public void Should_Not_Pause_Planning_Project()
    {
        var project = new Project("New System"); // Status is Planning
        Assert.Throws<DomainException>(() => project.Pause());
    }

    [Fact]
    public void Should_Not_Complete_Archived_Project()
    {
        var project = new Project("Old System");
        project.Archive();
        Assert.Throws<DomainException>(() => project.Complete());
    }
}
