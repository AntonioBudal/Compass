using System;
using Compass.Modules.Execution.Domain.DailyCycles;
using Compass.Modules.Execution.Domain.Time;
using Compass.SharedKernel.Domain.Exceptions;
using Xunit;

namespace Compass.Modules.Execution.Tests.Domain.DailyCycles;

public class DailyCycleTests
{
    private static DateTimeOffset T(int hour, int minute = 0) 
        => new DateTimeOffset(2026, 8, 18, hour, minute, 0, TimeSpan.Zero); // 2026-08-18 (Data base)

    private DailyCycle CreateActiveCycle()
    {
        var cycle = new DailyCycle(new DateOnly(2026, 8, 18));
        cycle.Start();
        return cycle;
    }

    [Fact]
    public void Start_ShouldTransitionToActive()
    {
        var cycle = new DailyCycle(new DateOnly(2026, 8, 18));
        Assert.Equal(CycleStatus.NotStarted, cycle.Status);

        cycle.Start();
        Assert.Equal(CycleStatus.Active, cycle.Status);
    }

    [Fact]
    public void Start_ShouldThrow_IfAlreadyActive()
    {
        var cycle = CreateActiveCycle();
        Assert.Throws<DomainException>(() => cycle.Start());
    }

    [Fact]
    public void RecordExecution_ShouldAddLog_WhenValid()
    {
        var cycle = CreateActiveCycle();
        var interval = new TimeInterval(T(14), T(15)); // 14h-15h

        cycle.RecordExecution(Guid.NewGuid(), interval, ExecutionType.DeepWork);

        var log = Assert.Single(cycle.Logs);
        Assert.Equal(ExecutionType.DeepWork, log.Type);
        Assert.Equal(T(14), log.Interval.Start);
        Assert.Equal(T(15), log.Interval.End);
    }

    [Fact]
    public void RecordExecution_ShouldThrow_WhenCycleNotActive()
    {
        var cycle = new DailyCycle(new DateOnly(2026, 8, 18)); // Status = NotStarted
        var interval = new TimeInterval(T(14), T(15));

        Assert.Throws<DomainException>(() => 
            cycle.RecordExecution(Guid.NewGuid(), interval, ExecutionType.Routine));
    }

    [Fact]
    public void RecordExecution_ShouldThrow_WhenIntervalOverlaps()
    {
        var cycle = CreateActiveCycle();
        
        // Cadastra um log das 14:00 as 15:00
        cycle.RecordExecution(Guid.NewGuid(), new TimeInterval(T(14), T(15)), ExecutionType.DeepWork);

        // Tenta cadastrar das 14:30 as 15:30 (Sobreposição parcial)
        var overlappingInterval = new TimeInterval(T(14, 30), T(15, 30));

        var exception = Assert.Throws<DomainException>(() => 
            cycle.RecordExecution(Guid.NewGuid(), overlappingInterval, ExecutionType.Routine));
            
        Assert.Contains("overlaps", exception.Message);
        Assert.Single(cycle.Logs); // A transação atômica impediu a corrupção do estado original
    }

    [Fact]
    public void RecordExecution_ShouldThrow_WhenIntervalIsFromDifferentDate()
    {
        var cycle = CreateActiveCycle(); // Ciclo do dia 18
        
        // Tenta registrar log no dia 19
        var wrongDateStart = new DateTimeOffset(2026, 8, 19, 10, 0, 0, TimeSpan.Zero);
        var wrongDateEnd = new DateTimeOffset(2026, 8, 19, 11, 0, 0, TimeSpan.Zero);
        var interval = new TimeInterval(wrongDateStart, wrongDateEnd);

        var exception = Assert.Throws<DomainException>(() => 
            cycle.RecordExecution(Guid.NewGuid(), interval, ExecutionType.Routine));
            
        Assert.Contains("inside a cycle meant for", exception.Message);
    }

    [Fact]
    public void Close_ShouldFreezeCycle_PreventingNewLogs()
    {
        var cycle = CreateActiveCycle();
        cycle.Close();

        Assert.Equal(CycleStatus.Closed, cycle.Status);

        Assert.Throws<DomainException>(() => 
            cycle.RecordExecution(Guid.NewGuid(), new TimeInterval(T(14), T(15)), ExecutionType.Routine));
    }
}
