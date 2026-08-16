using System;
using Compass.Modules.Planning.Application.Tasks.StartTask;
using Compass.SharedKernel.Domain.Exceptions;
using Xunit;
using Task = Compass.Modules.Planning.Domain.Tasks.Task;
using TaskStatus = Compass.Modules.Planning.Domain.Tasks.TaskStatus;

namespace Compass.Modules.Planning.Tests.Application.Tasks;

public class StartTaskUseCaseTests
{
    private readonly FakeTaskRepository _repository;
    private readonly StartTaskUseCase _useCase;

    public StartTaskUseCaseTests()
    {
        _repository = new FakeTaskRepository();
        _useCase = new StartTaskUseCase(_repository);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Start_Ready_Task_To_InProgress()
    {
        // Arrange
        var task = new Task("Valid Task");
        task.EstimateTime(30); // Muda para Ready
        await _repository.AddAsync(task);

        var command = new StartTaskCommand(task.Id);

        // Act
        await _useCase.ExecuteAsync(command);

        // Assert
        var updatedTask = await _repository.GetByIdAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal(TaskStatus.InProgress, updatedTask.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Throw_Domain_Exception_When_Starting_Draft()
    {
        // Arrange
        var task = new Task("Draft Task"); // Nasce sem tempo (Draft)
        await _repository.AddAsync(task);

        var command = new StartTaskCommand(task.Id);

        // Act & Assert
        // A Application tenta iniciar, mas a entidade recusa porque fere a invariante
        var ex = await Assert.ThrowsAsync<DomainException>(async () => await _useCase.ExecuteAsync(command));
        Assert.Contains("Estimate time first", ex.Message);
    }
}
