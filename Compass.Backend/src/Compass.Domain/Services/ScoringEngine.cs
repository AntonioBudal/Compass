using Compass.Domain.Entities;
using Compass.Domain.Enums;

namespace Compass.Domain.Services;

public class ScoredCommitment
{
    public Commitment Commitment { get; }
    public double ScorePercentage { get; }
    public string Reason { get; }
    public bool WasTimeAdjustedByEai { get; }
    public int EffectiveDurationMinutes { get; }

    public ScoredCommitment(
        Commitment commitment, 
        double scorePercentage, 
        string reason, 
        bool wasTimeAdjustedByEai, 
        int effectiveDurationMinutes)
    {
        Commitment = commitment;
        ScorePercentage = scorePercentage;
        Reason = reason;
        WasTimeAdjustedByEai = wasTimeAdjustedByEai;
        EffectiveDurationMinutes = effectiveDurationMinutes;
    }
}

public static class ScoringEngine
{
    // Pesos basais da Fórmula de Pontuação
    private const double BaseWeightUrgency = 0.35;
    private const double BaseWeightTime = 0.25;
    private const double BaseWeightEnergy = 0.20;
    private const double BaseWeightStrategy = 0.20;
    private const double PenaltyPerPostpone = 0.05;

    public static IReadOnlyList<ScoredCommitment> CalculateTopActions(
        IEnumerable<Commitment> candidates,
        int availableWindowMinutes,
        short userEnergyLevel,
        DateTime nowUtc,
        HashSet<Guid> activeGoalProjectIds,
        HashSet<Guid> blockedCommitmentIds,
        string timeZoneId = "America/Sao_Paulo",
        UserScoringProfile? userProfile = null)
    {
        // Fallback defensivo: se o perfil for nulo ou não estiver calibrado (< 10 amostras),
        // o sistema adota automaticamente o Objeto Nulo neutro (Modo Basal intacto)
        var profile = (userProfile != null && userProfile.SampleCount >= 10) 
            ? userProfile 
            : UserScoringProfile.Default(Guid.Empty);

        var scoredList = new List<ScoredCommitment>();

        // 1. Determinar o Bloco Horário Biológico do Operador via Fuso Local
        DateTime localNow = ConvertToLocalTimeSafe(nowUtc, timeZoneId);
        double timeOfDayEnergyBias = localNow.Hour switch
        {
            >= 6 and < 12 => profile.MorningEnergyBias,
            >= 12 and < 18 => profile.AfternoonEnergyBias,
            _ => profile.EveningEnergyBias
        };

        // 2. Calibrar Pesos Dinâmicos com Proteção de Limite (Clamping)
        double effectiveWeightUrgency = Math.Clamp(BaseWeightUrgency + profile.UrgencyWeightAdjust, 0.10, 0.60);
        double effectiveWeightStrategy = Math.Clamp(BaseWeightStrategy + profile.StrategyWeightAdjust, 0.10, 0.50);
        double effectiveWeightEnergy = BaseWeightEnergy;
        double effectiveWeightTime = BaseWeightTime;

        foreach (var candidate in candidates)
        {
            if (blockedCommitmentIds.Contains(candidate.Id) || candidate.Status == CommitmentStatus.Blocked)
                continue;

            int nominalMinutes = 30;
            short energyRequired = 2;
            DateTime? deadline = null;
            int postponedCount = 0;
            bool isHabit = false;

            if (candidate is TaskCommitment task)
            {
                nominalMinutes = task.EstimatedDurationMinutes;
                energyRequired = task.EnergyRequired;
                deadline = task.Deadline;
                postponedCount = task.PostponedCount;
            }
            else if (candidate is HabitCommitment habit)
            {
                nominalMinutes = habit.EstimatedDurationMinutes;
                energyRequired = habit.EnergyRequired;
                isHabit = true;
            }
            else
            {
                continue;
            }

            // 3. Aplicação do EAI (Estimation Accuracy Index)
            // Se o histórico mostra subestimativa (ex: 1.5x), a duração efetiva aumenta
            int effectiveDurationMinutes = (int)Math.Round(nominalMinutes * profile.EaiMultiplier);
            if (effectiveDurationMinutes < 5) effectiveDurationMinutes = 5;

            bool wasTimeAdjusted = Math.Abs(profile.EaiMultiplier - 1.0) > 0.10 && profile.SampleCount >= 10;

            // Curto-Circuito Defensivo Ponderado: Avalia a duração REAL estimada, não a nominal!
            if (effectiveDurationMinutes > availableWindowMinutes)
                continue;

            // --- Cálculo Ponderado Adaptativo ---

            double mTime = 1.0;

            // Compatibilidade Energética com Viés Cronobiológico
            double baseEnergyMatch = 1.0 - (Math.Abs(userEnergyLevel - energyRequired) / 2.0);
            double mEnergy = Math.Clamp(baseEnergyMatch * timeOfDayEnergyBias, 0.0, 1.0);

            double uDeadline = CalculateUrgencyScore(deadline, nowUtc, isHabit);

            double aStrategy = 0.0;
            if (candidate.ProjectId.HasValue && activeGoalProjectIds.Contains(candidate.ProjectId.Value))
            {
                aStrategy = 1.0;
            }

            // Somatório Matricial
            double rawScore = (effectiveWeightUrgency * uDeadline)
                            + (effectiveWeightTime * mTime)
                            + (effectiveWeightEnergy * mEnergy)
                            + (effectiveWeightStrategy * aStrategy);

            double pFriction = postponedCount * PenaltyPerPostpone;
            double finalScore = Math.Clamp(rawScore - pFriction, 0.0, 1.0);

            double percentage = Math.Round(finalScore * 100.0, 1);

            // Explicabilidade Enriquecida para a UI
            string reason = GenerateAdaptiveReason(
                candidate, 
                uDeadline, 
                mEnergy, 
                aStrategy, 
                availableWindowMinutes, 
                isHabit, 
                wasTimeAdjusted, 
                timeOfDayEnergyBias);

            scoredList.Add(new ScoredCommitment(
                candidate, 
                percentage, 
                reason, 
                wasTimeAdjusted, 
                effectiveDurationMinutes));
        }

        return scoredList
            .OrderByDescending(s => s.ScorePercentage)
            .ThenBy(s => s.EffectiveDurationMinutes)
            .ThenBy(s => s.Commitment.Title)
            .Take(3)
            .ToList()
            .AsReadOnly();
    }

    private static DateTime ConvertToLocalTimeSafe(DateTime nowUtc, string timeZoneId)
    {
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);
        }
        catch
        {
            return nowUtc.AddHours(-3); // Fallback de contingência para UTC-3
        }
    }

    private static double CalculateUrgencyScore(DateTime? deadline, DateTime nowUtc, bool isHabit)
    {
        if (isHabit) return 0.85;
        if (!deadline.HasValue) return 0.20;

        var hoursRemaining = (deadline.Value - nowUtc).TotalHours;

        if (hoursRemaining <= 0) return 1.00;
        if (hoursRemaining <= 24) return 0.90;
        if (hoursRemaining <= 72) return 0.70;
        if (hoursRemaining <= 168) return 0.40;

        return 0.25;
    }

    private static string GenerateAdaptiveReason(
        Commitment commitment,
        double uDeadline,
        double mEnergy,
        double aStrategy,
        int availableMinutes,
        bool isHabit,
        bool wasTimeAdjusted,
        double timeOfDayBias)
    {
        if (wasTimeAdjusted && commitment is TaskCommitment task)
        {
            return $" Duração ajustada com base no seu histórico real de entrega ({task.EstimatedDurationMinutes}m ➔ EAI calibrado).";
        }

        if (timeOfDayBias > 1.10 && mEnergy >= 0.8)
        {
            return $" Priorizado para aproveitar seu pico cronobiológico de energia no período atual.";
        }

        if (timeOfDayBias < 0.85 && commitment is TaskCommitment t && t.EnergyRequired == 1)
        {
            return $" Ação leve selecionada para respeitar sua curva fisiológica de fadiga neste horário.";
        }

        if (isHabit && commitment is HabitCommitment habit)
        {
            return $" Hábito diário pendente (Streak atual: {habit.CurrentStreak} dias) compatível com sua janela livre.";
        }

        if (aStrategy > 0.0 && mEnergy >= 0.5)
        {
            return $" Alinhado ao seu objetivo estratégico e perfeito para seus {availableMinutes}m disponíveis.";
        }

        if (uDeadline >= 0.90)
        {
            return $" Prioridade máxima: prazo de conclusão próximo ou excedido.";
        }

        return $" Compatível com sua agenda atual e tempo livre de {availableMinutes} minutos.";
    }
}