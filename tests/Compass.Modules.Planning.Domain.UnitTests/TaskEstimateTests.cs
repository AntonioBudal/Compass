using Compass.Modules.Planning.Domain.Exceptions;
using FluentAssertions;
using Xunit;
using TaskModel = Compass.Modules.Planning.Domain.Model.Task;
using TaskStatus = Compass.Modules.Planning.Domain.Model.TaskStatus;

namespace Compass.Modules.Planning.Domain.UnitTests;

public class TaskEstimateTests
{
    [Fact]
    public void SetEstimate_OnDraftTask_ShouldPromoteToReady()
    {
        // Arrange
        var task = TaskModel.Create("Minha tarefa");
        task.Status.Should().Be(TaskStatus.Draft);

        // Act
        task.SetEstimate(30);

        // Assert
        task.DurationMinutes.Should().Be(30);
        task.Status.Should().Be(TaskStatus.Ready);
    }

    [Fact]
    public void SetEstimate_NullOnReadyTask_ShouldDemoteToDraft()
    {
        // Arrange
        var task = TaskModel.Create("Minha tarefa", durationMinutes: 30);
        task.Status.Should().Be(TaskStatus.Ready);

        // Act
        task.SetEstimate(null);

        // Assert
        task.DurationMinutes.Should().BeNull();
        task.Status.Should().Be(TaskStatus.Draft);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void SetEstimate_WithNonPositiveValue_ShouldThrowPlanningDomainException(int invalidEstimate)
    {
        // Arrange
        var task = TaskModel.Create("Minha tarefa");

        // Act
        var act = () => task.SetEstimate(invalidEstimate);

        // Assert
        act.Should().Throw<PlanningDomainException>()
            .WithMessage("*Duration estimate must be a positive integer*");
    }

    [Fact]
    public void SetEstimate_OnDoneTask_ShouldThrowPlanningDomainException()
    {
        // Arrange
        var task = TaskModel.Create("Minha tarefa", durationMinutes: 30);
        task.Complete();
        task.Status.Should().Be(TaskStatus.Done);

        // Act
        var act = () => task.SetEstimate(45);

        // Assert
        act.Should().Throw<PlanningDomainException>()
            .WithMessage("*Cannot change duration estimate of a completed task*");
    }
}
