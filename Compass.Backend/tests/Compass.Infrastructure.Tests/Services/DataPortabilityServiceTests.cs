using Compass.Application.DTOs.Portability;
using Compass.Domain.Entities;
using Compass.Domain.Enums;
using Compass.Infrastructure.Persistence;
using Compass.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Compass.Infrastructure.Tests.Services;

public class DataPortabilityServiceTests
{
    private CompassDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<CompassDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            // Suprime o aviso de transação para permitir testes de métodos que usam BeginTransactionAsync
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new CompassDbContext(options);
    }

    [Fact]
    public async Task ExportUserBundleAsync_DeveGerarPacoteCompleto_QuandoUsuarioTiverDados()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var userId = Guid.NewGuid();

        var task = new TaskCommitment(userId, "Tarefa de Exportação", 45, 2);
        db.Commitments.Add(task);
        await db.SaveChangesAsync();

        var service = new DataPortabilityService(db, NullLogger<DataPortabilityService>.Instance);

        // Act
        var bundle = await service.ExportUserBundleAsync(userId);

        // Assert
        Assert.Equal(userId, bundle.UserId);
        Assert.Equal("4.0.0-tactical", bundle.SchemaVersion);
        Assert.Single(bundle.Commitments);
        Assert.Equal("Tarefa de Exportação", bundle.Commitments[0].Title);
    }

    [Fact]
    public async Task ImportUserBundleAsync_DeveRejeitarPacote_QuandoUserIdForVazio()
    {
        // Arrange
        using var db = GetInMemoryDbContext();
        var service = new DataPortabilityService(db, NullLogger<DataPortabilityService>.Instance);
        var invalidBundle = new PortabilityBundleDto(
            DateTime.UtcNow.ToString("O"), "4.0.0-tactical", Guid.Empty, 
            null, null, [], [], [], []
        );

        // Act
        var result = await service.ImportUserBundleAsync(invalidBundle);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("inválido", result.Message);
    }
}