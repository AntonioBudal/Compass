using Compass.Domain.Entities;
using Compass.Domain.Enums;
using Compass.Domain.Exceptions;
using Compass.Tests.Shared;
using Compass.Tests.Shared.Builders;
using FluentAssertions;

namespace Compass.Domain.Tests.Entities;

public class TaskCommitmentTests
{
    [Fact]
    public void Constructor_ShouldThrowDomainException_WhenDurationIsLessThan5Minutes()
    {
        // Arrange & Act
        Action action = () => new TaskCommitment(TestConstants.DefaultUserId, "Ler livro", 4, 2);

        // Assert
        action.Should().Throw<DomainException>()
              .WithMessage("*maior ou igual a 5 minutos*");
    }

    [Fact]
    public void Constructor_ShouldThrowDomainException_WhenEnergyIsInvalid()
    {
        // Arrange & Act
        Action action = () => new TaskCommitment(TestConstants.DefaultUserId, "Ler livro", 30, 4);

        // Assert
        action.Should().Throw<DomainException>()
              .WithMessage("*energia requerida deve estar entre 1*");
    }

    [Fact]
    public void Complete_ShouldSetStatusToCompletedAndSetDate()
    {
        // Arrange
        var task = new TaskCommitmentBuilder().WithStatus(CommitmentStatus.Pending).Build();

        // Act
        task.Complete();

        // Assert
        task.Status.Should().Be(CommitmentStatus.Completed);
        task.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateTaskDetails_ShouldModifyProperties_WhenParametersAreValid()
    {
        // Arrange
        var task = new TaskCommitmentBuilder()
            .WithDuration(30)
            .WithEnergy(2)
            .Build();

        // Act
        task.UpdateTaskDetails(60, 3, null);

        // Assert
        task.EstimatedDurationMinutes.Should().Be(60);
        task.EnergyRequired.Should().Be(3);
    }
}