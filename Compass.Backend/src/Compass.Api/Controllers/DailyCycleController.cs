using Compass.Application.DTOs.DailyCycle;
using Compass.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Compass.Api.Controllers;

[ApiController]
[Route("api/v1/daily-cycle")]
public class DailyCycleController : ControllerBase
{
    private readonly IDailyCycleService _dailyCycleService;

    public DailyCycleController(IDailyCycleService dailyCycleService)
    {
        _dailyCycleService = dailyCycleService;
    }

    /// <summary>
    /// Retorna o briefing matinal com estatísticas táticas para o início da jornada.
    /// </summary>
    [HttpGet("morning-briefing")]
    [ProducesResponseType(typeof(MorningBriefingDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMorningBriefing([FromQuery] string timeZoneId = "America/Sao_Paulo", CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var briefing = await _dailyCycleService.GetMorningBriefingAsync(userId, timeZoneId, cancellationToken);
        return Ok(briefing);
    }

    /// <summary>
    /// Executa o encerramento do dia, salvando métricas de foco e tags de divergência algorítmica.
    /// </summary>
    [HttpPost("shutdown")]
    [ProducesResponseType(typeof(DailyShutdownResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExecuteShutdown([FromBody] DailyShutdownRequestDto request, [FromQuery] string timeZoneId = "America/Sao_Paulo", CancellationToken cancellationToken = default)
    {
        if (request == null) return BadRequest("Os dados da revisão diária não podem ser nulos.");

        var userId = GetUserId();
        var response = await _dailyCycleService.ExecuteShutdownAsync(userId, request, timeZoneId, cancellationToken);
        return Ok(response);
    }

    private Guid GetUserId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var val) && Guid.TryParse(val, out var parsed))
            return parsed;
        return Guid.Parse("11111111-1111-1111-1111-111111111111");
    }
}