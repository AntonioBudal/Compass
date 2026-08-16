using System;
using System.Linq;
using Compass.Modules.Execution.Domain.DecisionEngine;
using Compass.Modules.Execution.Domain.Time;
using Xunit;

namespace Compass.Modules.Execution.Tests.Domain.DecisionEngine;

public class ExecutionDecisionEngineTests
{
    private static DateTimeOffset T(int hour, int min = 0) => new DateTimeOffset(2026, 8, 18, hour, min, 0, TimeSpan.Zero);

    private readonly ExecutionDecisionEngine _engine = new();
    private readonly DateTimeOffset _now = T(8); // O ponteiro do "Agora"
    private readonly TimeSpan _minDuration = TimeSpan.FromMinutes(15);
    private readonly ExecutionHistory _emptyHistory = new ExecutionHistory(Array.Empty<TimeInterval>());

    [Fact]
    public void Engine_Should_Prioritize_PerfectFit_HighPriority_Task()
    {
        // Arrange
        var slots = new[] { new AvailableSlot(new TimeInterval(T(14), T(16))) }; // Slot de 2 horas
        
        var tasks = new[]
        {
            new TaskCandidate(Guid.NewGuid(), TaskPriority.Low, TimeSpan.FromHours(1)), // Sobra buraco
            new TaskCandidate(Guid.NewGuid(), TaskPriority.High, TimeSpan.FromHours(2)) // Perfect fit!
        };

        // Act
        var result = _engine.GenerateRecommendations(tasks, slots, _emptyHistory, _now, _minDuration);

        // Assert
        var best = result.First();
        Assert.Equal(tasks[1].Id, best.TaskId); // O High priority que encaixou perfeito ganhou
        Assert.Equal(100.0m, best.Score); // (1.0 * 60) + (1.0 * 40)
        Assert.True(best.Factors.IsPerfectFit);
        Assert.False(best.Factors.IsChunked);
    }

    [Fact]
    public void Engine_Should_Perform_Chunking_When_Task_Exceeds_Slot()
    {
        // Arrange
        var slots = new[] { new AvailableSlot(new TimeInterval(T(10), T(11))) }; // Slot 1 hora
        
        var task = new TaskCandidate(Guid.NewGuid(), TaskPriority.High, TimeSpan.FromHours(3)); // Tarefa 3 horas

        // Act
        var result = _engine.GenerateRecommendations(new[] { task }, slots, _emptyHistory, _now, _minDuration);

        // Assert
        var best = Assert.Single(result);
        Assert.Equal(T(10), best.SuggestedInterval.Start);
        Assert.Equal(T(11), best.SuggestedInterval.End); // Invariante (Suggested ⊆ Slot)
        Assert.Equal(TimeSpan.FromHours(1), best.SuggestedInterval.End - best.SuggestedInterval.Start);
        Assert.True(best.Factors.IsChunked);
        Assert.Equal(92.0m, best.Score); // (1.0 * 60) + (0.8 * 40)
    }

    [Fact]
    public void Engine_Should_Discard_Tasks_Already_Completed_And_Past_Slots()
    {
        // Arrange
        var slots = new[] 
        { 
            new AvailableSlot(new TimeInterval(T(6), T(7))), // Passado (Now é 8h)
            new AvailableSlot(new TimeInterval(T(14), T(16))) // Futuro válido
        }; 
        
        var completedTask = new TaskCandidate(Guid.NewGuid(), TaskPriority.High, TimeSpan.Zero);
        var activeTask = new TaskCandidate(Guid.NewGuid(), TaskPriority.Medium, TimeSpan.FromHours(1));

        // Act
        var result = _engine.GenerateRecommendations(new[] { completedTask, activeTask }, slots, _emptyHistory, _now, _minDuration);

        // Assert
        var best = Assert.Single(result);
        Assert.Equal(activeTask.Id, best.TaskId); // A finalizada foi podada sumariamente
        Assert.Equal(T(14), best.SuggestedInterval.Start); // Projetada no slot futuro
    }

    [Fact]
    public void Engine_Should_Subtract_History_From_Slots()
    {
        // Arrange
        var slots = new[] { new AvailableSlot(new TimeInterval(T(10), T(12))) }; // Slot 2 horas (10 as 12)
        
        var history = new ExecutionHistory(new[] 
        {
            new TimeInterval(T(10), T(11)) // O usuário já executou a primeira hora
        });

        var task = new TaskCandidate(Guid.NewGuid(), TaskPriority.High, TimeSpan.FromHours(2));

        // Act
        var result = _engine.GenerateRecommendations(new[] { task }, slots, history, _now, _minDuration);

        // Assert
        var best = Assert.Single(result);
        // O slot que sobrou foi 11h as 12h
        Assert.Equal(T(11), best.SuggestedInterval.Start);
        Assert.Equal(T(12), best.SuggestedInterval.End);
        Assert.True(best.Factors.IsChunked); // Sobrou 1 hora, tarefa era de 2h, então chunkou
    }

    [Fact]
    public void Engine_Should_Apply_Deterministic_TieBreakers()
    {
        // Arrange
        var slots = new[] { new AvailableSlot(new TimeInterval(T(10), T(11))) };
        
        // Empate Quádruplo Absoluto de Score!
        // Ambas High, Ambas Cabem perfeito no slot (Score = 100).
        var taskA = new TaskCandidate(Guid.Parse("11111111-1111-1111-1111-111111111111"), TaskPriority.High, TimeSpan.FromHours(1));
        var taskB = new TaskCandidate(Guid.Parse("00000000-0000-0000-0000-000000000000"), TaskPriority.High, TimeSpan.FromHours(1)); // GUID menor!

        // Act
        var result = _engine.GenerateRecommendations(new[] { taskA, taskB }, slots, _emptyHistory, _now, _minDuration);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(taskB.Id, result[0].TaskId); // TieBreaker Nível 4 (Alfanumérico do GUID) forçou a TaskB primeiro!
    }
}
