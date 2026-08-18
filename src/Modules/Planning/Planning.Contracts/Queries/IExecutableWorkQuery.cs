using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Compass.Modules.Planning.Contracts.Queries;

public interface IExecutableWorkQuery
{
    Task<IReadOnlyList<WorkCandidate>> GetExecutableWorkAsync(DateOnly date, CancellationToken cancellationToken = default);
}

