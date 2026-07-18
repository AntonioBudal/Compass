using Compass.Application.DTOs;
using Compass.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Compass.Api.Controllers;

[ApiController]
[Route("api/v1/decisions")]
public class DecisionsController : ControllerBase
{
    private readonly IDecisionService _decisionService;

    public DecisionsController(IDecisionService decisionService)
    {
        _decisionService = decisionService;
    }

    /// <summary>
    /// Consulta o Motor de Decisão para obter as Top 3 Ações Viáveis para o momento atual.
    /// </summary>
    [HttpGet("now")]
    [ProducesResponseType(typeof(DecisionResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNow([FromQuery] string timeZoneId = "America/Sao_Paulo", CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _decisionService.GetNowDecisionAsync(userId, timeZoneId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Registra a escolha do usuário para telemetria e calibração do algoritmo.
    /// </summary>
    [HttpPost("now/choice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterChoice(
        [FromQuery] Guid snapshotId, 
        [FromQuery] Guid chosenCommitmentId, 
        CancellationToken cancellationToken = default)
    {
        await _decisionService.RegisterChoiceAsync(snapshotId, chosenCommitmentId, cancellationToken);
        return NoContent();
    }

    // Auxiliar Local-First para MVP (Lê do Header X-User-Id ou usa UUID fixo de desenvolvimento)
    private Guid GetUserId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var val) && Guid.TryParse(val, out var parsed))
        {
            return parsed;
        }
        return Guid.Parse("11111111-1111-1111-1111-111111111111"); // Usuário Padrão Local-First
    }
}