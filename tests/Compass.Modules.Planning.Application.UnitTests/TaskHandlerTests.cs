using Compass.Modules.Planning.Application.Commands;
using Compass.Modules.Planning.Application.Queries;
using Compass.Modules.Planning.Domain.Model;
using Compass.Modules.Planning.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;
using TaskModel = Compass.Modules.Planning.Domain.Model.Task;
using TaskStatus = Compass.Modules.Planning.Domain.Model.TaskStatus;

namespace Compass.Modules.Planning.Application.UnitTests;

public class TaskHandlerTests
{
    private readonly Mock<ITaskRepository> _repositoryMock;

    public TaskHandlerTests()
    {
        _repositoryMock = new Mock<ITaskRepository>();
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTaskCommandHandler_ShouldPersistAndReturnDto()
    {
        // Arrange
        var handler = new CreateTaskCommandHandler(_repositoryMock.Object);
        var command = new CreateTaskCommand("Escrever testes", "Testes unitários");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Title.Should().Be("Escrever testes");
        result.Description.Should().Be("Testes unitários");
        result.Status.Should().Be("Draft");

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<TaskModel>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTasksQueryHandler_ShouldReturnMappedDtos()
    {
        // Arrange
        var task1 = TaskModel.Create("Tarefa 1");
        var task2 = TaskModel.Create("Tarefa 2", durationMinutes: 30);
        _repositoryMock.Setup(r => r.ListAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskModel> { task1, task2 });

        var handler = new GetTasksQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.HandleAsync(new GetTasksQuery());

        // Assert
        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Tarefa 1");
        result[0].Status.Should().Be("Draft");
        result[1].Title.Should().Be("Tarefa 2");
        result[1].Status.Should().Be("Ready");
    }
}
