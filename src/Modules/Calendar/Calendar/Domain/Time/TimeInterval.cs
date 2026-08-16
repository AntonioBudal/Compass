using System;
using System.Collections.Generic;
using System.Linq;
using Compass.SharedKernel.Domain.Exceptions;

namespace Compass.Modules.Calendar.Domain.Time;

public record TimeInterval
{
    public DateTimeOffset Start { get; }
    public DateTimeOffset End { get; }

    public TimeInterval(DateTimeOffset start, DateTimeOffset end)
    {
        if (end <= start)
            throw new DomainException("Time interval must have an end time strictly greater than its start time.");
            
        Start = start;
        End = end;
    }

    // Verifica se colidem. Fronteiras idênticas (ex: 10h-12h e 12h-14h) NÃO colidem.
    public bool OverlapsWith(TimeInterval other)
    {
        return Start < other.End && other.Start < End;
    }

    // Retorna a intersecção geométrica entre dois intervalos (se houver)
    public TimeInterval? Intersect(TimeInterval other)
    {
        if (!OverlapsWith(other)) return null;
        
        var maxStart = Start > other.Start ? Start : other.Start;
        var minEnd = End < other.End ? End : other.End;
        
        if (maxStart >= minEnd) return null;
        
        return new TimeInterval(maxStart, minEnd);
    }

    // A Mágica: Subtrai um obstáculo (other) deste intervalo, retornando o que sobra.
    public IReadOnlyList<TimeInterval> Subtract(TimeInterval other)
    {
        if (!OverlapsWith(other)) return new[] { this }; // Sem colisão, sobra tudo.

        var result = new List<TimeInterval>();

        // Se o obstáculo começa depois de nós, sobra o pedaço inicial.
        if (Start < other.Start)
        {
            result.Add(new TimeInterval(Start, other.Start));
        }

        // Se o obstáculo termina antes de nós, sobra o pedaço final.
        if (End > other.End)
        {
            result.Add(new TimeInterval(other.End, End));
        }

        return result; // Pode retornar 0 (engolido), 1 (tangente) ou 2 (furado no meio).
    }

    // Achatamento: Funde intervalos que se sobrepõem ou se tocam.
    public static IReadOnlyList<TimeInterval> Merge(IEnumerable<TimeInterval> intervals)
    {
        var sorted = intervals.OrderBy(i => i.Start).ToList();
        if (!sorted.Any()) return new List<TimeInterval>();

        var merged = new List<TimeInterval>();
        var current = sorted[0];

        for (int i = 1; i < sorted.Count; i++)
        {
            var next = sorted[i];
            
            // Se sobrepõem ou se tocam perfeitamente (>=)
            if (current.End >= next.Start)
            {
                var maxEnd = current.End > next.End ? current.End : next.End;
                current = new TimeInterval(current.Start, maxEnd); // Estica o bloco
            }
            else
            {
                merged.Add(current);
                current = next;
            }
        }
        merged.Add(current);
        
        return merged;
    }
}
