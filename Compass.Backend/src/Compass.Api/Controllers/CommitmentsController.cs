using Compass.Application.DTOs;
using Compass.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Compass.Api.Controllers;

[ApiController]
[Route("api/v1/commitments")]
public class CommitmentsController : ControllerBase
{
    private readonly ICommitmentService _commitmentService;
    private readonly IValidator<CreateCommitmentDto> _createValidator;
    private readonly IValidator<UpdateStatusDto> _statusValidator;

    public CommitmentsController(
        ICommitmentService commitmentService,
        IValidator<CreateCommitmentDto> createValidator,
        IValidator<UpdateStatusDto> statusValidator)
    {
        _commitmentService = commitmentService;
        _createValidator = createValidator;
        _statusValidator = statusValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CommitmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActive(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _commitmentService.GetAllActiveAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CommitmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _commitmentService.GetByIdAsync(userId, id, cancellationToken);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(CommitmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCommitmentDto dto, CancellationToken cancellationToken = default)
    {
        // Validação explícita com FluentValidation
        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var userId = GetUserId();
        var created = await _commitmentService.CreateAsync(userId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(StatusTransitionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(
        Guid id, 
        [FromBody] UpdateStatusDto dto, 
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _statusValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var userId = GetUserId();
        var result = await _commitmentService.UpdateStatusAsync(userId, id, dto, cancellationToken);
        return Ok(result);
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