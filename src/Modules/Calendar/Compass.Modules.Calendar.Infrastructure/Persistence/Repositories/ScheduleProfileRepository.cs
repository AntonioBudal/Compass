using Compass.Modules.Calendar.Domain.Model;
using Compass.Modules.Calendar.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Compass.Modules.Calendar.Infrastructure.Persistence.Repositories;

public class ScheduleProfileRepository : IScheduleProfileRepository
{
    private readonly CalendarDbContext _context;

    public ScheduleProfileRepository(CalendarDbContext context)
    {
        _context = context;
    }

    public async Task<ScheduleProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<ScheduleProfile>()
            .Include(p => p.WeeklyAvailability)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(ScheduleProfile profile, CancellationToken cancellationToken = default)
    {
        await _context.Set<ScheduleProfile>().AddAsync(profile, cancellationToken);
    }

    public Task UpdateAsync(ScheduleProfile profile, CancellationToken cancellationToken = default)
    {
        _context.Set<ScheduleProfile>().Update(profile);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
