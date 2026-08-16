using System;
using System.Linq;
using System.Threading.Tasks;
using Compass.Modules.Execution.Domain.DailyCycles;
using Compass.Modules.Execution.Domain.Time;
using Compass.Modules.Execution.Infrastructure.Database;
using Compass.Modules.Execution.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Compass.Modules.Execution.IntegrationTests.Persistence;

public class DailyCyclePersistenceTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("compass_execution_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private ExecutionDbContext _dbContext = null!;
    private EfDailyCycleRepository _repository = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        var options = new DbContextOptionsBuilder<ExecutionDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        _dbContext = new ExecutionDbContext(options);
        await _dbContext.Database.MigrateAsync();
        _repository = new EfDailyCycleRepository(_dbContext);
    }

    [Fact]
    public async Task Caso1_Deve_Salvar_E_Recuperar_Ciclo_Sem_Logs()
    {
        var cycle = new DailyCycle(new DateOnly(2026, 8, 18));
        await _repository.AddAsync(cycle);
        _dbContext.ChangeTracker.Clear();

        var loaded = await _repository.GetByIdAsync(cycle.Id);
        
        Assert.NotNull(loaded);
        Assert.Equal(cycle.Date, loaded.Date);
        Assert.Empty(loaded.Logs);
    }

    [Fact]
    public async Task Caso2_E_Caso3_E_Caso5_Deve_Salvar_Multiplos_Logs_Preservando_Precisao_Temporal_E_Imutabilidade()
    {
        var cycle = new DailyCycle(new DateOnly(2026, 8, 18));
        cycle.Start();

        var taskId1 = Guid.NewGuid();
        var taskId2 = Guid.NewGuid();

        // 8:00:00.123 até 8:25:00.000 (Precisão extrema)
        var t1Start = new DateTimeOffset(2026, 8, 18, 8, 0, 0, 123, TimeSpan.Zero);
        var t1End = new DateTimeOffset(2026, 8, 18, 8, 25, 0, TimeSpan.Zero);
        cycle.RecordExecution(taskId1, new TimeInterval(t1Start, t1End), ExecutionType.DeepWork);

        var t2Start = new DateTimeOffset(2026, 8, 18, 8, 30, 0, TimeSpan.Zero);
        var t2End = new DateTimeOffset(2026, 8, 18, 9, 0, 0, TimeSpan.Zero);
        cycle.RecordExecution(taskId2, new TimeInterval(t2Start, t2End), ExecutionType.Routine);

        await _repository.AddAsync(cycle);
        _dbContext.ChangeTracker.Clear();

        var loaded = await _repository.GetByIdAsync(cycle.Id);

        Assert.NotNull(loaded);
        Assert.Equal(2, loaded.Logs.Count);

        var log1 = loaded.Logs.First(l => l.ReferenceId == taskId1);
        Assert.Equal(ExecutionType.DeepWork, log1.Type);
        Assert.Equal(t1Start, log1.Interval.Start); // Precisão de milissegundo intacta
        Assert.Equal(t1End, log1.Interval.End);

        var log2 = loaded.Logs.First(l => l.ReferenceId == taskId2);
        Assert.Equal(ExecutionType.Routine, log2.Type);
    }

    [Fact]
    public async Task Caso4_Deve_Salvar_E_Recuperar_Transicoes_De_Estado()
    {
        var cycle = new DailyCycle(new DateOnly(2026, 8, 18));
        cycle.Start();
        await _repository.AddAsync(cycle);
        _dbContext.ChangeTracker.Clear();

        var activeLoaded = await _repository.GetByIdAsync(cycle.Id);
        Assert.Equal(CycleStatus.Active, activeLoaded!.Status);

        activeLoaded.Close();
        await _repository.UpdateAsync(activeLoaded);
        _dbContext.ChangeTracker.Clear();

        var closedLoaded = await _repository.GetByIdAsync(cycle.Id);
        Assert.Equal(CycleStatus.Closed, closedLoaded!.Status);
    }

    [Fact]
    public async Task Concorrencia_Deve_Lancar_Excecao_No_Update_Simultaneo()
    {
        var cycle = new DailyCycle(new DateOnly(2026, 8, 18));
        cycle.Start();
        await _repository.AddAsync(cycle);

        // Abre duas conexões diferentes na memória (simulando 2 requests)
        var options = new DbContextOptionsBuilder<ExecutionDbContext>().UseNpgsql(_dbContainer.GetConnectionString()).Options;
        using var ctx1 = new ExecutionDbContext(options);
        using var ctx2 = new ExecutionDbContext(options);

        var repo1 = new EfDailyCycleRepository(ctx1);
        var repo2 = new EfDailyCycleRepository(ctx2);

        var cycleRequestA = await repo1.GetByIdAsync(cycle.Id);
        var cycleRequestB = await repo2.GetByIdAsync(cycle.Id);

        // Request A modifica e salva
        cycleRequestA!.Close();
        await repo1.UpdateAsync(cycleRequestA);

        // Request B tenta modificar e salvar o estado antigo
        cycleRequestB!.RecordExecution(Guid.NewGuid(), new TimeInterval(
            new DateTimeOffset(2026, 8, 18, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 8, 18, 11, 0, 0, TimeSpan.Zero)
        ), ExecutionType.Routine);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await repo2.UpdateAsync(cycleRequestB));
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _dbContainer.StopAsync();
    }
}
