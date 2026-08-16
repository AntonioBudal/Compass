using System;
using Compass.Modules.Execution.Domain.Time;

namespace Compass.Modules.Execution.Domain.DecisionEngine;

public record DecisionFactors(
    bool IsPerfectFit,
    bool IsChunked,
    decimal PriorityScore,
    decimal WindowFitScore
);

public record Recommendation(
    Guid TaskId,
    TimeInterval SuggestedInterval,
    decimal Score,
    DecisionFactors Factors
);
