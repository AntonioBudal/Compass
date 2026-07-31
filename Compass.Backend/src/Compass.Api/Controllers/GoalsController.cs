using Compass.Application.DTOs;
using Compass.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Compass.Api.Controllers;

[ApiController]
[Route("api/v1/goals")]
public class GoalsController : ControllerBase
{
    private readonly IGoalService _goalService;
    private readonly IValidator<UpdateGoalDto> _updateValidator;

    public GoalsController(IGoalService goalService, IValidator<UpdateGoalDto> updateValidator)
    {
        _goalService = goalService;
        _updateValidator = updateValidator;
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        Guid id, 
        [FromBody] UpdateGoalDto dto, 
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var userId = GetUserId();
        var updated = await _goalService.UpdateAsync(userId, id, dto, cancellationToken);
        return Ok(updated);
    }

    private Guid GetUserId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var val) && Guid.TryParse(val, out var parsed))
        {
            return parsed;
        }
        return Guid.Parse("11111111-1111-1111-1111-111111111111");
    }
}