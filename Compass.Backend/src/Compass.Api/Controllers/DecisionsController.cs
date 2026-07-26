using Compass.Application.DTOs.Decisions;
using Compass.Domain.Entities;
using Compass.Domain.Interfaces;
using Compass.Domain.Services;
using Compass.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Compass.Api.Controllers;

[ApiController]
[Route("api/v1/now")]
public class DecisionsController : ControllerBase
{
    private readonly CompassDbContext _context;
    private readonly ILogger<DecisionsController> _logger;

    public DecisionsController(CompassDbContext context, ILogger<DecisionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Retorna as 3 melhores ações táticas para o momento atual, ponderadas pelo perfil comportamental em RAM.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(DecisionResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopActions(
        [FromQuery] int windowMinutes = 60,
        [FromQuery] short energy = 2,
        [FromQuery] string timeZoneId = "America/Sao_Paulo",
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var nowUtc = DateTime.UtcNow;

        _logger.LogDebug("[NowEngine] Calculando recomendações adaptativas para usuário: {UserId}", userId);

        // 1. Busca os dados de entrada em paralelo limpo
        var candidates = await _context.Commitments
            .AsNoTracking()
            .Where(c => c.UserId == userId && c.Status == Domain.Enums.CommitmentStatus.Pending)
            .ToListAsync(cancellationToken);

        var userProfile = await _context.UserScoringProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken) 
            ?? UserScoringProfile.Default(userId);

        var projectNames = await _context.Projects
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.Status == Domain.Enums.CommitmentStatus.InProgress)
            .ToDictionaryAsync(p => p.Id, p => p.Title, cancellationToken);

        // 2. Executa a pontuação adaptativa no Domínio (Intacto)
        var scoredResults = ScoringEngine.CalculateTopActions(
            candidates: candidates,
            availableWindowMinutes: windowMinutes,
            userEnergyLevel: energy,
            nowUtc: nowUtc,
            activeGoalProjectIds: projectNames.Keys.ToHashSet(),
            blockedCommitmentIds: new HashSet<Guid>(),
            timeZoneId: timeZoneId,
            userProfile: userProfile
        );

        // 3. Projeção DTO Enriquecida
        var actionDtos = scoredResults.Select(s => new ScoredActionDto(
            CommitmentId: s.Commitment.Id,
            Title: s.Commitment.Title,
            Type: s.Commitment.Type.ToString().ToUpper(),
            NominalDurationMinutes: GetNominalDuration(s.Commitment),
            EffectiveDurationMinutes: s.EffectiveDurationMinutes,
            EnergyRequired: GetEnergy(s.Commitment),
            ScorePercentage: s.ScorePercentage,
            Reason: s.Reason,
            WasTimeAdjustedByEai: s.WasTimeAdjustedByEai,
            ProjectName: s.Commitment.ProjectId.HasValue && projectNames.TryGetValue(s.Commitment.ProjectId.Value, out var name) ? name : null
        )).ToList();

        var adaptiveDto = new AdaptiveProfileDto(
            IsCalibrated: userProfile.SampleCount >= 10,
            SampleCount: userProfile.SampleCount,
            EaiMultiplier: userProfile.EaiMultiplier,
            MorningEnergyBias: userProfile.MorningEnergyBias,
            AfternoonEnergyBias: userProfile.AfternoonEnergyBias,
            EveningEnergyBias: userProfile.EveningEnergyBias
        );

        var response = new DecisionResponseDto(
            GeneratedAtUtc: nowUtc,
            AvailableWindowMinutes: windowMinutes,
            OperatorEnergyLevel: energy,
            AdaptiveProfile: adaptiveDto,
            TopActions: actionDtos
        );

        return Ok(response);
    }

    private static int GetNominalDuration(Commitment c) => c switch
    {
        TaskCommitment t => t.EstimatedDurationMinutes,
        HabitCommitment h => h.EstimatedDurationMinutes,
        _ => 30
    };

    private static short GetEnergy(Commitment c) => c switch
    {
        TaskCommitment t => t.EnergyRequired,
        HabitCommitment h => h.EnergyRequired,
        _ => 2
    };

    private Guid GetUserId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var val) && Guid.TryParse(val, out var parsed))
            return parsed;
        return Guid.Parse("11111111-1111-1111-1111-111111111111");
    }
}