using System;
using System.Linq;
using System.Threading.Tasks;
using Compass.Modules.Execution.Application.DailyCycles.Queries;
using Compass.Modules.Execution.Domain.DailyCycles;
using Compass.Modules.Execution.Domain.Time;
using Compass.Modules.Execution.Infrastructure.Database;
using Compass.Modules.Execution.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Compass.Modules.Execution.IntegrationTests.Application.Queries;

public class DailyCycleQueriesTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("compass_execution_query_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private ExecutionDbContext _dbContext = null!;
    private DailyCycleQueryService _queryService = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        var options = new DbContextOptionsBuilder<ExecutionDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        _dbContext = new ExecutionDbContext(options);
        await _dbContext.Database.MigrateAsync();
        
        _queryService = new DailyCycleQueryService(_dbContext);
    }

    [Fact]
    public async Task Deve_Retornar_O_Ciclo_Diario_Com_Logs_Por_Id()
    {
        // Arrange - Popular o banco com dados reais de teste
        var date = new DateOnly(2026, 8, 16);
        var cycle = new DailyCycle(date);
        cycle.Start();

        var logRef = Guid.NewGuid();
        var start = new DateTimeOffset(2026, 8, 16, 10, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 8, 16, 11, 0, 0, TimeSpan.Zero);
        
        cycle.RecordExecution(logRef, new TimeInterval(start, end), ExecutionType.DeepWork);

        await _dbContext.DailyCycles.AddAsync(cycle);
        await _dbContext.SaveChangesAsync();

        // Act - Executar a consulta
        var dto = await _queryService.GetByIdAsync(cycle.Id);

        // Assert - Validar o DTO retornado
        Assert.NotNull(dto);
        Assert.Equal(cycle.Id, dto.Id);
        Assert.Equal(date, dto.Date);
        Assert.Equal("Active", dto.Status);
        
        Assert.Single(dto.Logs);
        var logDto = dto.Logs.First();
        Assert.Equal(logRef, logDto.ReferenceId);
        Assert.Equal("DeepWork", logDto.Type);
        Assert.Equal(start, logDto.Start);
        Assert.Equal(end, logDto.End);
    }

    [Fact]
    public async Task Deve_Retornar_O_Ciclo_Diario_Por_Data()
    {
        // Arrange
        var date = new DateOnly(2026, 8, 17);
        var cycle = new DailyCycle(date);
        
        await _dbContext.DailyCycles.AddAsync(cycle);
        await _dbContext.SaveChangesAsync();

        // Act
        var dto = await _queryService.GetByDateAsync(date);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(cycle.Id, dto.Id);
        Assert.Equal(date, dto.Date);
        Assert.Empty(dto.Logs);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _dbContainer.StopAsync();
    }
}
