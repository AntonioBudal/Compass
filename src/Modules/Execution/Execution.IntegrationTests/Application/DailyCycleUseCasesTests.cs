using System;
using System.Linq;
using System.Threading.Tasks;
using Compass.Modules.Execution.Application.DailyCycles.CloseCycle;
using Compass.Modules.Execution.Application.DailyCycles.RecordExecution;
using Compass.Modules.Execution.Application.DailyCycles.StartCycle;
using Compass.Modules.Execution.Domain.DailyCycles;
using Compass.Modules.Execution.Infrastructure.Database;
using Compass.Modules.Execution.Infrastructure.Repositories;
using Compass.SharedKernel.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Compass.Modules.Execution.IntegrationTests.Application;

public class DailyCycleUseCasesTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("compass_execution_app_test")
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
    public async Task Deve_Executar_O_Ciclo_De_Vida_Completo_De_Um_Dia()
    {
        // 1. Iniciar o dia
        var startHandler = new StartDailyCycleCommandHandler(_repository);
        var date = new DateOnly(2026, 8, 18);
        var cycleId = await startHandler.HandleAsync(new StartDailyCycleCommand(date));

        Assert.NotEqual(Guid.Empty, cycleId);
        _dbContext.ChangeTracker.Clear();

        // 2. Registrar Trabalho (Pomodoro)
        var recordHandler = new RecordExecutionCommandHandler(_repository, new NoOpPublisher());
        var taskId = Guid.NewGuid();
        var start = new DateTimeOffset(2026, 8, 18, 14, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 8, 18, 14, 25, 0, TimeSpan.Zero);
        
        await recordHandler.HandleAsync(new RecordExecutionCommand(cycleId, taskId, start, end, ExecutionType.DeepWork));
        _dbContext.ChangeTracker.Clear();

        // Verificar se salvou
        var loaded = await _repository.GetByIdAsync(cycleId);
        Assert.Single(loaded!.Logs);
        Assert.Equal(ExecutionType.DeepWork, loaded.Logs.First().Type);
        
        // 3. Tentar registrar no mesmo tempo (Testando o vazamento da invariante)
        var exception = await Assert.ThrowsAsync<DomainException>(async () => 
            await recordHandler.HandleAsync(new RecordExecutionCommand(cycleId, Guid.NewGuid(), start, end, ExecutionType.Routine)));
        
        Assert.Contains("overlaps", exception.Message);

        // 4. Fechar o dia
        var closeHandler = new CloseDailyCycleCommandHandler(_repository);
        await closeHandler.HandleAsync(new CloseDailyCycleCommand(cycleId));
        _dbContext.ChangeTracker.Clear();

        var closedCycle = await _repository.GetByIdAsync(cycleId);
        Assert.Equal(CycleStatus.Closed, closedCycle!.Status);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _dbContainer.StopAsync();
    }

    private sealed class NoOpPublisher : MediatR.IPublisher
    {
        public System.Threading.Tasks.Task Publish(
            object notification,
            CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : MediatR.INotification
        {
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}

