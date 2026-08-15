using Compass.SharedKernel.Domain.Exceptions;
using Xunit;
using Task = Compass.Modules.Planning.Domain.Tasks.Task;
using TaskStatus = Compass.Modules.Planning.Domain.Tasks.TaskStatus;

namespace Compass.Modules.Planning.Tests.Domain.Tasks;

public class TaskTests
{
    [Fact]
    public void Should_Start_As_Draft_When_Created()
    {
        var task = new Task("Learn F#");
        Assert.Equal("Learn F#", task.Title);
        Assert.Equal(TaskStatus.Draft, task.Status);
        Assert.Null(task.EstimatedDurationMinutes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Reject_Creation_With_Invalid_Title(string invalidTitle)
    {
        Assert.Throws<DomainException>(() => new Task(invalidTitle));
    }

    [Fact]
    public void Should_Become_Ready_When_Estimated()
    {
        var task = new Task("Write tests");
        task.EstimateTime(30);
        Assert.Equal(30, task.EstimatedDurationMinutes);
        Assert.Equal(TaskStatus.Ready, task.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Should_Reject_Invalid_Duration(int invalidDuration)
    {
        var task = new Task("Write tests");
        Assert.Throws<DomainException>(() => task.EstimateTime(invalidDuration));
    }

    [Fact]
    public void Should_Not_Allow_Progress_On_Draft()
    {
        var task = new Task("Draft Task");
        Assert.Throws<DomainException>(() => task.RegisterProgress());
    }

    [Fact]
    public void Should_Not_Complete_Draft()
    {
        var task = new Task("Draft Task");
        Assert.Throws<DomainException>(() => task.Complete());
    }

    [Fact]
    public void Should_Transition_To_InProgress_And_Then_Completed()
    {
        var task = new Task("Valid Task");
        task.EstimateTime(15);
        task.RegisterProgress();
        Assert.Equal(TaskStatus.InProgress, task.Status);
        
        task.Complete();
        Assert.Equal(TaskStatus.Completed, task.Status);
    }

    [Fact]
    public void Should_Not_Edit_Completed_Task()
    {
        var task = new Task("Completed Task");
        task.EstimateTime(15);
        task.Complete();

        Assert.Throws<DomainException>(() => task.EstimateTime(30));
        Assert.Throws<DomainException>(() => task.ChangeDeadline(DateTimeOffset.UtcNow));
        Assert.Throws<DomainException>(() => task.RegisterProgress());
    }

    [Fact]
    public void Should_Reopen_Completed_Task_To_Ready()
    {
        var task = new Task("Task to reopen");
        task.EstimateTime(15);
        task.Complete();
        task.Reopen();

        Assert.Equal(TaskStatus.Ready, task.Status);
    }

    [Fact]
    public void Should_Restore_Correct_Status_When_Unarchived()
    {
        var draftTask = new Task("Draft");
        draftTask.Archive();
        draftTask.Unarchive();
        Assert.Equal(TaskStatus.Draft, draftTask.Status);

        var readyTask = new Task("Ready");
        readyTask.EstimateTime(10);
        readyTask.Archive();
        readyTask.Unarchive();
        Assert.Equal(TaskStatus.Ready, readyTask.Status);
    }
}
