using Compass.Modules.Planning.Application.Abstractions;
using Compass.Modules.Planning.Contracts.DTOs;
using Compass.Modules.Planning.Domain.Repositories;
using TaskModel = Compass.Modules.Planning.Domain.Model.Task;

namespace Compass.Modules.Planning.Application.Commands;

public sealed record CreateTaskCommand(
    string Title,
    string? Description = null,
    int? DurationMinutes = null,
    DateTimeOffset? Deadline = null
) : ICommand<TaskDto>;

public class CreateTaskCommandHandler : ICommandHandler<CreateTaskCommand, TaskDto>
{
    private readonly ITaskRepository _repository;

    public CreateTaskCommandHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async System.Threading.Tasks.Task<TaskDto> HandleAsync(CreateTaskCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var task = TaskModel.Create(
            command.Title,
            command.Description,
            command.DurationMinutes,
            command.Deadline
        );

        await _repository.AddAsync(task, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return MapToDto(task);
    }

    public static TaskDto MapToDto(TaskModel task)
    {
        return new TaskDto(
            task.Id,
            task.Title,
            task.Description,
            task.DurationMinutes,
            task.Deadline,
            task.Status.ToString(),
            task.CreatedAt,
            task.UpdatedAt,
            task.CompletedAt
        );
    }
}
