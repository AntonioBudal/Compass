using Compass.Modules.Planning.Application.Abstractions;
using Compass.Modules.Planning.Contracts.DTOs;
using Compass.Modules.Planning.Domain.Exceptions;
using Compass.Modules.Planning.Domain.Repositories;

namespace Compass.Modules.Planning.Application.Commands;

public sealed record SetTaskEstimateCommand(
    Guid Id,
    int? DurationMinutes
) : ICommand<TaskDto>;

public class SetTaskEstimateCommandHandler : ICommandHandler<SetTaskEstimateCommand, TaskDto>
{
    private readonly ITaskRepository _repository;

    public SetTaskEstimateCommandHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async System.Threading.Tasks.Task<TaskDto> HandleAsync(SetTaskEstimateCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var task = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (task == null)
        {
            throw new PlanningDomainException($"Task with id '{command.Id}' was not found.");
        }

        task.SetEstimate(command.DurationMinutes);

        await _repository.UpdateAsync(task, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return CreateTaskCommandHandler.MapToDto(task);
    }
}
