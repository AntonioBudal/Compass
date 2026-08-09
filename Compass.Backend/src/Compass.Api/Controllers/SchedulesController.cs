using Compass.Domain.Entities;
using Compass.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Compass.Api.Controllers;

[ApiController]
[Route("api/v1/schedules")]
public class SchedulesController : ControllerBase
{
    private readonly CompassDbContext _context;

    public SchedulesController(CompassDbContext context)
    {
        _context = context;
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetTodaySchedule(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        // C# DayOfWeek vai de 0 (Domingo) a 6 (Sábado)
        short today = (short)DateTime.Now.DayOfWeek; 

        var schedule = await _context.Schedules
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId && s.DayOfWeek == today && s.IsActive, cancellationToken);

        if (schedule == null)
        {
            // Fallback: Se não tem agenda no banco, assume o padrão corporativo para a UI não quebrar
            return Ok(new { workStart = "08:00", workEnd = "18:00", isActive = true });
        }

        return Ok(new 
        { 
            workStart = schedule.WorkStart.ToString("HH:mm"), 
            workEnd = schedule.WorkEnd.ToString("HH:mm"), 
            isActive = schedule.IsActive 
        });
    }

    private Guid GetUserId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var val) && Guid.TryParse(val, out var parsed))
            return parsed;
        return Guid.Parse("11111111-1111-1111-1111-111111111111");
    }
}