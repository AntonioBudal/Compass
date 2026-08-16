using Compass.Modules.Planning.Domain.Habits;
using Compass.SharedKernel.Domain.Exceptions;
using Xunit;

namespace Compass.Modules.Planning.Tests.Domain.Habits;

public class HabitTests
{
    private readonly HabitFrequency _validFrequency = HabitFrequency.CreateInterval(1);

    [Fact]
    public void Should_Create_Active_Habit_When_Valid()
    {
        var habit = new Habit("Read Book", 30, _validFrequency);
        Assert.Equal("Read Book", habit.Title);
        Assert.Equal(30, habit.EstimatedDurationMinutes);
        Assert.Equal(HabitStatus.Active, habit.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Should_Reject_Creation_With_Invalid_Duration(int invalidDuration)
    {
        Assert.Throws<DomainException>(() => new Habit("Read Book", invalidDuration, _validFrequency));
    }

    [Fact]
    public void Should_Transition_Active_To_Paused_And_Back()
    {
        var habit = new Habit("Read Book", 30, _validFrequency);
        
        habit.Pause();
        Assert.Equal(HabitStatus.Paused, habit.Status);
        
        habit.Resume();
        Assert.Equal(HabitStatus.Active, habit.Status);
    }

    [Fact]
    public void Should_Not_Change_Frequency_Of_Archived_Habit()
    {
        var habit = new Habit("Read Book", 30, _validFrequency);
        habit.Archive();

        var newFreq = HabitFrequency.CreateInterval(2);
        Assert.Throws<DomainException>(() => habit.ChangeFrequency(newFreq));
    }
}
