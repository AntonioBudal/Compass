using Compass.Modules.Planning.Application.Commands;
using Compass.Modules.Planning.Domain.Model;
using Compass.Modules.Planning.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;
using TaskModel = Compass.Modules.Planning.Domain.Model.Task;

namespace Compass.Modules.Planning.Application.UnitTests;

public class TaskLifecycleHandlerTests
{
    private readonly Mock<ITaskRepository> _repositoryMock;

    public TaskLifecycleHandlerTests()
    {
        _repositoryMock = new Mock<ITaskRepository>();
    }

    [Fact]
    public async System.Threading.Tasks.Task StartTaskCommandHandler_ShouldStartTask()
    {
        // Arrange
        var task = TaskModel.Create("Tarefa", durationMinutes: 30);
        _repositoryMock.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var handler = new StartTaskCommandHandler(_repositoryMock.Object);

        // Act
        var result = await handler.HandleAsync(new StartTaskCommand(task.Id));

        // Assert
        result.Status.Should().Be("InProgress");
        _repositoryMock.Verify(r => r.UpdateAsync(task, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task CompleteTaskCommandHandler_ShouldCompleteTask()
    {
        // Arrange
        var task = TaskModel.Create("Tarefa", durationMinutes: 30);
        _repositoryMock.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var handler = new CompleteTaskCommandHandler(_repositoryMock.Object);

        // Act
        var result = await handler.HandleAsync(new CompleteTaskCommand(task.Id));

        // Assert
        result.Status.Should().Be("Done");
        result.CompletedAt.Should().NotBeNull();
        _repositoryMock.Verify(r => r.UpdateAsync(task, It.IsAny<CancellationToken>()), Times.Once);
    }
}
