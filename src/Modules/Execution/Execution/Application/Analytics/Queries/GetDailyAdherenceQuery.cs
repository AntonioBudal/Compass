using System;
using System.Collections.Generic;
using MediatR;

namespace Compass.Modules.Execution.Application.Analytics.Queries;

public record TaskAdherenceDto(
    Guid ReferenceId, 
    string Title, 
    double PlannedMinutes, 
    double ExecutedMinutes, 
    double IntersectedMinutes);

public record DailyAdherenceReportDto(
    Guid ProfileId, 
    DateOnly Date, 
    double TotalPlannedMinutes, 
    double TotalExecutedMinutes, 
    double GlobalConformityPercentage,
    IReadOnlyList<TaskAdherenceDto> Tasks);

public record GetDailyAdherenceQuery(Guid ProfileId, DateOnly Date) : IRequest<DailyAdherenceReportDto?>;