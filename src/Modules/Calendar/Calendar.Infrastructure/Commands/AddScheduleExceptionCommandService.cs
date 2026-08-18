using System;
using System.Threading;
using System.Threading.Tasks;
using Compass.Modules.Calendar.Application.Profiles.Commands;
using Compass.Modules.Calendar.Domain.ScheduleExceptions;
using Compass.Modules.Calendar.Infrastructure.Database;
using Compass.Modules.Calendar.Infrastructure.Database.Models;

namespace Compass.Modules.Calendar.Infrastructure.Commands;

internal sealed class AddScheduleExceptionCommandService : IAddScheduleExceptionCommandService
{
    private readonly CalendarDbContext _dbContext;

    public AddScheduleExceptionCommandService(CalendarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddExceptionAsync(Guid profileId, CreateExceptionRequest request, CancellationToken cancellationToken = default)
    {
        var start = TimeSpan.Parse(request.StartTime);
        var end = TimeSpan.Parse(request.EndTime);
        
        // CORREÇÃO: Passando o Guid como primeiro parâmetro.
        var id = Guid.NewGuid();
        var domainException = new ScheduleException(id, request.Date, start, end, request.Reason);

        var data = new ScheduleExceptionData
        {
            Id = id,
            ScheduleProfileId = profileId,
            Date = domainException.Date,
            StartTime = domainException.StartTime,
            EndTime = domainException.EndTime,
            Reason = domainException.Reason
        };

        _dbContext.ScheduleExceptions.Add(data);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
