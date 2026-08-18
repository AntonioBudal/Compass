using System;
using System.Collections.Generic;
using System.Linq;

namespace Compass.Modules.Calendar.Domain.Availability;

public class AvailabilityCalculator
{
    public IReadOnlyList<TimeWindow> Calculate(
        IEnumerable<TimeWindow> baseWindows, 
        IEnumerable<TimeWindow> blockedWindows)
    {
        var result = new List<TimeWindow>();
        var mergedBlocks = MergeBlocks(blockedWindows);

        foreach (var baseWindow in baseWindows.OrderBy(w => w.Start))
        {
            var currentPieces = new List<TimeWindow> { baseWindow };

            foreach (var block in mergedBlocks)
            {
                var nextPieces = new List<TimeWindow>();
                
                foreach (var piece in currentPieces)
                {
                    // Se o bloqueio não toca neste pedaço, o pedaço continua inteiro
                    if (block.End <= piece.Start || block.Start >= piece.End)
                    {
                        nextPieces.Add(piece);
                        continue;
                    }

                    // Se sobrou um pedaço à esquerda do bloqueio
                    if (block.Start > piece.Start)
                    {
                        nextPieces.Add(new TimeWindow(piece.Start, block.Start));
                    }

                    // Se sobrou um pedaço à direita do bloqueio
                    if (block.End < piece.End)
                    {
                        nextPieces.Add(new TimeWindow(block.End, piece.End));
                    }
                    
                    // Nota: Se o bloqueio engole o pedaço inteiro, ele simplesmente não é adicionado a nextPieces
                }
                currentPieces = nextPieces; // Atualiza a lista de pedaços cortados para o próximo bloqueio
            }

            // Apenas adiciona pedaços que possuem duração positiva
            result.AddRange(currentPieces.Where(p => p.DurationMinutes > 0));
        }

        return result.OrderBy(w => w.Start).ToList();
    }

    private List<TimeWindow> MergeBlocks(IEnumerable<TimeWindow> blocks)
    {
        var sorted = blocks.OrderBy(b => b.Start).ToList();
        var merged = new List<TimeWindow>();
        
        if (!sorted.Any()) return merged;

        var current = sorted[0];
        
        for (int i = 1; i < sorted.Count; i++)
        {
            var next = sorted[i];
            
            // Se os blocos se sobrepõem ou são adjacentes, nós os fundimos
            if (next.Start <= current.End)
            {
                var maxEnd = next.End > current.End ? next.End : current.End;
                current = new TimeWindow(current.Start, maxEnd);
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
