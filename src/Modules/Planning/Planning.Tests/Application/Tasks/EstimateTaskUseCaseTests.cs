using System;
using Compass.Modules.Planning.Application.Tasks.EstimateTask;
using Compass.SharedKernel.Domain.Exceptions;
using Xunit;
using Task = Compass.Modules.Planning.Domain.Tasks.Task;
using TaskStatus = Compass.Modules.Planning.Domain.Tasks.TaskStatus;

namespace Compass.Modules.Planning.Tests.Application.Tasks;

public class EstimateTaskUseCaseTests
{
    private readonly FakeTaskRepository _repository;
    private readonly EstimateTaskUseCase _useCase;

    public EstimateTaskUseCaseTests()
    {
        _repository = new FakeTaskRepository();
        _useCase = new EstimateTaskUseCase(_repository);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Estimate_And_Transition_Task_To_Ready()
    {
        // Arrange
        var task = new Task("Draft Task");
        await _repository.AddAsync(task);
        
        var command = new EstimateTaskCommand(task.Id, 30);

        // Act
        await _useCase.ExecuteAsync(command);

        // Assert
        var updatedTask = await _repository.GetByIdAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal(30, updatedTask.EstimatedDurationMinutes);
        Assert.Equal(TaskStatus.Ready, updatedTask.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Throw_Exception_When_Task_Not_Found()
    {
        // Arrange
        var command = new EstimateTaskCommand(Guid.NewGuid(), 30);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await _useCase.ExecuteAsync(command));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Bubble_Up_Domain_Exception_When_Duration_Is_Invalid()
    {
        // Arrange
        var task = new Task("Draft Task");
        await _repository.AddAsync(task);
        
        var command = new EstimateTaskCommand(task.Id, -5); // Invariante: Duração negativa não permitida

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(async () => await _useCase.ExecuteAsync(command));
    }
}
