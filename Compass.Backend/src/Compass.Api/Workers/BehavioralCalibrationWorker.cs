using Compass.Application.Interfaces;
using Compass.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compass.Api.Workers;

public class BehavioralCalibrationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BehavioralCalibrationWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(4); // Calibração a cada 4 horas
    private readonly TimeSpan _initialDelay = TimeSpan.FromSeconds(15); // Atraso no boot para liberar o Kestrel

    public BehavioralCalibrationWorker(IServiceScopeFactory scopeFactory, ILogger<BehavioralCalibrationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[CalibrationWorker] Worker comportamental iniciado. Aguardando {Delay}s para primeiro ciclo...", _initialDelay.TotalSeconds);
        await Task.Delay(_initialDelay, stoppingToken);

        using var timer = new PeriodicTimer(_checkInterval);

        do
        {
            try
            {
                await RunCalibrationCycleAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "[CalibrationWorker] Erro inesperado durante o ciclo de calibração comportamental.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested);
    }

    private async Task RunCalibrationCycleAsync(CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("[CalibrationWorker] Iniciando ciclo de calibração comportamental em {Time} UTC", startTime);

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CompassDbContext>();
        var profiler = scope.ServiceProvider.GetRequiredService<IUserBehaviorProfilerService>();

        // 1. Identifica usuários ativos nos últimos 7 dias
        var activeCutoff = DateTime.UtcNow.AddDays(-7);
        var activeUserIds = await db.Commitments
            .AsNoTracking()
            .Where(c => c.CreatedAt >= activeCutoff || (c.CompletedAt != null && c.CompletedAt >= activeCutoff))
            .Select(c => c.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        _logger.LogInformation("[CalibrationWorker] {Count} usuários ativos detectados para recalibração.", activeUserIds.Count);

        int updatedCount = 0;
        foreach (var userId in activeUserIds)
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                // 2. Calcula o perfil em RAM sem reter locks no banco (< 30ms)
                var profile = await profiler.CalculateProfileAsync(userId, "America/Sao_Paulo", cancellationToken);

                // 3. UPSERT ATÔMICO NATIVO NO POSTGRESQL (Evita Deadlocks e Race Conditions)
                await db.Database.ExecuteSqlInterpolatedAsync($@"
                    INSERT INTO user_scoring_profiles (
                        id, user_id, sample_count, 
                        urgency_weight_adjust, strategy_weight_adjust, 
                        energy_alignment_weight, postponement_penalty_weight, eai_multiplier,
                        morning_energy_bias, afternoon_energy_bias, evening_energy_bias, night_energy_bias,
                        updated_at
                    ) VALUES (
                        {profile.Id}, {profile.UserId}, {profile.SampleCount},
                        {profile.UrgencyWeightAdjust}, {profile.StrategyWeightAdjust},
                        {profile.EnergyAlignmentWeight}, {profile.PostponementPenaltyWeight}, {profile.EaiMultiplier},
                        {profile.MorningEnergyBias}, {profile.AfternoonEnergyBias}, {profile.EveningEnergyBias}, {profile.NightEnergyBias},
                        {profile.UpdatedAt}
                    )
                    ON CONFLICT (user_id) DO UPDATE SET
                        sample_count = EXCLUDED.sample_count,
                        urgency_weight_adjust = EXCLUDED.urgency_weight_adjust,
                        strategy_weight_adjust = EXCLUDED.strategy_weight_adjust,
                        energy_alignment_weight = EXCLUDED.energy_alignment_weight,
                        postponement_penalty_weight = EXCLUDED.postponement_penalty_weight,
                        eai_multiplier = EXCLUDED.eai_multiplier,
                        morning_energy_bias = EXCLUDED.morning_energy_bias,
                        afternoon_energy_bias = EXCLUDED.afternoon_energy_bias,
                        evening_energy_bias = EXCLUDED.evening_energy_bias,
                        night_energy_bias = EXCLUDED.night_energy_bias,
                        updated_at = EXCLUDED.updated_at;", cancellationToken);

                updatedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[CalibrationWorker] Falha ao recalibrar usuário {UserId}. Pulando para o próximo.", userId);
            }
        }

        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
        _logger.LogInformation("[CalibrationWorker] Ciclo concluído em {Duration:N0}ms. {Updated}/{Total} perfis atualizados com sucesso.", duration, updatedCount, activeUserIds.Count);
    }
}