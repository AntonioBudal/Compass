using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Compass.Modules.Execution.Application.DailyPlanning;

public record AcceptDailyPlanCommand(Guid ProfileId, DateOnly Date) : IRequest<Guid>;

public class AcceptDailyPlanCommandHandler : IRequestHandler<AcceptDailyPlanCommand, Guid>
{
    private readonly IDailyPlanRepository _repository;
    private readonly IMediator _mediator;

    public AcceptDailyPlanCommandHandler(IDailyPlanRepository repository, IMediator mediator)
    {
        _repository = repository;
        _mediator = mediator;
    }

    public async Task<Guid> Handle(AcceptDailyPlanCommand request, CancellationToken cancellationToken)
    {
        // Regra: Impede a re-aceitação acidental
        if (await _repository.ExistsAsync(request.ProfileId, request.Date, cancellationToken))
        {
            throw new Exception("A daily plan has already been accepted for this profile and date.");
        }

        // Regra TOCTOU: Em vez de confiar em um payload estático do Frontend, 
        // disparamos o motor novamente de forma limpa, garantindo a versão mais fresca
        var freshPlanResponse = await _mediator.Send(new BuildDailyPlanQuery(request.ProfileId, request.Date), cancellationToken);

        var domainPlan = new Compass.Modules.Execution.Domain.DecisionEngine.DailyPlan(request.ProfileId, request.Date);

        foreach (var sug in freshPlanResponse.Suggestions)
        {
            domainPlan.AddSuggestion(new Compass.Modules.Execution.Domain.DecisionEngine.SuggestedExecution(
                sug.ReferenceId,
                sug.Type,
                sug.Title ?? "Persisted Task", // Ajuste no DTO se Title for nulo
                sug.Start,
                sug.End,
                "Accepted by User"
            ));
        }

        await _repository.AddAsync(domainPlan, cancellationToken);

        return domainPlan.Id;
    }
}
