using Compass.Modules.Planning.Application.Commands;
using Compass.Modules.Planning.Domain.Exceptions;
using Compass.Modules.Planning.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;
using TaskModel = Compass.Modules.Planning.Domain.Model.Task;

namespace Compass.Modules.Planning.Application.UnitTests;

public class SetTaskEstimateCommandHandlerTests
{
    private readonly Mock<ITaskRepository> _repositoryMock;

    public SetTaskEstimateCommandHandlerTests()
    {
        _repositoryMock = new Mock<ITaskRepository>();
    }

    [Fact]
    public async System.Threading.Tasks.Task SetTaskEstimate_WhenTaskExists_ShouldUpdateEstimateAndReturnDto()
    {
        // Arrange
        var task = TaskModel.Create("Tarefa inicial");
        _repositoryMock.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var handler = new SetTaskEstimateCommandHandler(_repositoryMock.Object);
        var command = new SetTaskEstimateCommand(task.Id, 45);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.DurationMinutes.Should().Be(45);
        result.Status.Should().Be("Ready");
        _repositoryMock.Verify(r => r.UpdateAsync(task, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task SetTaskEstimate_WhenTaskNotFound_ShouldThrowPlanningDomainException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskModel?)null);

        var handler = new SetTaskEstimateCommandHandler(_repositoryMock.Object);
        var command = new SetTaskEstimateCommand(Guid.NewGuid(), 45);

        // Act
        var act = () => handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<PlanningDomainException>()
            .WithMessage("*was not found*");
    }
}
