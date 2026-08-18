using System;
using System.Threading;
using System.Threading.Tasks;

namespace Compass.Modules.Calendar.Application.Profiles.Commands;

public record CreateExceptionRequest(DateOnly Date, string StartTime, string EndTime, string Reason);

public interface IAddScheduleExceptionCommandService
{
    Task AddExceptionAsync(Guid profileId, CreateExceptionRequest request, CancellationToken cancellationToken = default);
}
