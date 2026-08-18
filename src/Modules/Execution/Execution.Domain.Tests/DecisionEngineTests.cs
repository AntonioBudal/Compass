using System;
using System.Linq;
using Compass.Modules.Execution.Domain.DecisionEngine;
using Xunit;

namespace Compass.Modules.Execution.Domain.Tests;

public class DecisionEngineTests
{
    private readonly DailyDecisionEngine _engine = new();
    private readonly DateOnly _date = new DateOnly(2026, 8, 17);
    
    // Auxiliar para gerar instantes nos testes
    private DateTimeOffset T(int hour) => new DateTimeOffset(2026, 8, 17, hour, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Build_Should_Allocate_Candidates_In_Absolute_Timeline()
    {
        var profileId = Guid.NewGuid();
        var candidates = new[] { new WorkCandidate(Guid.NewGuid(), "Task", "Task", 60, null, 1) };
        var windows = new[] { new AvailableWindow(T(8), T(10)) }; // 120m disponiveis
        
        // CORREÇÃO: Passando profileId como primeiro argumento.
        var plan = _engine.Build(profileId, _date, candidates, windows);

        Assert.Single(plan.Suggestions);
        var sug = plan.Suggestions[0];
        
        // Verifica a preservação exata da alocação de tempo sem offset local
        Assert.Equal(T(8), sug.Start);
        Assert.Equal(T(9), sug.End);
        Assert.Equal(TimeSpan.Zero, sug.Start.Offset);
    }
}