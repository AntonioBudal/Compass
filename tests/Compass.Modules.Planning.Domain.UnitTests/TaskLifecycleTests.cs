using Compass.Modules.Planning.Domain.Exceptions;
using FluentAssertions;
using Xunit;
using TaskModel = Compass.Modules.Planning.Domain.Model.Task;
using TaskStatus = Compass.Modules.Planning.Domain.Model.TaskStatus;

namespace Compass.Modules.Planning.Domain.UnitTests;

public class TaskLifecycleTests
{
    [Fact]
    public void Start_OnReadyTask_ShouldTransitionToInProgress()
    {
        // Arrange
        var task = TaskModel.Create("Tarefa pronta", durationMinutes: 30);
        task.Status.Should().Be(TaskStatus.Ready);

        // Act
        task.Start();

        // Assert
        task.Status.Should().Be(TaskStatus.InProgress);
    }

    [Fact]
    public void Start_OnDraftTask_ShouldThrowPlanningDomainException()
    {
        // Arrange
        var task = TaskModel.Create("Tarefa em rascunho");
        task.Status.Should().Be(TaskStatus.Draft);

        // Act
        var act = () => task.Start();

        // Assert
        act.Should().Throw<PlanningDomainException>()
            .WithMessage("*must have a duration estimate and be in Ready status before starting*");
    }

    [Fact]
    public void Start_OnDoneTask_ShouldThrowPlanningDomainException()
    {
        // Arrange
        var task = TaskModel.Create("Tarefa concluída", durationMinutes: 30);
        task.Complete();
        task.Status.Should().Be(TaskStatus.Done);

        // Act
        var act = () => task.Start();

        // Assert
        act.Should().Throw<PlanningDomainException>()
            .WithMessage("*Completed tasks cannot be restarted directly*");
    }

    [Fact]
    public void Complete_OnInProgressTask_ShouldTransitionToDoneAndSetCompletedAt()
    {
        // Arrange
        var task = TaskModel.Create("Tarefa em andamento", durationMinutes: 30);
        task.Start();
        task.Status.Should().Be(TaskStatus.InProgress);

        // Act
        task.Complete();

        // Assert
        task.Status.Should().Be(TaskStatus.Done);
        task.CompletedAt.Should().NotBeNull();
        task.CompletedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Complete_OnReadyTask_ShouldTransitionToDoneDirectly()
    {
        // Arrange
        var task = TaskModel.Create("Tarefa pronta rápida", durationMinutes: 15);

        // Act
        task.Complete();

        // Assert
        task.Status.Should().Be(TaskStatus.Done);
        task.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateDetails_OnActiveTask_ShouldUpdateFields()
    {
        // Arrange
        var task = TaskModel.Create("Título antigo");
        var newDeadline = DateTimeOffset.UtcNow.AddDays(3);

        // Act
        task.UpdateDetails("Título novo", "Descrição nova", 60, newDeadline);

        // Assert
        task.Title.Should().Be("Título novo");
        task.Description.Should().Be("Descrição nova");
        task.DurationMinutes.Should().Be(60);
        task.Deadline.Should().BeCloseTo(newDeadline, TimeSpan.FromSeconds(2));
        task.Status.Should().Be(TaskStatus.Ready);
    }
}
