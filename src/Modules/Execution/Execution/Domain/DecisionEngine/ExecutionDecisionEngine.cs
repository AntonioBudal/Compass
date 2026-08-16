using System;
using System.Collections.Generic;
using System.Linq;
using Compass.Modules.Execution.Domain.Time;

namespace Compass.Modules.Execution.Domain.DecisionEngine;

public class ExecutionDecisionEngine
{
    public IReadOnlyList<Recommendation> GenerateRecommendations(
        IEnumerable<TaskCandidate> candidates,
        IEnumerable<AvailableSlot> slots,
        ExecutionHistory history,
        DateTimeOffset now,
        TimeSpan minimumUsefulDuration,
        int topN = 3)
    {
        // Fase 1: Subtração Geométrica (Tempo Restante Real)
        var remainingSlots = CalculateRemainingSlots(slots, history);

        var recommendations = new List<Recommendation>();

        foreach (var slot in remainingSlots)
        {
            // Fase 2 (A): Filtro de Passado e Limite Útil
            if (slot.End <= now || (slot.End - slot.Start) < minimumUsefulDuration)
                continue;

            // Ajusta o início do slot se já estivermos dentro dele (Tempo decorrendo)
            var effectiveStart = slot.Start < now ? now : slot.Start;
            var effectiveSlot = new TimeInterval(effectiveStart, slot.End);
            var slotDuration = effectiveSlot.End - effectiveSlot.Start;

            if (slotDuration < minimumUsefulDuration) continue;

            foreach (var task in candidates)
            {
                // Fase 2 (B): Filtro de Tarefa Finalizada
                if (task.RemainingDuration <= TimeSpan.Zero)
                    continue;

                // Fase 3: Cálculo do Intervalo Sugerido e Chunking
                var suggestedDuration = task.RemainingDuration < slotDuration ? task.RemainingDuration : slotDuration;
                var suggestedInterval = new TimeInterval(effectiveSlot.Start, effectiveSlot.Start.Add(suggestedDuration));

                // Fase 4: Cálculo do Score
                var factors = CalculateFactors(task, suggestedDuration, slotDuration);
                var totalScore = (factors.PriorityScore * 60m) + (factors.WindowFitScore * 40m);

                recommendations.Add(new Recommendation(task.Id, suggestedInterval, totalScore, factors));
            }
        }

        // Fase 6: Ranking e Desempate Determinístico (Tie-breaking)
        var ranked = recommendations
            .OrderByDescending(r => r.Score)
            .ThenByDescending(r => GetPriorityWeight(candidates.First(c => c.Id == r.TaskId).Priority))
            .ThenBy(r => candidates.First(c => c.Id == r.TaskId).RemainingDuration) // Quick wins primeiro
            .ThenBy(r => r.SuggestedInterval.Start) // O que acontece mais cedo
            .ThenBy(r => r.TaskId) // Fallback alfanumérico estrito
            .ToList();

        // Fase 7: Top N Selection (Limpa duplicidades da mesma tarefa caso ela apareça em vários slots, pegando o melhor)
        return ranked
            .GroupBy(r => r.TaskId)
            .Select(g => g.First())
            .Take(topN)
            .ToList()
            .AsReadOnly();
    }

    private IEnumerable<TimeInterval> CalculateRemainingSlots(IEnumerable<AvailableSlot> slots, ExecutionHistory history)
    {
        // Achata o histórico se houver sobreposições anômalas, para não subtrair o mesmo buraco duas vezes
        var mergedHistory = TimeInterval.Merge(history.Intervals);
        var currentSlots = slots.Select(s => s.Interval).ToList();

        foreach (var pastLog in mergedHistory)
        {
            var nextSlots = new List<TimeInterval>();
            foreach (var slot in currentSlots)
            {
                nextSlots.AddRange(slot.Subtract(pastLog));
            }
            currentSlots = nextSlots;
        }

        return currentSlots;
    }

    private DecisionFactors CalculateFactors(TaskCandidate task, TimeSpan suggestedDuration, TimeSpan slotDuration)
    {
        // Priority (60%)
        var priorityScore = task.Priority switch
        {
            TaskPriority.High => 1.0m,
            TaskPriority.Medium => 0.5m,
            TaskPriority.Low => 0.1m,
            _ => 0.1m
        };

        // Window Fit (40%)
        decimal windowFitScore;
        bool isPerfectFit = false;
        bool isChunked = false;

        var ratio = (decimal)suggestedDuration.Ticks / slotDuration.Ticks;

        if (ratio == 1.0m && task.RemainingDuration == suggestedDuration)
        {
            windowFitScore = 1.0m;
            isPerfectFit = true;
        }
        else if (ratio < 1.0m)
        {
            windowFitScore = ratio; // Penalidade por deixar buraco sobrando
        }
        else // ratio == 1.0m mas a tarefa ainda precisa de mais tempo (Chunking)
        {
            windowFitScore = 0.8m; 
            isChunked = true;
        }

        return new DecisionFactors(isPerfectFit, isChunked, priorityScore, windowFitScore);
    }

    private int GetPriorityWeight(TaskPriority priority) => (int)priority;
}
