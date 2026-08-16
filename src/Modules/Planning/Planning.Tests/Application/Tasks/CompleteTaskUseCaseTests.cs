using System;
using Compass.Modules.Planning.Application.Tasks.CompleteTask;
using Compass.SharedKernel.Domain.Exceptions;
using Xunit;
using Task = Compass.Modules.Planning.Domain.Tasks.Task;
using TaskStatus = Compass.Modules.Planning.Domain.Tasks.TaskStatus;

namespace Compass.Modules.Planning.Tests.Application.Tasks;

public class CompleteTaskUseCaseTests
{
    private readonly FakeTaskRepository _repository;
    private readonly CompleteTaskUseCase _useCase;

    public CompleteTaskUseCaseTests()
    {
        _repository = new FakeTaskRepository();
        _useCase = new CompleteTaskUseCase(_repository);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Complete_InProgress_Task()
    {
        // Arrange
        var task = new Task("Doing work");
        task.EstimateTime(30); // Ready
        task.RegisterProgress(); // InProgress
        await _repository.AddAsync(task);

        var command = new CompleteTaskCommand(task.Id);

        // Act
        await _useCase.ExecuteAsync(command);

        // Assert
        var updatedTask = await _repository.GetByIdAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal(TaskStatus.Completed, updatedTask.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Throw_Domain_Exception_When_Completing_Draft()
    {
        // Arrange
        var task = new Task("Draft Task"); // Draft (sem estimativa)
        await _repository.AddAsync(task);

        var command = new CompleteTaskCommand(task.Id);

        // Act & Assert
        // A aplicação tenta completar, mas a entidade recusa porque não há estimativa.
        var ex = await Assert.ThrowsAsync<DomainException>(async () => await _useCase.ExecuteAsync(command));
        Assert.Contains("requires an estimation", ex.Message);
    }
}
