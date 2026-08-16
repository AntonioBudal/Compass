using System;
using System.Linq;
using Compass.Modules.Calendar.Domain.Time;
using Xunit;

namespace Compass.Modules.Calendar.Tests.Domain.Time;

public class TimeIntervalTests
{
    private static DateTimeOffset T(int hour) => new DateTimeOffset(2026, 1, 1, hour, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Subtract_Tangent_ShouldReturnIntactAdjusted()
    {
        var slot = new TimeInterval(T(8), T(12));
        var commitment = new TimeInterval(T(8), T(9)); // Corta o começo

        var result = slot.Subtract(commitment);
        
        var remaining = Assert.Single(result);
        Assert.Equal(T(9), remaining.Start);
        Assert.Equal(T(12), remaining.End);
    }

    [Fact]
    public void Subtract_Split_ShouldReturnTwoIntervals()
    {
        var slot = new TimeInterval(T(8), T(12));
        var commitment = new TimeInterval(T(9), T(10)); // Corta no meio

        var result = slot.Subtract(commitment);
        
        Assert.Equal(2, result.Count);
        Assert.Equal(T(8), result[0].Start);
        Assert.Equal(T(9), result[0].End);
        
        Assert.Equal(T(10), result[1].Start);
        Assert.Equal(T(12), result[1].End);
    }

    [Fact]
    public void Subtract_Devastating_ShouldReturnEmpty()
    {
        var slot = new TimeInterval(T(10), T(12));
        var commitment = new TimeInterval(T(9), T(13)); // Engole inteiro

        var result = slot.Subtract(commitment);
        
        Assert.Empty(result);
    }

    [Fact]
    public void Merge_Overlapping_ShouldFlatten()
    {
        var intervals = new[]
        {
            new TimeInterval(T(8), T(10)),
            new TimeInterval(T(9), T(11)), // Sobrepõe o primeiro
            new TimeInterval(T(11), T(12)) // Toca o segundo
        };

        var merged = TimeInterval.Merge(intervals);
        
        var block = Assert.Single(merged);
        Assert.Equal(T(8), block.Start);
        Assert.Equal(T(12), block.End);
    }
}
