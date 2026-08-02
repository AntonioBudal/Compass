using Compass.Domain.Entities;
using Compass.Domain.Enums;
using Compass.Domain.Exceptions;
using Compass.Tests.Shared;
using Compass.Tests.Shared.Builders;
using FluentAssertions;

namespace Compass.Domain.Tests.Entities;

public class GoalTests
{
    [Fact]
    public void Constructor_ShouldThrowDomainException_WhenTitleIsTooShort()
    {
        // Arrange & Act
        Action action = () => new Goal(TestConstants.DefaultUserId, "AB");

        // Assert
        action.Should().Throw<DomainException>()
              .WithMessage("*pelo menos 3 caracteres*");
    }

    [Fact]
    public void UpdateProgress_ShouldSetToCompleted_WhenProgressReaches100()
    {
        // Arrange
        var goal = new GoalBuilder().Build();

        // Act
        goal.UpdateProgress(100.00m);

        // Assert
        goal.ProgressPercentage.Should().Be(100.00m);
        goal.Status.Should().Be(GoalStatus.Completed);
    }

    [Fact]
    public void UpdateProgress_ShouldThrowDomainException_WhenProgressIsInvalid()
    {
        // Arrange
        var goal = new GoalBuilder().Build();

        // Act
        Action action = () => goal.UpdateProgress(105.00m);

        // Assert
        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateGoalDetails_ShouldUpdateProperties_WhenValid()
    {
        // Arrange
        var goal = new GoalBuilder().WithTitle("Meta Antiga").Build();
        var newTarget = DateTime.UtcNow.AddDays(10);

        // Act
        goal.UpdateGoalDetails("Meta Nova Atualizada", "Novo porquê", newTarget);

        // Assert
        goal.Title.Should().Be("Meta Nova Atualizada");
        goal.WhyDescription.Should().Be("Novo porquê");
        goal.TargetDate.Should().Be(newTarget);
    }
}