using Compass.Application.DTOs.Projects;
using Compass.Domain.Entities;
using Compass.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Compass.Api.Controllers;

[ApiController]
[Route("api/v1/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(IProjectRepository projectRepository, ILogger<ProjectsController> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
    }

    [HttpGet("catalog")]
    [ProducesResponseType(typeof(IEnumerable<ProjectCatalogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCatalog(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        
        _logger.LogDebug("[ProjectsCatalog] Consultando catálogo LRU para usuário: {UserId}", userId);
        
        var projects = await _projectRepository.GetActiveCatalogAsync(userId, cancellationToken);

        // CORRIGIDO: Mapeamos a propriedade Title real para o DTO que a interface visual espera
        var catalogDtos = projects.Select(p => new ProjectCatalogDto(
            p.Id,
            p.Title, // Lê de Title e injeta em Name no JSON
            null,    // Como o Project atual não tem descrição, enviamos null de forma limpa
            p.LastUsedAt
        ));

        Response.Headers.CacheControl = "private, max-age=60";
        
        return Ok(catalogDtos);
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