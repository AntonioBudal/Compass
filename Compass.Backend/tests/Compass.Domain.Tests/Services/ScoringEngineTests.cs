using Compass.Domain.Entities;
using Compass.Domain.Enums;
using Compass.Domain.Services;
using Xunit;

namespace Compass.Domain.Tests.Services;

public class ScoringEngineTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void CalculateTopActions_DeusUsarPerfilBasal_QuandoAmostrasForemMenoresQue10()
    {
        // Arrange: Perfil com apenas 5 amostras (não calibrado)
        var uncalibratedProfile = new UserScoringProfile(_userId);
        uncalibratedProfile.UpdateProfile(5, 0.20, 0.20, 1.0, 1.0, 1.8); // EAI agressivo de 1.8x

        var task = new TaskCommitment(_userId, "Tarefa de Teste", estimatedMinutes: 30, energyRequired: 2);
        
        // Act
        var results = ScoringEngine.CalculateTopActions(
            new[] { task },
            availableWindowMinutes: 45,
            userEnergyLevel: 2,
            nowUtc: DateTime.UtcNow,
            activeGoalProjectIds: new HashSet<Guid>(),
            blockedCommitmentIds: new HashSet<Guid>(),
            userProfile: uncalibratedProfile
        );

        // Assert: Como SampleCount < 10, o motor deve ignorar o EAI de 1.8x e usar o basal (30m nominal)
        Assert.Single(results);
        Assert.False(results[0].WasTimeAdjustedByEai);
        Assert.Equal(30, results[0].EffectiveDurationMinutes);
    }

    [Fact]
    public void CalculateTopActions_DeveAplicarEaiEAlteraDuracaoEfetiva_QuandoPerfilEstiverCalibrado()
    {
        // Arrange: Perfil calibrado com 15 amostras e EAI de 1.5x
        var calibratedProfile = new UserScoringProfile(_userId);
        calibratedProfile.UpdateProfile(15, 0.0, 0.0, 1.0, 1.0, 1.5);

        var task = new TaskCommitment(_userId, "Tarefa Complexa", estimatedMinutes: 40, energyRequired: 2);

        // Act
        var results = ScoringEngine.CalculateTopActions(
            new[] { task },
            availableWindowMinutes: 90,
            userEnergyLevel: 2,
            nowUtc: DateTime.UtcNow,
            activeGoalProjectIds: new HashSet<Guid>(),
            blockedCommitmentIds: new HashSet<Guid>(),
            userProfile: calibratedProfile
        );

        // Assert: 40m * 1.5 = 60m efetivos
        Assert.Single(results);
        Assert.True(results[0].WasTimeAdjustedByEai);
        Assert.Equal(60, results[0].EffectiveDurationMinutes);
        Assert.Contains("EAI calibrado", results[0].Reason);
    }

    [Fact]
    public void CalculateTopActions_DeveCortarTarefa_QuandoDuracaoEfetivaSuperarJanelaDisponivel()
    {
        // Arrange: Tarefa de 30m, mas com EAI de 2.0x (vira 60m). Janela livre do usuário é 45m.
        var calibratedProfile = new UserScoringProfile(_userId);
        calibratedProfile.UpdateProfile(20, 0.0, 0.0, 1.0, 1.0, 2.0);

        var task = new TaskCommitment(_userId, "Tarefa Subestimada", estimatedMinutes: 30, energyRequired: 2);

        // Act
        var results = ScoringEngine.CalculateTopActions(
            new[] { task },
            availableWindowMinutes: 45, // Janela menor que os 60m efetivos!
            userEnergyLevel: 2,
            nowUtc: DateTime.UtcNow,
            activeGoalProjectIds: new HashSet<Guid>(),
            blockedCommitmentIds: new HashSet<Guid>(),
            userProfile: calibratedProfile
        );

        // Assert: A tarefa deve ser filtrada pelo curto-circuito defensivo
        Assert.Empty(results);
    }
}