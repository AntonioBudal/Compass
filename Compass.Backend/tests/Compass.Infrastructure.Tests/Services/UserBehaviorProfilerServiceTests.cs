using Compass.Domain.Entities;
using Compass.Domain.Enums;
using Compass.Infrastructure.Persistence;
using Compass.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Compass.Infrastructure.Tests.Services;

public class UserBehaviorProfilerServiceTests
{
    private CompassDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<CompassDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new CompassDbContext(options);
    }

    [Fact]
    public async Task CalculateProfileAsync_DeveWinsorizarOutliersEm3x_QuandoSessaoDeFocoForAberrante()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var userId = Guid.NewGuid();

        // Tarefa estimada em 30m
        var task = new TaskCommitment(userId, "Tarefa Esquecida", estimatedMinutes: 30, energyRequired: 2);
        task.Complete(); // Concluída hoje
        db.Commitments.Add(task);

        // Simula que o usuário esqueceu o cronômetro rodando por 5 horas (300m = 10x a estimativa)
        var focusSession = new FocusSession(userId, task.Id, startTimeUtc: DateTime.UtcNow.AddHours(-5));
        focusSession.EndSession(actualDurationMinutes: 300);
        db.FocusSessions.Add(focusSession);

        await db.SaveChangesAsync();

        var service = new UserBehaviorProfilerService(db, NullLogger<UserBehaviorProfilerService>.Instance);

        // Act
        var profile = await service.CalculateProfileAsync(userId);

        // Assert: A Winsorização de 3x deve truncar os 300m para 90m (30m * 3).
        // Logo, EAI bruto seria 90 / 30 = 3.0. Como o Clamping da entidade limita o EAI em 2.0x, o valor final deve ser 2.0.
        Assert.Equal(1, profile.SampleCount);
        Assert.Equal(2.0, profile.EaiMultiplier);
    }

    [Fact]
    public async Task CalculateProfileAsync_DeveRetornarObjetoNulo_QuandoNaoHouverAmostras()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var service = new UserBehaviorProfilerService(db, NullLogger<UserBehaviorProfilerService>.Instance);

        // Act
        var profile = await service.CalculateProfileAsync(Guid.NewGuid());

        // Assert
        Assert.Equal(0, profile.SampleCount);
        Assert.Equal(1.0, profile.EaiMultiplier);
        Assert.Equal(0.0, profile.UrgencyWeightAdjust);
    }
}