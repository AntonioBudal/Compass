using System;
using System.Collections.Generic;
using System.Linq;
using Compass.Modules.Calendar.Domain.Availability;
using Xunit;

namespace Compass.Modules.Calendar.Domain.Tests;

public class AvailabilityCalculatorTests
{
    private readonly AvailabilityCalculator _calculator = new();

    [Fact]
    public void Base_Window_Without_Blocks_Should_Return_Intact()
    {
        var baseWindows = new[] { new TimeWindow(new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0)) };
        
        var result = _calculator.Calculate(baseWindows, Array.Empty<TimeWindow>());

        Assert.Single(result);
        Assert.Equal(TimeSpan.FromHours(8), result[0].Start);
        Assert.Equal(TimeSpan.FromHours(12), result[0].End);
    }

    [Fact]
    public void Block_Inside_Base_Should_Split_Window_In_Two()
    {
        var baseWindows = new[] { new TimeWindow(new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0)) };
        var blockedWindows = new[] { new TimeWindow(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0)) };
        
        var result = _calculator.Calculate(baseWindows, blockedWindows);

        Assert.Equal(2, result.Count);
        Assert.Equal(TimeSpan.FromHours(8), result[0].Start);
        Assert.Equal(TimeSpan.FromHours(9), result[0].End);
        
        Assert.Equal(TimeSpan.FromHours(10), result[1].Start);
        Assert.Equal(TimeSpan.FromHours(12), result[1].End);
    }

    [Fact]
    public void Block_Overlapping_End_Should_Cut_End()
    {
        var baseWindows = new[] { new TimeWindow(new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0)) };
        var blockedWindows = new[] { new TimeWindow(new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0)) };
        
        var result = _calculator.Calculate(baseWindows, blockedWindows);

        Assert.Equal(2, result.Count);
        Assert.Equal(TimeSpan.FromHours(8), result[0].Start);
        Assert.Equal(TimeSpan.FromHours(10), result[0].End);
        
        Assert.Equal(TimeSpan.FromHours(11), result[1].Start);
        Assert.Equal(TimeSpan.FromHours(12), result[1].End);
    }

    [Fact]
    public void Multiple_Blocks_Should_Cut_Correspondingly()
    {
        var baseWindows = new[] { new TimeWindow(new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0)) };
        var blockedWindows = new[] 
        { 
            new TimeWindow(new TimeSpan(9, 0, 0), new TimeSpan(9, 30, 0)),
            new TimeWindow(new TimeSpan(11, 0, 0), new TimeSpan(11, 30, 0))
        };
        
        var result = _calculator.Calculate(baseWindows, blockedWindows);

        Assert.Equal(3, result.Count);
        // 08:00 - 09:00
        Assert.Equal(new TimeSpan(8,0,0), result[0].Start);
        Assert.Equal(new TimeSpan(9,0,0), result[0].End);
        // 09:30 - 11:00
        Assert.Equal(new TimeSpan(9,30,0), result[1].Start);
        Assert.Equal(new TimeSpan(11,0,0), result[1].End);
        // 11:30 - 12:00
        Assert.Equal(new TimeSpan(11,30,0), result[2].Start);
        Assert.Equal(new TimeSpan(12,0,0), result[2].End);
    }

    [Fact]
    public void Overlapping_Blocks_Should_Merge_Before_Cutting()
    {
        var baseWindows = new[] { new TimeWindow(new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0)) };
        var blockedWindows = new[] 
        { 
            new TimeWindow(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0)),
            new TimeWindow(new TimeSpan(9, 30, 0), new TimeSpan(10, 30, 0)) // Sobrepõe o primeiro
        };
        
        var result = _calculator.Calculate(baseWindows, blockedWindows);

        Assert.Equal(2, result.Count);
        // Espera-se 08:00-09:00 e 10:30-12:00
        Assert.Equal(new TimeSpan(8,0,0), result[0].Start);
        Assert.Equal(new TimeSpan(9,0,0), result[0].End);
        
        Assert.Equal(new TimeSpan(10,30,0), result[1].Start);
        Assert.Equal(new TimeSpan(12,0,0), result[1].End);
    }

    [Fact]
    public void Block_Outside_Base_Window_Should_Be_Ignored()
    {
        var baseWindows = new[] { new TimeWindow(new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0)) };
        var blockedWindows = new[] { new TimeWindow(new TimeSpan(13, 0, 0), new TimeSpan(14, 0, 0)) };
        
        var result = _calculator.Calculate(baseWindows, blockedWindows);

        Assert.Single(result);
        Assert.Equal(TimeSpan.FromHours(8), result[0].Start);
        Assert.Equal(TimeSpan.FromHours(12), result[0].End);
    }

    [Fact]
    public void Block_Covering_Entire_Base_Window_Should_Remove_It()
    {
        var baseWindows = new[] { new TimeWindow(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)) };
        var blockedWindows = new[] { new TimeWindow(new TimeSpan(7, 0, 0), new TimeSpan(11, 0, 0)) };
        
        var result = _calculator.Calculate(baseWindows, blockedWindows);

        Assert.Empty(result);
    }
}
