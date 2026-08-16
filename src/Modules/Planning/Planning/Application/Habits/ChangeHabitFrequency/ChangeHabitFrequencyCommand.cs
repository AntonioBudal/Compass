using System;
using System.Collections.Generic;

namespace Compass.Modules.Planning.Application.Habits.ChangeHabitFrequency;

public record ChangeHabitFrequencyCommand(
    Guid HabitId,
    int? IntervalDays = null,
    IEnumerable<DayOfWeek>? DaysOfWeek = null
);
