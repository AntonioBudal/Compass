using Compass.Application.DTOs.Analytics;
using Compass.Domain.Entities;
using Compass.Domain.Enums;
using Compass.Infrastructure.Persistence;
using Compass.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Compass.Application.Tests.Services;

public class ProgressServiceTests : IDisposable
{
    private readonly CompassDbContext _context;
    private readonly ProgressService _service;
    private readonly Guid _testUserId = Guid.NewGuid();

    public ProgressServiceTests()
    {
        var options = new DbContextOptionsBuilder<CompassDbContext>()
            .UseInMemoryDatabase(databaseName: $"Compass_Progress_Test_{Guid.NewGuid()}")
            .Options;

        _context = new CompassDbContext(options);
        _service = new ProgressService(_context);
    }

    [Fact]
    public async Task GetDailyTimeSeriesAsync_ShouldStrictlyExcludeToday_YesterdayCutoffRule()
    {
        // Arrange: 1 tarefa concluída ONTEM e 1 tarefa concluída HOJE
        var yesterdayTask = new TaskCommitment(_testUserId, "Tarefa de Ontem", 45, 3);
        yesterdayTask.Complete();
        // Forçando o carimbo para ontem em UTC
        typeof(Commitment).GetProperty("CompletedAt")!.SetValue(yesterdayTask, DateTime.UtcNow.AddDays(-1));

        var todayTask = new TaskCommitment(_testUserId, "Tarefa de Hoje", 30, 2);
        todayTask.Complete(); // CompletedAt assume DateTime.UtcNow (Hoje) por padrão

        _context.Commitments.AddRange(yesterdayTask, todayTask);
        await _context.SaveChangesAsync();

        // Act: Consulta a série temporal dos últimos 7 dias no fuso UTC
        var series = await _service.GetDailyTimeSeriesAsync(_testUserId, "7d", "UTC");
        var seriesList = series.ToList();

        // Assert: Apenas a tarefa de ONTEM pode retornar!
        Assert.Single(seriesList);
        Assert.Equal(yesterdayTask.CompletedAt!.Value.ToString("yyyy-MM-dd"), seriesList[0].DateIso);
        Assert.Equal(45, seriesList[0].EstimatedMinutes);
        
        // Garante que a tarefa de HOJE não vazou para o payload histórico do servidor
        Assert.DoesNotContain(seriesList, s => s.DateIso == DateTime.UtcNow.ToString("yyyy-MM-dd"));
    }

    [Fact]
    public async Task GetOverviewAsync_WhenNoFocusSessionsExist_ShouldImputeAccuracyAndSetFlagTrue()
    {
        // Arrange: 2 tarefas concluídas ontem no valor total de 60 minutos
        var task1 = new TaskCommitment(_testUserId, "Task 1", 30, 2);
        var task2 = new TaskCommitment(_testUserId, "Task 2", 30, 3);
        task1.Complete();
        task2.Complete();
        
        typeof(Commitment).GetProperty("CompletedAt")!.SetValue(task1, DateTime.UtcNow.AddDays(-1));
        typeof(Commitment).GetProperty("CompletedAt")!.SetValue(task2, DateTime.UtcNow.AddDays(-1));

        _context.Commitments.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        // Act
        var overview = await _service.GetOverviewAsync(_testUserId, "30d", "UTC");

        // Assert
        Assert.Equal(2, overview.TotalCompleted);
        Assert.Equal(60, overview.TotalUsefulMinutes);
        Assert.Equal(30, overview.TotalDeepWorkMinutes);
        Assert.True(overview.HasImputedAccuracyData); // Alerta para a UI
        Assert.Equal(1.0, overview.EstimationAccuracyIndex); // 100% neutro
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}