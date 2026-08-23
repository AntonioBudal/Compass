using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Execution.Application.DailyPlanning.Queries;
using Compass.Modules.Execution.Domain.DecisionEngine;
// Ajuste o namespace abaixo se o seu DbContext estiver em um local diferente
using Compass.Modules.Execution.Infrastructure.Database; 
using Microsoft.EntityFrameworkCore;

namespace Compass.Modules.Execution.Infrastructure.Queries;

internal sealed class DailyPlanQueryService : IDailyPlanQueryService
{
    private readonly ExecutionDbContext _dbContext;

    public DailyPlanQueryService(ExecutionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DailyPlanDetailsDto?> GetByDateAsync(Guid profileId, DateOnly date, CancellationToken cancellationToken = default)
    {
        // Utilizamos Set<DailyPlan>() para não depender da propriedade exposta publicamente no DbContext
        var plan = await _dbContext.Set<DailyPlan>()
            .AsNoTracking()
            .Where(p => p.ProfileId == profileId && p.Date == date)
            .Select(p => new DailyPlanDetailsDto(
                p.Id,
                p.ProfileId,
                p.Date,
                p.Suggestions.Select(s => new DailyPlanItemDto(
                    s.Id,
                    s.ReferenceId,
                    s.Title,
                    s.Type,
                    s.Start,
                    s.End)).ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return plan;
    }
}