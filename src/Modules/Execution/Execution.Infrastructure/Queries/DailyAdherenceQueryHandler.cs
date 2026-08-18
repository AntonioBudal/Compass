using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Execution.Application.Analytics.Queries;
using Compass.Modules.Execution.Domain.Time;
using Compass.Modules.Execution.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Compass.Modules.Execution.Infrastructure.Queries;

internal sealed class DailyAdherenceQueryHandler : IRequestHandler<GetDailyAdherenceQuery, DailyAdherenceReportDto?>
{
    private readonly ExecutionDbContext _dbContext;

    public DailyAdherenceQueryHandler(ExecutionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DailyAdherenceReportDto?> Handle(GetDailyAdherenceQuery request, CancellationToken cancellationToken)
    {
        // 1. Busca o Plano Diário (A Intenção)
        var plan = await _dbContext.DailyPlans
            .Include(p => p.Suggestions)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProfileId == request.ProfileId && p.Date == request.Date, cancellationToken);

        if (plan == null) return null;

        // 2. Busca o Ciclo Diário (A Realidade)
        // Nota: Assumindo que o ciclo diário compartilha a mesma semântica de data
        var cycle = await _dbContext.DailyCycles
            .Include(c => c.Logs)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Date == request.Date, cancellationToken);

        var taskAdherences = new List<TaskAdherenceDto>();
        double totalPlanned = 0;
        double totalExecuted = 0;
        double totalIntersected = 0;

        // 3. Cruzamento Analítico
        // Agrupamos o planejamento por Tarefa (podem haver múltiplas sessões sugeridas para a mesma tarefa)
        var plannedGroups = plan.Suggestions.GroupBy(s => s.ReferenceId);

        foreach (var group in plannedGroups)
        {
            var refId = group.Key;
            var title = group.First().Title;
            var plannedIntervals = group.Select(s => new TimeInterval(s.Start, s.End)).ToList();
            
            var plannedMins = plannedIntervals.Sum(i => (i.End - i.Start).TotalMinutes);
            totalPlanned += plannedMins;

            double executedMins = 0;
            double intersectedMins = 0;

            if (cycle != null)
            {
                // Filtra os logs que o usuário de fato registrou para esta tarefa hoje
                var executedIntervals = cycle.Logs
                    .Where(l => l.ReferenceId == refId)
                    .Select(l => l.Interval)
                    .ToList();

                executedMins = executedIntervals.Sum(i => (i.End - i.Start).TotalMinutes);
                totalExecuted += executedMins;

                // A mágica: A intersecção espacial no tempo. O usuário fez exatamente na hora que o motor mandou?
                // A mágica: A intersecção espacial no tempo. O usuário fez exatamente na hora que o motor mandou?
                foreach (var pInterval in plannedIntervals)
                {
                    foreach (var eInterval in executedIntervals)
                    {
                        var maxStart = pInterval.Start > eInterval.Start ? pInterval.Start : eInterval.Start;
                        var minEnd = pInterval.End < eInterval.End ? pInterval.End : eInterval.End;

                        if (maxStart < minEnd)
                        {
                            intersectedMins += (minEnd - maxStart).TotalMinutes;
                        }
                    }
                }
                
                totalIntersected += intersectedMins;
            }

            taskAdherences.Add(new TaskAdherenceDto(
                ReferenceId: refId,
                Title: title,
                PlannedMinutes: plannedMins,
                ExecutedMinutes: executedMins,
                IntersectedMinutes: intersectedMins
            ));
        }

        // Se planejou zero (impossível no nosso fluxo normal, mas seguro evitar divisão por zero)
        var conformity = totalPlanned > 0 ? (totalIntersected / totalPlanned) * 100 : 0;

        return new DailyAdherenceReportDto(
            request.ProfileId,
            request.Date,
            totalPlanned,
            totalExecuted,
            conformity,
            taskAdherences.AsReadOnly()
        );
    }
}