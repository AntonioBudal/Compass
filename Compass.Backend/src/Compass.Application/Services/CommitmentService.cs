using Compass.Application.DTOs;
using Compass.Application.Interfaces;
using Compass.Domain.Entities;
using Compass.Domain.Enums;
using Compass.Domain.Exceptions;
using Compass.Domain.Interfaces;

namespace Compass.Application.Services;

public class CommitmentService : ICommitmentService
{
    private readonly ICommitmentRepository _commitmentRepo;
    private readonly IProjectRepository _projectRepo;

    public CommitmentService(ICommitmentRepository commitmentRepo, IProjectRepository projectRepo)
    {
        _commitmentRepo = commitmentRepo;
        _projectRepo = projectRepo;
    }

    public async Task<CommitmentDto?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var commitment = await _commitmentRepo.GetByIdAsync(id, cancellationToken);
        if (commitment == null || commitment.UserId != userId) return null;

        string? projectName = null;
        if (commitment.ProjectId.HasValue)
        {
            var project = await _projectRepo.GetByIdAsync(commitment.ProjectId.Value, cancellationToken);
            projectName = project?.Title;
        }

        return MapToDto(commitment, projectName);
    }

    public async Task<IEnumerable<CommitmentDto>> GetAllActiveAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var commitments = await _commitmentRepo.GetActiveCandidatesAsync(userId, cancellationToken);
        var projects = await _projectRepo.GetByUserIdAsync(userId, cancellationToken);
        var projectMap = projects.ToDictionary(p => p.Id, p => p.Title);

        return commitments.Select(c =>
        {
            projectMap.TryGetValue(c.ProjectId ?? Guid.Empty, out var projName);
            return MapToDto(c, projName);
        });
    }

    public async Task<CommitmentDto> CreateAsync(Guid userId, CreateCommitmentDto dto, CancellationToken cancellationToken = default)
    {
        // CORRIGIDO: Ordem dos parâmetros alinhada com as entidades da Etapa 01
        Commitment commitment = dto.Type.ToUpperInvariant() switch
        {
            "TASK" => new TaskCommitment(userId, dto.Title, dto.EstimatedDurationMinutes ?? 30, dto.EnergyRequired ?? 2, dto.ProjectId, dto.Deadline),
            "HABIT" => new HabitCommitment(userId, dto.Title, dto.CronExpression ?? "0 0 * * *", dto.EstimatedDurationMinutes ?? 30, dto.EnergyRequired ?? 2, dto.ProjectId),
            "EVENT" => new EventCommitment(userId, dto.Title, dto.StartTime ?? DateTime.UtcNow, dto.EndTime ?? DateTime.UtcNow.AddHours(1), dto.LocationOrLink, dto.ProjectId),
            "NOTE" => new NoteCommitment(userId, dto.Title, dto.Content, dto.ProjectId),
            _ => throw new DomainException("Tipo de compromisso desconhecido.")
        };

        await _commitmentRepo.AddAsync(commitment, cancellationToken);
        await _commitmentRepo.SaveChangesAsync(cancellationToken);

        string? projectName = null;
        if (commitment.ProjectId.HasValue)
        {
            var project = await _projectRepo.GetByIdAsync(commitment.ProjectId.Value, cancellationToken);
            projectName = project?.Title;
        }

        return MapToDto(commitment, projectName);
    }

    public async Task<StatusTransitionResponseDto> UpdateStatusAsync(
        Guid userId, 
        Guid commitmentId, 
        UpdateStatusDto dto, 
        CancellationToken cancellationToken = default)
    {
        var commitment = await _commitmentRepo.GetByIdAsync(commitmentId, cancellationToken)
            ?? throw new DomainException("Compromisso não encontrado.");

        if (commitment.UserId != userId)
            throw new DomainException("Acesso negado a este compromisso.");

        string previousStatus = commitment.Status.ToString().ToUpperInvariant();
        
        if (!Enum.TryParse<CommitmentStatus>(dto.NewStatus, true, out var targetStatus))
            throw new DomainException($"O status '{dto.NewStatus}' é inválido.");

        // Transição de estado de acordo com as regras de negócio
        if (targetStatus == CommitmentStatus.Completed && commitment.Status != CommitmentStatus.Completed)
        {
            commitment.Complete();
        }
        else if (targetStatus == CommitmentStatus.InProgress && commitment.Status != CommitmentStatus.InProgress)
        {
            commitment.Start();
        }
        else
        {
            commitment.UpdateStatus(targetStatus);
        }

        _commitmentRepo.Update(commitment);
        await _commitmentRepo.SaveChangesAsync(cancellationToken);

        string currentStatus = commitment.Status.ToString().ToUpperInvariant();

        // Gerar eventos em cascata para feedback visual imediato no frontend
        var cascadedEvents = new List<CascadedDomainEventDto>();
        if (targetStatus == CommitmentStatus.Completed && commitment is HabitCommitment habit)
        {
            cascadedEvents.Add(new CascadedDomainEventDto(
                "HabitStreakIncremented",
                habit.Id,
                $"🔥 Hábito concluído! Seu streak atual subiu para {habit.CurrentStreak} dias contínuos."
            ));
        }

        return new StatusTransitionResponseDto(
            commitment.Id,
            previousStatus,
            currentStatus,
            DateTime.UtcNow,
            cascadedEvents.AsReadOnly()
        );
    }

    private static CommitmentDto MapToDto(Commitment c, string? projectName = null)
    {
        int duration = 0; short energy = 0; DateTime? deadline = null; DateTime? start = null; DateTime? end = null;
        string? location = null; string? cron = null; int currentStreak = 0; int bestStreak = 0; int postponed = 0; string? content = null;

        if (c is TaskCommitment t) { duration = t.EstimatedDurationMinutes; energy = t.EnergyRequired; deadline = t.Deadline; postponed = t.PostponedCount; }
        else if (c is HabitCommitment h) { duration = h.EstimatedDurationMinutes; energy = h.EnergyRequired; cron = h.CronExpression; currentStreak = h.CurrentStreak; bestStreak = h.BestStreak; }
        else if (c is EventCommitment e) { start = e.StartTime; end = e.EndTime; location = e.LocationOrLink; }
        else if (c is NoteCommitment n) { content = n.Content; }

        return new CommitmentDto(
            c.Id, c.Title, c.Type.ToString().ToUpperInvariant(), c.Status.ToString().ToUpperInvariant(),
            duration, energy, deadline, start, end, location, cron, currentStreak, bestStreak, postponed, content,
            c.ProjectId, projectName
        );
    }
}