using System;
using System.Collections.Generic;
using System.Linq;

namespace Compass.Modules.Execution.Domain.DecisionEngine;

public class DailyDecisionEngine
{
    public DailyPlan Build(Guid profileId, DateOnly date, IEnumerable<WorkCandidate> candidates, IEnumerable<AvailableWindow> windows)
    {
        var plan = new DailyPlan(profileId, date);

        // A Timeline Absoluta recebida do Calendar
        var sortedWindows = windows.OrderBy(w => w.Start).ToList();

        var pendingCandidates = candidates
            .OrderBy(c => c.Priority)
            .ThenBy(c => c.Deadline.HasValue ? 0 : 1)
            .ThenBy(c => c.Deadline)
            .ToList();

        foreach (var window in sortedWindows)
        {
            var currentStart = window.Start; // DateTimeOffset
            var currentEnd = window.End;

            bool allocatedInWindow = true;

            while (allocatedInWindow && pendingCandidates.Any())
            {
                allocatedInWindow = false;

                for (int i = 0; i < pendingCandidates.Count; i++)
                {
                    var candidate = pendingCandidates[i];
                    var candidateDuration = TimeSpan.FromMinutes(candidate.EstimatedMinutes);

                    if (currentStart.Add(candidateDuration) <= currentEnd)
                    {
                        var startUtc = currentStart;
                        var endUtc = startUtc.Add(candidateDuration);

                        plan.AddSuggestion(new SuggestedExecution(
                            candidate.ReferenceId,
                            candidate.Type,
                            candidate.Title,
                            startUtc,
                            endUtc,
                            $"Prioridade {candidate.Priority}"
                        ));

                        currentStart = endUtc;
                        pendingCandidates.RemoveAt(i);
                        allocatedInWindow = true; 
                        break;
                    }
                }
            }
        }

        return plan;
    }
}


