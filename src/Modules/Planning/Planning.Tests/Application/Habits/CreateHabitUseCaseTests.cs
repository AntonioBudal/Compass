using System;
using Compass.Modules.Planning.Application.Habits.CreateHabit;
using Compass.Modules.Planning.Domain.Habits;
using Compass.SharedKernel.Domain.Exceptions;
using Xunit;
using HabitStatus = Compass.Modules.Planning.Domain.Habits.HabitStatus;

namespace Compass.Modules.Planning.Tests.Application.Habits;

public class CreateHabitUseCaseTests
{
    private readonly FakeHabitRepository _repository;
    private readonly CreateHabitUseCase _useCase;

    public CreateHabitUseCaseTests()
    {
        _repository = new FakeHabitRepository();
        _useCase = new CreateHabitUseCase(_repository);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Create_Habit_With_Valid_Interval_And_Persist()
    {
        var command = new CreateHabitCommand("Read", 30, IntervalDays: 2);
        
        var result = await _useCase.ExecuteAsync(command);

        Assert.NotEqual(Guid.Empty, result.HabitId);
        Assert.Equal(HabitStatus.Active, result.Status);

        var saved = Assert.Single(_repository.SavedHabits);
        Assert.Equal("Read", saved.Title);
        Assert.Equal(HabitFrequencyType.Interval, saved.Frequency.Type);
        Assert.Equal(2, saved.Frequency.IntervalDays);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Create_Habit_With_Valid_Weekly_Days_And_Persist()
    {
        var command = new CreateHabitCommand("Gym", 60, DaysOfWeek: new[] { DayOfWeek.Monday, DayOfWeek.Wednesday });
        
        var result = await _useCase.ExecuteAsync(command);

        var saved = Assert.Single(_repository.SavedHabits);
        Assert.Equal(HabitFrequencyType.Weekly, saved.Frequency.Type);
        Assert.Contains(DayOfWeek.Monday, saved.Frequency.DaysOfWeek);
        Assert.Contains(DayOfWeek.Wednesday, saved.Frequency.DaysOfWeek);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Bubble_Up_DomainException_For_Invalid_Interval()
    {
        var command = new CreateHabitCommand("Read", 30, IntervalDays: -1); // Inválido
        
        await Assert.ThrowsAsync<DomainException>(async () => await _useCase.ExecuteAsync(command));
        Assert.Empty(_repository.SavedHabits);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Bubble_Up_DomainException_For_Empty_DaysOfWeek()
    {
        var command = new CreateHabitCommand("Gym", 60, DaysOfWeek: Array.Empty<DayOfWeek>()); // Inválido
        
        await Assert.ThrowsAsync<DomainException>(async () => await _useCase.ExecuteAsync(command));
        Assert.Empty(_repository.SavedHabits);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Bubble_Up_DomainException_For_Invalid_Duration()
    {
        var command = new CreateHabitCommand("Gym", -10, IntervalDays: 1); // Inválido
        
        await Assert.ThrowsAsync<DomainException>(async () => await _useCase.ExecuteAsync(command));
        Assert.Empty(_repository.SavedHabits);
    }
}
