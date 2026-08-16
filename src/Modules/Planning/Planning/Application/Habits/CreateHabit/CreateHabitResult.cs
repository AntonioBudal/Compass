using System;
using HabitStatus = Compass.Modules.Planning.Domain.Habits.HabitStatus;

namespace Compass.Modules.Planning.Application.Habits.CreateHabit;

public record CreateHabitResult(Guid HabitId, HabitStatus Status);
