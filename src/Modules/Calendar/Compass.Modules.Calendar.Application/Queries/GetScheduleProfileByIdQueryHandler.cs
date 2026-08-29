using Compass.Modules.Calendar.Application.Abstractions;
using Compass.Modules.Calendar.Application.Commands;
using Compass.Modules.Calendar.Contracts.DTOs;
using Compass.Modules.Calendar.Domain.Repositories;

namespace Compass.Modules.Calendar.Application.Queries;

public class GetScheduleProfileByIdQueryHandler : IQueryHandler<GetScheduleProfileByIdQuery, ScheduleProfileDto?>
{
    private readonly IScheduleProfileRepository _repository;

    public GetScheduleProfileByIdQueryHandler(IScheduleProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<ScheduleProfileDto?> HandleAsync(GetScheduleProfileByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var profile = await _repository.GetByIdAsync(query.Id, cancellationToken);
        if (profile == null)
        {
            return null;
        }

        return CreateScheduleProfileCommandHandler.MapToDto(profile);
    }
}
