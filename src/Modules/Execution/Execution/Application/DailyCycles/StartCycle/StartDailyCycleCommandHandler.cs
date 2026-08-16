using System;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Execution.Domain.DailyCycles;

namespace Compass.Modules.Execution.Application.DailyCycles.StartCycle;

public class StartDailyCycleCommandHandler
{
    private readonly IDailyCycleRepository _repository;

    public StartDailyCycleCommandHandler(IDailyCycleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> HandleAsync(StartDailyCycleCommand command, CancellationToken cancellationToken = default)
    {
        // Nota: Em um cenário multi-tenant ou para evitar duplicidade, 
        // faríamos uma checagem se já existe um ciclo para esta Date.
        var cycle = new DailyCycle(command.Date);
        cycle.Start();

        await _repository.AddAsync(cycle, cancellationToken);
        
        return cycle.Id;
    }
}
