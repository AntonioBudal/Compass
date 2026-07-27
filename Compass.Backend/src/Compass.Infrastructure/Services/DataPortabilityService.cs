using Compass.Application.DTOs.Portability;
using Compass.Domain.Entities;
using Compass.Domain.Enums;
using Compass.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Compass.Infrastructure.Services;

public interface IDataPortabilityService
{
    Task<PortabilityBundleDto> ExportUserBundleAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PortabilityImportResultDto> ImportUserBundleAsync(PortabilityBundleDto bundle, CancellationToken cancellationToken = default);
}

public class DataPortabilityService : IDataPortabilityService
{
    private readonly CompassDbContext _context;
    private readonly ILogger<DataPortabilityService> _logger;

    public DataPortabilityService(CompassDbContext context, ILogger<DataPortabilityService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PortabilityBundleDto> ExportUserBundleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var start = DateTime.UtcNow;
        _logger.LogInformation("[Portability] Iniciando exportação de dados para o usuário {UserId}", userId);

        // Execução sequencial limpa sem tracking para evitar contenção de DbContext e zerar overhead de memória
        var setting = await _context.Settings
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .Select(s => new SettingExportDto(s.DefaultEnergyLevel, s.Theme, s.AutoPostponeEnabled, s.DailyReviewTime.ToString("HH:mm"), s.PreferencesJson))
            .FirstOrDefaultAsync(cancellationToken);

        var profile = await _context.UserScoringProfiles
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => new ScoringProfileExportDto(p.SampleCount, p.EaiMultiplier, p.MorningEnergyBias, p.AfternoonEnergyBias, p.EveningEnergyBias, p.NightEnergyBias))
            .FirstOrDefaultAsync(cancellationToken);

        var projects = await _context.Projects
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => new ProjectExportDto(p.Id, p.Title, p.Status.ToString(), p.TotalEstimatedDurationMinutes, p.LastUsedAt))
            .ToListAsync(cancellationToken);

        var commitments = await _context.Commitments
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Select(c => new CommitmentExportDto(
                c.Id, c.Title, c.Type.ToString(), c.Status.ToString(), 
                EF.Property<int?>(c, "EstimatedDurationMinutes") ?? 30,
                EF.Property<short?>(c, "EnergyRequired") ?? (short)2,
                EF.Property<int?>(c, "PostponedCount") ?? 0,
                EF.Property<DateTime?>(c, "Deadline"),
                c.CreatedAt, c.CompletedAt, c.ProjectId))
            .ToListAsync(cancellationToken);

        var sessions = await _context.FocusSessions
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .Select(f => new FocusSessionExportDto(f.Id, f.CommitmentId, f.StartTime, f.EndTime, f.ActualDurationMinutes))
            .ToListAsync(cancellationToken);

        var reviews = await _context.DailyReviews
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.ReviewDate)
            .Select(d => new DailyReviewExportDto(d.Id, d.ReviewDate.ToString("yyyy-MM-dd"), d.CompletedCount, d.PostponedCount, d.TotalFocusMinutes, d.Notes))
            .ToListAsync(cancellationToken);

        var duration = (DateTime.UtcNow - start).TotalMilliseconds;
        _logger.LogInformation("[Portability] Exportação concluída em {Duration:N0}ms. ({Count} compromissos, {Sessions} sessões)", duration, commitments.Count, sessions.Count);

        return new PortabilityBundleDto(
            ExportedAtUtc: DateTime.UtcNow.ToString("O"),
            SchemaVersion: "4.0.0-tactical",
            UserId: userId,
            Settings: setting,
            AdaptiveProfile: profile,
            Projects: projects,
            Commitments: commitments,
            FocusSessions: sessions,
            DailyReviews: reviews
        );
    }


    public async Task<PortabilityImportResultDto> ImportUserBundleAsync(
        PortabilityBundleDto bundle, 
        CancellationToken cancellationToken = default)
    {
        if (bundle == null || bundle.UserId == Guid.Empty)
            return new PortabilityImportResultDto(false, 0, 0, 0, 0, false, false, "Pacote de importação inválido ou sem identificação de usuário.");

        var userId = bundle.UserId;
        _logger.LogInformation("[Portability] Iniciando importação transacional para o usuário {UserId}. Versão do Schema: {Version}", userId, bundle.SchemaVersion);

        // CONTROLE TRANSACIONAL ATÔMICO: Qualquer erro executa Rollback imediato no PostgreSQL
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        int projCount = 0, commCount = 0, sessCount = 0, revCount = 0;
        bool settingsUpdated = false, profileUpdated = false;

        try
        {
            // 1. UPSERT DE PROJETOS (Last-Write-Wins via Raw SQL atômico para respeitar encapsulamento DDD)
            if (bundle.Projects != null && bundle.Projects.Any())
            {
                foreach (var p in bundle.Projects)
                {
                    var statusEnum = Enum.TryParse<CommitmentStatus>(p.Status, true, out var st) ? st : CommitmentStatus.InProgress;
                    var lastUsed = p.LastUsedAt ?? DateTime.UtcNow;

                    await _context.Database.ExecuteSqlInterpolatedAsync($@"
                        INSERT INTO projects (id, user_id, title, status, total_estimated_duration_minutes, created_at, updated_at, last_used_at)
                        VALUES ({p.Id}, {userId}, {p.Title}, {statusEnum}, {p.TotalEstimatedMinutes}, {DateTime.UtcNow}, {DateTime.UtcNow}, {lastUsed})
                        ON CONFLICT (id) DO UPDATE SET
                            title = EXCLUDED.title,
                            status = EXCLUDED.status,
                            total_estimated_duration_minutes = EXCLUDED.total_estimated_duration_minutes,
                            updated_at = EXCLUDED.updated_at,
                            last_used_at = EXCLUDED.last_used_at;", cancellationToken);
                    projCount++;
                }
            }

            // 2. UPSERT DE COMPROMISSOS (Tarefas, Hábitos, Eventos)
            if (bundle.Commitments != null && bundle.Commitments.Any())
            {
                foreach (var c in bundle.Commitments)
                {
                    var typeEnum = Enum.TryParse<CommitmentType>(c.Type, true, out var t) ? t : CommitmentType.Task;
                    var statusEnum = Enum.TryParse<CommitmentStatus>(c.Status, true, out var st) ? st : CommitmentStatus.Pending;

                    await _context.Database.ExecuteSqlInterpolatedAsync($@"
                        INSERT INTO commitments (id, user_id, title, type, status, estimated_duration_minutes, energy_required, postponed_count, deadline, created_at, completed_at, project_id, updated_at)
                        VALUES ({c.Id}, {userId}, {c.Title}, {typeEnum}, {statusEnum}, {c.EstimatedMinutes}, {c.EnergyRequired}, {c.PostponedCount}, {c.Deadline}, {c.CreatedAt}, {c.CompletedAt}, {c.ProjectId}, {DateTime.UtcNow})
                        ON CONFLICT (id) DO UPDATE SET
                            title = EXCLUDED.title,
                            status = EXCLUDED.status,
                            estimated_duration_minutes = EXCLUDED.estimated_duration_minutes,
                            energy_required = EXCLUDED.energy_required,
                            postponed_count = EXCLUDED.postponed_count,
                            deadline = EXCLUDED.deadline,
                            completed_at = EXCLUDED.completed_at,
                            project_id = EXCLUDED.project_id,
                            updated_at = EXCLUDED.updated_at;", cancellationToken);
                    commCount++;
                }
            }

            // 3. UPSERT DE SESSÕES DE FOCO
            if (bundle.FocusSessions != null && bundle.FocusSessions.Any())
            {
                foreach (var f in bundle.FocusSessions)
                {
                    await _context.Database.ExecuteSqlInterpolatedAsync($@"
                        INSERT INTO focus_sessions (id, user_id, commitment_id, start_time_utc, end_time_utc, actual_duration_minutes, created_at)
                        VALUES ({f.Id}, {userId}, {f.CommitmentId}, {f.StartTimeUtc}, {f.EndTimeUtc}, {f.ActualMinutes}, {DateTime.UtcNow})
                        ON CONFLICT (id) DO UPDATE SET
                            end_time_utc = EXCLUDED.end_time_utc,
                            actual_duration_minutes = EXCLUDED.actual_duration_minutes;", cancellationToken);
                    sessCount++;
                }
            }

            // 4. UPSERT DE REVISÕES DIÁRIAS (Evita conflito no índice único user_date)
            if (bundle.DailyReviews != null && bundle.DailyReviews.Any())
            {
                foreach (var r in bundle.DailyReviews)
                {
                    if (DateOnly.TryParse(r.ReviewDate, out var revDate))
                    {
                        await _context.Database.ExecuteSqlInterpolatedAsync($@"
                            INSERT INTO daily_reviews (id, user_id, review_date, completed_count, postponed_count, total_focus_minutes, notes, created_at)
                            VALUES ({r.Id}, {userId}, {revDate}, {r.CompletedCount}, {r.PostponedCount}, {r.TotalFocusMinutes}, {r.Notes}, {DateTime.UtcNow})
                            ON CONFLICT (user_id, review_date) DO UPDATE SET
                                completed_count = EXCLUDED.completed_count,
                                postponed_count = EXCLUDED.postponed_count,
                                total_focus_minutes = EXCLUDED.total_focus_minutes,
                                notes = EXCLUDED.notes;", cancellationToken);
                        revCount++;
                    }
                }
            }

            // 5. RESTAURAÇÃO DE CONFIGURAÇÕES
            if (bundle.Settings != null)
            {
                var s = bundle.Settings;
                var reviewTime = TimeOnly.TryParse(s.DailyReviewTime, out var rt) ? rt : new TimeOnly(20, 0);
                
                var existingSetting = await _context.Settings.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
                if (existingSetting == null)
                {
                    existingSetting = new Setting(userId, s.DefaultEnergyLevel, s.Theme);
                    _context.Settings.Add(existingSetting);
                }
                existingSetting.UpdatePreferences(s.DefaultEnergyLevel, s.Theme, s.AutoPostponeEnabled, reviewTime, s.PreferencesJson);
                settingsUpdated = true;
            }

            // 6. RESTAURAÇÃO DE PERFIL ADAPTATIVO
            if (bundle.AdaptiveProfile != null)
            {
                var p = bundle.AdaptiveProfile;
                var existingProfile = await _context.UserScoringProfiles.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
                if (existingProfile == null)
                {
                    existingProfile = new UserScoringProfile(userId);
                    _context.UserScoringProfiles.Add(existingProfile);
                }
                existingProfile.UpdateProfile(p.SampleCount, 0.0, 0.0, 1.0, 1.0, p.EaiMultiplier);
                existingProfile.UpdateChronologyBiases(p.MorningBias, p.AfternoonBias, p.EveningBias, p.NightBias);
                profileUpdated = true;
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("[Portability] Importação concluída com sucesso! {Proj} projetos, {Comm} compromissos restaurados.", projCount, commCount);

            return new PortabilityImportResultDto(
                Success: true,
                ProjectsImported: projCount,
                CommitmentsImported: commCount,
                FocusSessionsImported: sessCount,
                DailyReviewsImported: revCount,
                ProfileUpdated: profileUpdated,
                SettingsUpdated: settingsUpdated,
                Message: "Dados restaurados e reconciliados com sucesso!"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Portability] Erro crítico durante importação. Executando Rollback.");
            await transaction.RollbackAsync(cancellationToken);
            return new PortabilityImportResultDto(false, 0, 0, 0, 0, false, false, $"Falha na importação: {ex.Message}");
        }
    }
}