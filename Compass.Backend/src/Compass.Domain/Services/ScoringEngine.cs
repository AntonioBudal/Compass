using Compass.Domain.Entities;
using Compass.Domain.Enums;

namespace Compass.Domain.Services;

public class ScoredCommitment
{
    public Commitment Commitment { get; }
    public double ScorePercentage { get; }
    public string Reason { get; }

    public ScoredCommitment(Commitment commitment, double scorePercentage, string reason)
    {
        Commitment = commitment;
        ScorePercentage = scorePercentage;
        Reason = reason;
    }
}

public static class ScoringEngine
{
    // Pesos da Fórmula de Pontuação (Total base = 1.0 ou 100%)
    private const double WeightUrgency = 0.35;
    private const double WeightTime = 0.25;
    private const double WeightEnergy = 0.20;
    private const double WeightStrategy = 0.20;
    private const double PenaltyPerPostpone = 0.05; // -5% por adiamento

    public static IReadOnlyList<ScoredCommitment> CalculateTopActions(
        IEnumerable<Commitment> candidates,
        int availableWindowMinutes,
        short userEnergyLevel,
        DateTime nowUtc,
        HashSet<Guid> activeGoalProjectIds,
        HashSet<Guid> blockedCommitmentIds)
    {
        var scoredList = new List<ScoredCommitment>();

        foreach (var candidate in candidates)
        {
            // 1. Filtro de Curto-Circuito: Dependências e Status Bloqueado
            if (blockedCommitmentIds.Contains(candidate.Id) || candidate.Status == CommitmentStatus.Blocked)
            {
                continue;
            }

            int estimatedMinutes = 30;
            short energyRequired = 2;
            DateTime? deadline = null;
            int postponedCount = 0;
            bool isHabit = false;

            // Extrair propriedades via Pattern Matching
            if (candidate is TaskCommitment task)
            {
                estimatedMinutes = task.EstimatedDurationMinutes;
                energyRequired = task.EnergyRequired;
                deadline = task.Deadline;
                postponedCount = task.PostponedCount;
            }
            else if (candidate is HabitCommitment habit)
            {
                estimatedMinutes = habit.EstimatedDurationMinutes;
                energyRequired = habit.EnergyRequired;
                isHabit = true;
            }
            else
            {
                // Eventos e Notas não concorrem no motor de sugestões "Agora"
                continue;
            }

            // 2. Filtro de Curto-Circuito: Janela de Tempo Insuficiente (M_tempo = 0)
            if (estimatedMinutes > availableWindowMinutes)
            {
                continue; // Tarefa é limada da lista por não caber no tempo livre atual
            }

            // --- Cálculo dos Componentes da Fórmula ---

            // A. Compatibilidade Temporal (M_tempo) -> Se passou do curto-circuito, é 1.0 (100%)
            double mTime = 1.0;

            // B. Compatibilidade Energética (M_energia) = 1 - (|E_usuário - E_tarefa| / 2)
            double mEnergy = 1.0 - (Math.Abs(userEnergyLevel - energyRequired) / 2.0);

            // C. Urgência do Prazo (U_prazo)
            double uDeadline = CalculateUrgencyScore(deadline, nowUtc, isHabit);

            // D. Alinhamento Estratégico (A_meta)
            double aStrategy = 0.0;
            if (candidate.ProjectId.HasValue && activeGoalProjectIds.Contains(candidate.ProjectId.Value))
            {
                aStrategy = 1.0;
            }

            // --- Somatório Ponderado ---
            double rawScore = (WeightUrgency * uDeadline)
                            + (WeightTime * mTime)
                            + (WeightEnergy * mEnergy)
                            + (WeightStrategy * aStrategy);

            // E. Penalidade por Atrito (P_atrito)
            double pFriction = postponedCount * PenaltyPerPostpone;

            double finalScore = rawScore - pFriction;
            
            // Garantir que o score fique no limite normalizado entre 0.0 e 1.0
            if (finalScore < 0.0) finalScore = 0.0;
            if (finalScore > 1.0) finalScore = 1.0;

            double percentage = Math.Round(finalScore * 100.0, 1);

            // Gerar explicabilidade (transparência da decisão)
            string reason = GenerateReason(candidate, uDeadline, mEnergy, aStrategy, availableWindowMinutes, isHabit);

            scoredList.Add(new ScoredCommitment(candidate, percentage, reason));
        }

        // Ordenar por maior pontuação (determinístico: em caso de empate, prioriza menor duração e depois título)
        return scoredList
            .OrderByDescending(s => s.ScorePercentage)
            .ThenBy(s => s.Commitment is TaskCommitment t ? t.EstimatedDurationMinutes : 15)
            .ThenBy(s => s.Commitment.Title)
            .Take(3)
            .ToList()
            .AsReadOnly();
    }

    private static double CalculateUrgencyScore(DateTime? deadline, DateTime nowUtc, bool isHabit)
    {
        if (isHabit)
        {
            // Hábitos diários possuem alta urgência estrutural para manutenção do Streak
            return 0.85;
        }

        if (!deadline.HasValue)
        {
            return 0.20; // Tarefas sem prazo possuem urgência basal baixa
        }

        var hoursRemaining = (deadline.Value - nowUtc).TotalHours;

        if (hoursRemaining <= 0) return 1.00; // Atrasada ou vencendo agora!
        if (hoursRemaining <= 24) return 0.90; // Vence nas próximas 24 horas
        if (hoursRemaining <= 72) return 0.70; // Vence em 3 dias
        if (hoursRemaining <= 168) return 0.40; // Vence em 1 semana

        return 0.25; // Vence em mais de 1 semana
    }

    private static string GenerateReason(
        Commitment commitment, 
        double uDeadline, 
        double mEnergy, 
        double aStrategy, 
        int availableMinutes, 
        bool isHabit)
    {
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

        if (mEnergy == 1.0)
        {
            return $" Encaixe ideal: exige exatamente o nível de energia que você possui agora.";
        }

        return $" Compatível com sua agenda atual e tempo livre de {availableMinutes} minutos.";
    }
}