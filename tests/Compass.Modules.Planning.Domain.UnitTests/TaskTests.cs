using Compass.Modules.Planning.Domain.Exceptions;
using Compass.Modules.Planning.Domain.Model;
using FluentAssertions;
using Xunit;
using TaskModel = Compass.Modules.Planning.Domain.Model.Task;
using TaskStatus = Compass.Modules.Planning.Domain.Model.TaskStatus;

namespace Compass.Modules.Planning.Domain.UnitTests;

public class TaskTests
{
    [Fact]
    public void Create_WithoutEstimate_ShouldInitializeAsDraft()
    {
        // Act
        var task = TaskModel.Create("Preparar relatório trimestral");

        // Assert
        task.Id.Should().NotBeEmpty();
        task.Title.Should().Be("Preparar relatório trimestral");
        task.Description.Should().BeNull();
        task.DurationMinutes.Should().BeNull();
        task.Deadline.Should().BeNull();
        task.Status.Should().Be(TaskStatus.Draft);
        task.CompletedAt.Should().BeNull();
        task.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
        task.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_WithValidEstimate_ShouldInitializeAsReady()
    {
        // Act
        var task = TaskModel.Create("Revisar PR #42", "Revisão de código", 45);

        // Assert
        task.Title.Should().Be("Revisar PR #42");
        task.Description.Should().Be("Revisão de código");
        task.DurationMinutes.Should().Be(45);
        task.Status.Should().Be(TaskStatus.Ready);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrWhitespaceTitle_ShouldThrowPlanningDomainException(string? invalidTitle)
    {
        // Act
        var act = () => TaskModel.Create(invalidTitle!);

        // Assert
        act.Should().Throw<PlanningDomainException>()
            .WithMessage("*Task title cannot be empty*");
    }

    [Fact]
    public void Create_WithTitleExceeding255Chars_ShouldThrowPlanningDomainException()
    {
        // Arrange
        var longTitle = new string('A', 256);

        // Act
        var act = () => TaskModel.Create(longTitle);

        // Assert
        act.Should().Throw<PlanningDomainException>()
            .WithMessage("*cannot exceed 255 characters*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(-1)]
    public void Create_WithNonPositiveEstimate_ShouldThrowPlanningDomainException(int nonPositiveEstimate)
    {
        // Act
        var act = () => TaskModel.Create("Tarefa", durationMinutes: nonPositiveEstimate);

        // Assert
        act.Should().Throw<PlanningDomainException>()
            .WithMessage("*Duration estimate must be a positive integer*");
    }
}
