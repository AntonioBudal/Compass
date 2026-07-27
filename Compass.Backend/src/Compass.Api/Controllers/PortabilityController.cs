using Compass.Application.DTOs.Portability;
using Compass.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Compass.Api.Controllers;

[ApiController]
[Route("api/v1/portability")]
public class PortabilityController : ControllerBase
{
    private readonly IDataPortabilityService _portabilityService;

    public PortabilityController(IDataPortabilityService portabilityService)
    {
        _portabilityService = portabilityService;
    }

    /// <summary>
    /// Exporta o bundle completo com toda a telemetria, histórico de foco e configurações do operador.
    /// </summary>
    [HttpGet("export")]
    [ProducesResponseType(typeof(PortabilityBundleDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportData(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var bundle = await _portabilityService.ExportUserBundleAsync(userId, cancellationToken);

        var fileName = $"compass_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
        Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");

        return Ok(bundle);
    }

    private Guid GetUserId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var val) && Guid.TryParse(val, out var parsed))
            return parsed;
        return Guid.Parse("11111111-1111-1111-1111-111111111111");
    }

    /// <summary>
    /// Ingere um arquivo JSON autossuficiente (gerado pelo export) e restaura transacionalmente o banco de dados.
    /// </summary>
    [HttpPost("import")]
    [ProducesResponseType(typeof(PortabilityImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportData([FromBody] PortabilityBundleDto bundle, CancellationToken cancellationToken)
    {
        if (bundle == null) return BadRequest("O payload JSON de importação não pode estar vazio.");

        var result = await _portabilityService.ImportUserBundleAsync(bundle, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}