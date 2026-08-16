using System.Collections.Generic;
using System.Linq;
using Compass.SharedKernel.Domain.Exceptions;

namespace Compass.Modules.Calendar.Domain.Profiles;

using Compass.Modules.Calendar.Domain.Time;

public class DaySchedule
{
    public IReadOnlyList<TimeWindow> Windows { get; }

    public DaySchedule(IEnumerable<TimeWindow> windows)
    {
        // 1. Invariante: Ordenação garantida pelo construtor
        var sortedWindows = windows.OrderBy(w => w.Start).ToList();

        // 2. Invariante: Sem sobreposição
        for (int i = 0; i < sortedWindows.Count - 1; i++)
        {
            if (sortedWindows[i].OverlapsWith(sortedWindows[i + 1]))
            {
                throw new DomainException("Time windows within the same day cannot overlap.");
            }
        }

        Windows = sortedWindows.AsReadOnly();
    }
}
