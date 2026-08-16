using System;
using System.Collections.Generic;

namespace Compass.Modules.Planning.Application.Habits.CreateHabit;

// A representação externa da intenção, sem acoplamento direto com o Value Object interno.
public record CreateHabitCommand(
    string Title,
    int EstimatedDurationMinutes,
    int? IntervalDays = null,
    IEnumerable<DayOfWeek>? DaysOfWeek = null
);
