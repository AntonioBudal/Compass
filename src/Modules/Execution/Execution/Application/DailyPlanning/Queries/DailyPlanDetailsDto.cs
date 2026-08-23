using System;
using System.Collections.Generic;

namespace Compass.Modules.Execution.Application.DailyPlanning.Queries;

public sealed record DailyPlanItemDto(
    Guid Id,
    Guid ReferenceId,
    string Title,
    string Type, // "Task" ou "Habit"
    DateTimeOffset Start,
    DateTimeOffset End);

public sealed record DailyPlanDetailsDto(
    Guid Id,
    Guid ProfileId,
    DateOnly Date,
    IReadOnlyList<DailyPlanItemDto> Items);