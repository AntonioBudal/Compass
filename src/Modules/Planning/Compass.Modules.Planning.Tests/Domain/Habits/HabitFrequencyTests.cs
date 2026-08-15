using Compass.Modules.Planning.Domain.Habits;
using Compass.SharedKernel.Domain.Exceptions;
using Xunit;

namespace Compass.Modules.Planning.Tests.Domain.Habits;

public class HabitFrequencyTests
{
    [Fact]
    public void CreateInterval_Should_Succeed_For_Valid_Days()
    {
        var freq = HabitFrequency.CreateInterval(3);
        Assert.Equal(HabitFrequencyType.Interval, freq.Type);
        Assert.Equal(3, freq.IntervalDays);
        Assert.Empty(freq.DaysOfWeek);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateInterval_Should_Throw_For_Invalid_Days(int days)
    {
        Assert.Throws<DomainException>(() => HabitFrequency.CreateInterval(days));
    }

    [Fact]
    public void CreateWeekly_Should_Succeed_And_Map_Days_Correctly()
    {
        var inputDays = new[] { DayOfWeek.Monday, DayOfWeek.Wednesday };
        var freq = HabitFrequency.CreateWeekly(inputDays);
        
        Assert.Equal(HabitFrequencyType.Weekly, freq.Type);
        Assert.Null(freq.IntervalDays);
        Assert.Contains(DayOfWeek.Monday, freq.DaysOfWeek);
        Assert.Contains(DayOfWeek.Wednesday, freq.DaysOfWeek);
        Assert.DoesNotContain(DayOfWeek.Tuesday, freq.DaysOfWeek);
    }

    [Fact]
    public void CreateWeekly_Should_Throw_If_No_Days_Provided()
    {
        Assert.Throws<DomainException>(() => HabitFrequency.CreateWeekly(Array.Empty<DayOfWeek>()));
    }
}
