using Compass.Application.DTOs.DailyCycle;
using Compass.Domain.Entities;
using Compass.Infrastructure.Persistence;
using Compass.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Compass.Infrastructure.Tests.Services;

public class DailyCycleServiceTests
{
    private CompassDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<CompassDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new CompassDbContext(options);
    }

    [Fact]
    public async Task GetMorningBriefingAsync_DeveCalcularPendenciasETempoTotal_ComExatidao()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var userId = Guid.NewGuid();

        db.Commitments.Add(new TaskCommitment(userId, "Tarefa 1", 30, 2));
        db.Commitments.Add(new TaskCommitment(userId, "Tarefa 2", 60, 3));
        await db.SaveChangesAsync();

        var service = new DailyCycleService(db, NullLogger<DailyCycleService>.Instance);

        // Act
        var briefing = await service.GetMorningBriefingAsync(userId);

        // Assert
        Assert.Equal(2, briefing.PendingTasksCount);
        Assert.Equal(90, briefing.TotalEstimatedFocusMinutes);
        Assert.Equal("Tarefa 1", briefing.TopFocusTitle);
    }

    [Fact]
    public async Task ExecuteShutdownAsync_DeveSubstituirRevisaoAnterior_QuandoExecutadoDuasVezesNoMesmoDia()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var userId = Guid.NewGuid();
        var service = new DailyCycleService(db, NullLogger<DailyCycleService>.Instance);

        var request1 = new DailyShutdownRequestDto(5, 1, 180, "Primeiro encerramento", ["#underestimated"]);
        var request2 = new DailyShutdownRequestDto(8, 0, 240, "Encerramento corrigido", ["#flow"]);

        // Act - Executa duas vezes consecutivas para o mesmo usuário e data
        await service.ExecuteShutdownAsync(userId, request1);
        var res2 = await service.ExecuteShutdownAsync(userId, request2);

        // Assert: Não deve lançar exceção e deve haver apenas 1 registro no banco
        var reviews = await db.DailyReviews.Where(r => r.UserId == userId).ToListAsync();
        Assert.Single(reviews);
        Assert.Equal(8, reviews[0].CompletedCount);
        Assert.Contains("[TAGS: #flow]", reviews[0].Notes);
        Assert.True(res2.AnalyticalLogUpdated);
    }
}