using Compass.Modules.Planning.Application.Abstractions;
using Compass.Modules.Planning.Application.Commands;
using Compass.Modules.Planning.Contracts.DTOs;
using Compass.Modules.Planning.Domain.Model;
using Compass.Modules.Planning.Domain.Repositories;
using TaskStatus = Compass.Modules.Planning.Domain.Model.TaskStatus;

namespace Compass.Modules.Planning.Application.Queries;

public sealed record GetTasksQuery(TaskStatus? Status = null) : IQuery<IReadOnlyList<TaskDto>>;

public class GetTasksQueryHandler : IQueryHandler<GetTasksQuery, IReadOnlyList<TaskDto>>
{
    private readonly ITaskRepository _repository;

    public GetTasksQueryHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async System.Threading.Tasks.Task<IReadOnlyList<TaskDto>> HandleAsync(GetTasksQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var tasks = await _repository.ListAsync(query.Status, cancellationToken);
        return tasks.Select(CreateTaskCommandHandler.MapToDto).ToList();
    }
}

public sealed record GetTaskByIdQuery(Guid Id) : IQuery<TaskDto?>;

public class GetTaskByIdQueryHandler : IQueryHandler<GetTaskByIdQuery, TaskDto?>
{
    private readonly ITaskRepository _repository;

    public GetTaskByIdQueryHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async System.Threading.Tasks.Task<TaskDto?> HandleAsync(GetTaskByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var task = await _repository.GetByIdAsync(query.Id, cancellationToken);
        return task == null ? null : CreateTaskCommandHandler.MapToDto(task);
    }
}
