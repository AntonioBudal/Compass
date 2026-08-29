using Compass.Modules.Planning.Application.Abstractions;
using Compass.Modules.Planning.Contracts.DTOs;
using Compass.Modules.Planning.Domain.Exceptions;
using Compass.Modules.Planning.Domain.Repositories;

namespace Compass.Modules.Planning.Application.Commands;

public sealed record CompleteTaskCommand(Guid Id) : ICommand<TaskDto>;

public class CompleteTaskCommandHandler : ICommandHandler<CompleteTaskCommand, TaskDto>
{
    private readonly ITaskRepository _repository;

    public CompleteTaskCommandHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async System.Threading.Tasks.Task<TaskDto> HandleAsync(CompleteTaskCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var task = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (task == null)
        {
            throw new PlanningDomainException($"Task with id '{command.Id}' was not found.");
        }

        task.Complete();

        await _repository.UpdateAsync(task, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return CreateTaskCommandHandler.MapToDto(task);
    }
}
