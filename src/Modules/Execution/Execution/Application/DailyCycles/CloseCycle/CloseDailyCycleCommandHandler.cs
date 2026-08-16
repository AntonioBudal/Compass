using System;
using System.Threading;
using System.Threading.Tasks;

namespace Compass.Modules.Execution.Application.DailyCycles.CloseCycle;

public class CloseDailyCycleCommandHandler
{
    private readonly IDailyCycleRepository _repository;

    public CloseDailyCycleCommandHandler(IDailyCycleRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(CloseDailyCycleCommand command, CancellationToken cancellationToken = default)
    {
        var cycle = await _repository.GetByIdAsync(command.DailyCycleId, cancellationToken);
        
        if (cycle == null)
            throw new Exception($"DailyCycle with ID {command.DailyCycleId} not found.");

        cycle.Close();

        await _repository.UpdateAsync(cycle, cancellationToken);
    }
}
