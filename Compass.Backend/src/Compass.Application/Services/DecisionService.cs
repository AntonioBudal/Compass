using Compass.Application.DTOs;
using Compass.Application.Interfaces;
using Compass.Domain.Entities;
using Compass.Domain.Enums;
using Compass.Domain.Exceptions;
using Compass.Domain.Interfaces;
using Compass.Domain.Services;

namespace Compass.Application.Services;

public class DecisionService : IDecisionService
{
    private readonly ICommitmentRepository _commitmentRepo;
    private readonly IProjectRepository _projectRepo;
    private readonly IDecisionSnapshotRepository _snapshotRepo;

    public DecisionService(
        ICommitmentRepository commitmentRepo,
        IProjectRepository projectRepo,
        IDecisionSnapshotRepository snapshotRepo)
    {
        _commitmentRepo = commitmentRepo;
        _projectRepo = projectRepo;
        _snapshotRepo = snapshotRepo;
    }

    public async Task<DecisionResponseDto> GetNowDecisionAsync(
        Guid userId, 
        string timeZoneId, 
        CancellationToken cancellationToken = default)
    {
        var nowUtc = DateTime.UtcNow;

        // 1. Consultar eventos do dia para encontrar Hard Blockers e calcular a janela
        var startOfDayUtc = nowUtc.Date;
        var endOfDayUtc = startOfDayUtc.AddDays(1).AddTicks(-1);

        var todayEvents = await _commitmentRepo.GetEventsByRangeAsync(userId, startOfDayUtc, endOfDayUtc, cancellationToken);

        // 2. Calcular janela de tempo disponível em minutos líquidos (subtraindo reuniões)
        int availableMinutes = TimeWindowCalculator.CalculateAvailableMinutes(
            nowUtc, 
            timeZoneId, 
            todaySchedule: null, // Fallback para fim do dia caso não haja turno configurado
            todayEvents);

        // 3. Identificar próximo Hard Blocker ativo para exibir no contexto
        var nextBlocker = todayEvents
            .Where(e => e.StartTime > nowUtc && e.Status != CommitmentStatus.Archived)
            .OrderBy(e => e.StartTime)
            .FirstOrDefault();

        ActiveHardBlockerDto? blockerDto = nextBlocker != null
            ? new ActiveHardBlockerDto(nextBlocker.Title, nextBlocker.StartTime)
            : null;

        short defaultEnergy = 2; // Nível 2 (Média) como padrão de energia no MVP

        // 4. Se a janela livre for 0 (usuário em reunião), retorna resposta vazia e segura
        if (availableMinutes <= 0)
        {
            var emptyContext = new DecisionContextDto(defaultEnergy, 0, blockerDto);
            return new DecisionResponseDto(nowUtc, emptyContext, null, new List<ScoredActionDto>().AsReadOnly());
        }

        // 5. Buscar candidatas ativas no banco de dados (acessa o índice parcial do PostgreSQL)
        var candidates = await _commitmentRepo.GetActiveCandidatesAsync(userId, cancellationToken);
        var projects = await _projectRepo.GetByUserIdAsync(userId, cancellationToken);

        // Projetos vinculados a metas ativas ganham bônus de pontuação
        var activeGoalProjectIds = projects
            .Where(p => p.GoalId.HasValue && p.Status != CommitmentStatus.Archived)
            .Select(p => p.Id)
            .ToHashSet();

        var projectNamesMap = projects.ToDictionary(p => p.Id, p => p.Title);

        // 6. Executar o algoritmo matemático do ScoringEngine em memória RAM
        var scoredList = ScoringEngine.CalculateTopActions(
            candidates,
            availableMinutes,
            defaultEnergy,
            nowUtc,
            activeGoalProjectIds,
            new HashSet<Guid>()); // Zero dependências bloqueadas no fluxo basal

        // 7. Mapear para DTOs visualmente ricos para o Vue.js 3
        var scoredDtos = scoredList.Select(s =>
        {
            int duration = 30;
            short energy = 2;
            if (s.Commitment is TaskCommitment t) { duration = t.EstimatedDurationMinutes; energy = t.EnergyRequired; }
            else if (s.Commitment is HabitCommitment h) { duration = h.EstimatedDurationMinutes; energy = h.EnergyRequired; }

            string? projName = s.Commitment.ProjectId.HasValue && projectNamesMap.TryGetValue(s.Commitment.ProjectId.Value, out var name)
                ? name : null;

            return new ScoredActionDto(
                s.Commitment.Id,
                s.Commitment.Title,
                s.Commitment.Type.ToString().ToUpperInvariant(),
                duration,
                energy,
                projName,
                s.ScorePercentage,
                s.Reason
            );
        }).ToList();

        var topFocus = scoredDtos.FirstOrDefault();
        var alternatives = scoredDtos.Skip(1).ToList().AsReadOnly();

        // 8. Gravar o Snapshot de Auditoria no PostgreSQL (< 15ms)
        var snapshot = new DecisionSnapshot(
            userId,
            availableMinutes,
            defaultEnergy,
            top1Id: topFocus?.Id,
            top2Id: alternatives.ElementAtOrDefault(0)?.Id,
            top3Id: alternatives.ElementAtOrDefault(1)?.Id
        );

        await _snapshotRepo.AddAsync(snapshot, cancellationToken);
        await _snapshotRepo.SaveChangesAsync(cancellationToken);

        var contextDto = new DecisionContextDto(defaultEnergy, availableMinutes, blockerDto);
        return new DecisionResponseDto(nowUtc, contextDto, topFocus, alternatives);
    }

    public async Task RegisterChoiceAsync(Guid snapshotId, Guid chosenCommitmentId, CancellationToken cancellationToken = default)
    {
        var snapshot = await _snapshotRepo.GetByIdAsync(snapshotId, cancellationToken)
            ?? throw new DomainException("Snapshot de decisão não encontrado.");

        snapshot.RegisterChoice(chosenCommitmentId);
        _snapshotRepo.Update(snapshot);
        await _snapshotRepo.SaveChangesAsync(cancellationToken);
    }
}