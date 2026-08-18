using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Calendar.Contracts.Queries;
using Compass.Modules.Planning.Contracts.Queries;
using Compass.Modules.Execution.Domain.DecisionEngine;
using MediatR;

namespace Compass.Modules.Execution.Application.DailyPlanning;

// DTO Exato da Resposta
public record DailyPlanSuggestionDto(Guid ReferenceId, string Type, string Title, DateTimeOffset Start, DateTimeOffset End);
public record DailyPlanResponseDto(string Date, IReadOnlyList<DailyPlanSuggestionDto> Suggestions);

public record BuildDailyPlanQuery(Guid ProfileId, DateOnly Date) : IRequest<DailyPlanResponseDto>;

public class BuildDailyPlanQueryHandler : IRequestHandler<BuildDailyPlanQuery, DailyPlanResponseDto>
{
    private readonly IExecutableWorkQuery _executableWorkQuery;
    private readonly IAvailabilityQuery _availabilityQuery;

    public BuildDailyPlanQueryHandler(
        IExecutableWorkQuery executableWorkQuery,
        IAvailabilityQuery availabilityQuery)
    {
        _executableWorkQuery = executableWorkQuery;
        _availabilityQuery = availabilityQuery;
    }

    public async Task<DailyPlanResponseDto> Handle(BuildDailyPlanQuery request, CancellationToken cancellationToken)
    {
        // Orquestração Pura (Cross-Module Calls)
        var workCandidates = await _executableWorkQuery.GetExecutableWorkAsync(request.Date, cancellationToken);
        var availability = await _availabilityQuery.GetAvailabilityAsync(request.ProfileId, request.Date, cancellationToken);

        var engineCandidates = workCandidates.Select(c => new Compass.Modules.Execution.Domain.DecisionEngine.WorkCandidate(
            c.ReferenceId, c.Title, c.Type, c.EstimatedMinutes, c.Deadline, c.Priority)).ToList();

        var engineWindows = availability.Select(a => new Compass.Modules.Execution.Domain.DecisionEngine.AvailableWindow(
            a.Start, a.End)).ToList();

        // Toma a Decisão
        var engine = new DailyDecisionEngine();
        var plan = engine.Build(request.ProfileId, request.Date, engineCandidates, engineWindows);

        // Mapeia para o Contrato HTTP
        var dtos = plan.Suggestions.Select(s => new DailyPlanSuggestionDto(
            s.ReferenceId,
            s.Type, s.Title,
            s.Start,
            s.End
        )).ToList();

        return new DailyPlanResponseDto(request.Date.ToString("yyyy-MM-dd"), dtos);
    }
}




