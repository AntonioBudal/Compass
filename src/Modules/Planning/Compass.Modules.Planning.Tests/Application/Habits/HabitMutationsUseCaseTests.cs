using System;
using Compass.Modules.Planning.Application.Habits.ArchiveHabit;
using Compass.Modules.Planning.Application.Habits.ChangeHabitFrequency;
using Compass.Modules.Planning.Application.Habits.PauseHabit;
using Compass.Modules.Planning.Application.Habits.ResumeHabit;
using Compass.Modules.Planning.Domain.Habits;
using Compass.SharedKernel.Domain.Exceptions;
using Xunit;
using HabitStatus = Compass.Modules.Planning.Domain.Habits.HabitStatus;

namespace Compass.Modules.Planning.Tests.Application.Habits;

public class HabitMutationsUseCaseTests
{
    private readonly FakeHabitRepository _repository;

    public HabitMutationsUseCaseTests()
    {
        _repository = new FakeHabitRepository();
    }

    private Habit CreateActiveHabit() 
        => new Habit("Test Habit", 30, HabitFrequency.CreateInterval(1));

    [Fact]
    public async System.Threading.Tasks.Task Should_Pause_Active_Habit()
    {
        var habit = CreateActiveHabit();
        await _repository.AddAsync(habit);
        
        var useCase = new PauseHabitUseCase(_repository);
        await useCase.ExecuteAsync(new PauseHabitCommand(habit.Id));

        var updated = await _repository.GetByIdAsync(habit.Id);
        Assert.Equal(HabitStatus.Paused, updated!.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Resume_Paused_Habit()
    {
        var habit = CreateActiveHabit();
        habit.Pause();
        await _repository.AddAsync(habit);
        
        var useCase = new ResumeHabitUseCase(_repository);
        await useCase.ExecuteAsync(new ResumeHabitCommand(habit.Id));

        var updated = await _repository.GetByIdAsync(habit.Id);
        Assert.Equal(HabitStatus.Active, updated!.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Archive_Habit()
    {
        var habit = CreateActiveHabit();
        await _repository.AddAsync(habit);
        
        var useCase = new ArchiveHabitUseCase(_repository);
        await useCase.ExecuteAsync(new ArchiveHabitCommand(habit.Id));

        var updated = await _repository.GetByIdAsync(habit.Id);
        Assert.Equal(HabitStatus.Archived, updated!.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Change_Frequency_To_Interval()
    {
        var habit = CreateActiveHabit(); // Starts with interval 1
        await _repository.AddAsync(habit);
        
        var useCase = new ChangeHabitFrequencyUseCase(_repository);
        await useCase.ExecuteAsync(new ChangeHabitFrequencyCommand(habit.Id, IntervalDays: 5));

        var updated = await _repository.GetByIdAsync(habit.Id);
        Assert.Equal(HabitFrequencyType.Interval, updated!.Frequency.Type);
        Assert.Equal(5, updated!.Frequency.IntervalDays);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Change_Frequency_To_Weekly()
    {
        var habit = CreateActiveHabit();
        await _repository.AddAsync(habit);
        
        var useCase = new ChangeHabitFrequencyUseCase(_repository);
        await useCase.ExecuteAsync(new ChangeHabitFrequencyCommand(habit.Id, DaysOfWeek: new[] { DayOfWeek.Friday }));

        var updated = await _repository.GetByIdAsync(habit.Id);
        Assert.Equal(HabitFrequencyType.Weekly, updated!.Frequency.Type);
        Assert.Contains(DayOfWeek.Friday, updated!.Frequency.DaysOfWeek);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Bubble_Up_DomainException_When_Pausing_Paused_Habit()
    {
        var habit = CreateActiveHabit();
        habit.Pause();
        await _repository.AddAsync(habit);
        
        var useCase = new PauseHabitUseCase(_repository);
        await Assert.ThrowsAsync<DomainException>(async () => await useCase.ExecuteAsync(new PauseHabitCommand(habit.Id)));
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Bubble_Up_DomainException_When_Resuming_Active_Habit()
    {
        var habit = CreateActiveHabit(); // Already active
        await _repository.AddAsync(habit);
        
        var useCase = new ResumeHabitUseCase(_repository);
        await Assert.ThrowsAsync<DomainException>(async () => await useCase.ExecuteAsync(new ResumeHabitCommand(habit.Id)));
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Bubble_Up_DomainException_When_Changing_Frequency_With_Invalid_Interval()
    {
        var habit = CreateActiveHabit();
        await _repository.AddAsync(habit);
        
        var useCase = new ChangeHabitFrequencyUseCase(_repository);
        await Assert.ThrowsAsync<DomainException>(async () => await useCase.ExecuteAsync(new ChangeHabitFrequencyCommand(habit.Id, IntervalDays: -5)));
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Bubble_Up_DomainException_When_Changing_Frequency_With_Empty_Days()
    {
        var habit = CreateActiveHabit();
        await _repository.AddAsync(habit);
        
        var useCase = new ChangeHabitFrequencyUseCase(_repository);
        await Assert.ThrowsAsync<DomainException>(async () => await useCase.ExecuteAsync(new ChangeHabitFrequencyCommand(habit.Id, DaysOfWeek: Array.Empty<DayOfWeek>())));
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Bubble_Up_DomainException_When_Changing_Frequency_Of_Archived_Habit()
    {
        var habit = CreateActiveHabit();
        habit.Archive();
        await _repository.AddAsync(habit);
        
        var useCase = new ChangeHabitFrequencyUseCase(_repository);
        await Assert.ThrowsAsync<DomainException>(async () => await useCase.ExecuteAsync(new ChangeHabitFrequencyCommand(habit.Id, IntervalDays: 2)));
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_Throw_Exception_When_Habit_Not_Found()
    {
        var useCase = new PauseHabitUseCase(_repository);
        var ex = await Assert.ThrowsAsync<Exception>(async () => await useCase.ExecuteAsync(new PauseHabitCommand(Guid.NewGuid())));
        Assert.Contains("not found", ex.Message);
    }
}
