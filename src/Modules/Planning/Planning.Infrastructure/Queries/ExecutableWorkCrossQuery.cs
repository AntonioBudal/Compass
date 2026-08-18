using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Planning.Contracts.Queries;
using Compass.Modules.Planning.Domain.Tasks;
using Compass.Modules.Planning.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Compass.Modules.Planning.Infrastructure.Queries;

internal sealed class ExecutableWorkCrossQuery : IExecutableWorkQuery
{
    private readonly PlanningDbContext _dbContext;

    public ExecutableWorkCrossQuery(PlanningDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<WorkCandidate>> GetExecutableWorkAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        // Regra de Negócio: Tasks Executáveis são aquelas prontas para uso ou já em progresso
        var statuses = new[] { Compass.Modules.Planning.Domain.Tasks.TaskStatus.Ready, Compass.Modules.Planning.Domain.Tasks.TaskStatus.InProgress };

        var taskCandidates = await _dbContext.Tasks
            .Where(t => statuses.Contains(t.Status) && t.EstimatedDurationMinutes.HasValue)
            .AsNoTracking()
            .Select(t => new WorkCandidate(
                t.Id,
                t.Title,
                "Task",
                t.EstimatedDurationMinutes!.Value,
                t.HardDeadline,
                2 // Default Priority para Tasks até que o Domínio possua esse campo
            ))
            .ToListAsync(cancellationToken);

        // Os Habits não possuem Priority ou avaliação de Frequência ainda, então retornamos apenas Tasks prontas.
        return taskCandidates.AsReadOnly();
    }
}

