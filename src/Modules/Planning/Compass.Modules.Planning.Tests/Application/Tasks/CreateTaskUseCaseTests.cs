using System;
using Compass.Modules.Planning.Application.Tasks;
using Compass.Modules.Planning.Application.Tasks.CreateTask;
using Compass.Modules.Planning.Domain.Tasks;
using Compass.SharedKernel.Domain.Exceptions;
using Xunit;
using Task = Compass.Modules.Planning.Domain.Tasks.Task;
using TaskStatus = Compass.Modules.Planning.Domain.Tasks.TaskStatus;

namespace Compass.Modules.Planning.Tests.Application.Tasks;

public class CreateTaskUseCaseTests
{
    private readonly FakeTaskRepository _repository;
    private readonly CreateTaskUseCase _useCase;

    public CreateTaskUseCaseTests()
    {
        _repository = new FakeTaskRepository();
        _useCase = new CreateTaskUseCase(_repository);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Create_Draft_Task_When_No_Estimation_Is_Provided()
    {
        var command = new CreateTaskCommand("Buy groceries");
        var result = await _useCase.ExecuteAsync(command);

        Assert.NotEqual(Guid.Empty, result.TaskId);
        Assert.Equal(TaskStatus.Draft, result.Status);

        var savedTask = Assert.Single(_repository.SavedTasks);
        Assert.Equal("Buy groceries", savedTask.Title);
        Assert.Equal(TaskStatus.Draft, savedTask.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Create_Ready_Task_When_Estimation_Is_Provided()
    {
        var command = new CreateTaskCommand("Write report", null, null, 45);
        var result = await _useCase.ExecuteAsync(command);

        Assert.Equal(TaskStatus.Ready, result.Status);
        
        var savedTask = Assert.Single(_repository.SavedTasks);
        Assert.Equal(45, savedTask.EstimatedDurationMinutes);
        Assert.Equal(TaskStatus.Ready, savedTask.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Bubble_Up_Domain_Exception_When_Title_Is_Invalid()
    {
        var command = new CreateTaskCommand("");
        await Assert.ThrowsAsync<DomainException>(async () => await _useCase.ExecuteAsync(command));
        Assert.Empty(_repository.SavedTasks);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Bubble_Up_Domain_Exception_When_Estimation_Is_Invalid()
    {
        var command = new CreateTaskCommand("Valid title", null, null, -10);
        await Assert.ThrowsAsync<DomainException>(async () => await _useCase.ExecuteAsync(command));
        Assert.Empty(_repository.SavedTasks);
    }
}
