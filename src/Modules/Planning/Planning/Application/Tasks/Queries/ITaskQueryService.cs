using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Compass.Modules.Planning.Application.Tasks.Queries;

public interface ITaskQueryService
{
    Task<TaskDetailsDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskDetailsDto>> GetInboxAsync(
        CancellationToken cancellationToken = default);
}