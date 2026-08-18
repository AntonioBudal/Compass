using System;
using System.Collections.Generic;

namespace Compass.Modules.Execution.Domain.DecisionEngine;

public record WorkCandidate(Guid ReferenceId, string Title, string Type, int EstimatedMinutes, DateTimeOffset? Deadline, int Priority);

public record AvailableWindow(DateTimeOffset Start, DateTimeOffset End)
{
    public double DurationMinutes => (End - Start).TotalMinutes;
}

public class SuggestedExecution
{
    public Guid Id { get; private set; }
    public Guid DailyPlanId { get; private set; }
    public Guid ReferenceId { get; private set; }
    public string Type { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public DateTimeOffset Start { get; private set; }
    public DateTimeOffset End { get; private set; }
    public string Reason { get; private set; } = null!;

    private SuggestedExecution() { } // ORM

    public SuggestedExecution(Guid referenceId, string type, string title, DateTimeOffset start, DateTimeOffset end, string reason)
    {
        Id = Guid.NewGuid();
        ReferenceId = referenceId;
        Type = type;
        Title = title;
        Start = start;
        End = end;
        Reason = reason;
    }
}

public class DailyPlan
{
    public Guid Id { get; private set; }
    public Guid ProfileId { get; private set; }
    public DateOnly Date { get; private set; }
    
    private readonly List<SuggestedExecution> _suggestions = new();
    public IReadOnlyList<SuggestedExecution> Suggestions => _suggestions.AsReadOnly();

    private DailyPlan() { } // ORM

    public DailyPlan(Guid profileId, DateOnly date)
    {
        Id = Guid.NewGuid();
        ProfileId = profileId;
        Date = date;
    }

    public void AddSuggestion(SuggestedExecution suggestion)
    {
        _suggestions.Add(suggestion);
    }
}
