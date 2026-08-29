using Compass.Modules.Calendar.Application.Abstractions;
using Compass.Modules.Calendar.Contracts.DTOs;

namespace Compass.Modules.Calendar.Application.Queries;

public sealed record GetScheduleProfileByIdQuery(Guid Id) : IQuery<ScheduleProfileDto?>;
