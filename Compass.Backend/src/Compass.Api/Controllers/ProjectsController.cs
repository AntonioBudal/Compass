using Compass.Application.DTOs;
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
            p.Title, 
            null,       // Description 
            p.GoalId,   //  Agora sim, o Guid? está no lugar certo!
            p.LastUsedAt
        ));

        Response.Headers.CacheControl = "private, max-age=60";
        
        return Ok(catalogDtos);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        
        // Entidade limpa e direta
        var project = new Project(userId, dto.Name, dto.GoalId);
        
        await _projectRepository.AddAsync(project, cancellationToken);
        // Lembre-se de garantir que o ProjectRepository tenha o método SaveChangesAsync()
        await _projectRepository.SaveChangesAsync(cancellationToken); 

        return Ok(new ProjectCatalogDto(project.Id, project.Title, null, project.GoalId, project.LastUsedAt));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateProjectDto dto, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        if (project == null || project.UserId != GetUserId()) return NotFound();

        // Usa o método que acabamos de criar na entidade
        project.UpdateDetails(dto.Name, dto.GoalId);
        
        _projectRepository.Update(project);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        return Ok(new ProjectCatalogDto(project.Id, project.Title, null, project.GoalId, project.LastUsedAt));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        if (project == null || project.UserId != GetUserId()) return NoContent();

        _projectRepository.Remove(project);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        return NoContent();
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