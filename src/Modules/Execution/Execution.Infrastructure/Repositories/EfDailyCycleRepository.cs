using System;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Execution.Application.DailyCycles;
using Compass.Modules.Execution.Domain.DailyCycles;
using Compass.Modules.Execution.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Compass.Modules.Execution.Infrastructure.Repositories;

internal class EfDailyCycleRepository : IDailyCycleRepository
{
    private readonly ExecutionDbContext _dbContext;

    public EfDailyCycleRepository(ExecutionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        DailyCycle cycle,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.DailyCycles.AddAsync(
            cycle,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<DailyCycle?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DailyCycles
            .Include(c => c.Logs)
            .FirstOrDefaultAsync(
                c => c.Id == id,
                cancellationToken);
    }

    public async Task UpdateAsync(
        DailyCycle cycle,
        CancellationToken cancellationToken = default)
    {
        /*
         * DailyCycle é a fronteira de concorrência do aggregate.
         *
         * Quando apenas um ExecutionLog é adicionado, a raiz pode
         * continuar Unchanged no ChangeTracker.
         *
         * Como o xmin pertence à linha daily_cycles, precisamos
         * provocar deliberadamente um UPDATE da raiz para:
         *
         * 1. validar o xmin carregado originalmente;
         * 2. incrementar o xmin;
         * 3. detectar duas mutações concorrentes no mesmo aggregate.
         *
         * Marcamos somente Status, evitando colocar toda a entidade
         * em EntityState.Modified.
         */
        _dbContext.Entry(cycle)
            .Property(c => c.Status)
            .IsModified = true;

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
