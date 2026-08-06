using Compass.Domain.Entities;
using Compass.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Compass.Api.Controllers;

[ApiController]
[Route("api/v1/goals")]
public class GoalsController : ControllerBase
{
    private readonly IGoalRepository _goalRepository;

    public GoalsController(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var goals = await _goalRepository.GetActiveGoalsAsync(userId, cancellationToken);
        
        // Mapeamento limpo para o DTO do Frontend
        var dtos = goals.Select(g => new {
            id = g.Id,
            title = g.Title,
            why = g.WhyDescription, // Frontend mapeia para 'why'
            targetDate = g.TargetDate,
            status = g.Status.ToString().ToUpperInvariant()
        });

        return Ok(dtos);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGoalDto dto, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var goal = new Goal(userId, dto.Title, dto.WhyDescription, dto.TargetDate);
        
        await _goalRepository.AddAsync(goal, cancellationToken);
        await _goalRepository.SaveChangesAsync(cancellationToken);

        return Ok(new { id = goal.Id, title = goal.Title, why = goal.WhyDescription, targetDate = goal.TargetDate, status = goal.Status.ToString().ToUpperInvariant() });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateGoalDto dto, CancellationToken cancellationToken = default)
    {
        var goal = await _goalRepository.GetByIdAsync(id, cancellationToken);
        if (goal == null || goal.UserId != GetUserId()) return NotFound();

        // O método UpdateGoalDetails já existe no seu Goal.cs
        goal.UpdateGoalDetails(dto.Title, dto.WhyDescription, dto.TargetDate);
        
        _goalRepository.Update(goal);
        await _goalRepository.SaveChangesAsync(cancellationToken);

        return Ok(new { id = goal.Id, title = goal.Title, why = goal.WhyDescription, targetDate = goal.TargetDate, status = goal.Status.ToString().ToUpperInvariant() });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var goal = await _goalRepository.GetByIdAsync(id, cancellationToken);
        if (goal == null || goal.UserId != GetUserId()) return NoContent();

        _goalRepository.Remove(goal);
        await _goalRepository.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private Guid GetUserId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var val) && Guid.TryParse(val, out var parsed))
            return parsed;
        return Guid.Parse("11111111-1111-1111-1111-111111111111");
    }
}

// DTO simplificado (Coloque em Compass.Application/DTOs/Goals/ se preferir)
public record CreateGoalDto(string Title, string? WhyDescription, DateTime? TargetDate);