using Compass.SharedKernel.Domain;
using Compass.SharedKernel.Domain.Exceptions;

namespace Compass.Modules.Planning.Domain.Habits;

public enum HabitFrequencyType
{
    Interval,
    Weekly
}

public sealed class HabitFrequency : ValueObject
{
    public HabitFrequencyType Type { get; private set; }
    public int? IntervalDays { get; private set; }
    
    private readonly int _daysOfWeekBitmask;

    public IReadOnlyCollection<DayOfWeek> DaysOfWeek => DecodeBitmask(_daysOfWeekBitmask);

    private HabitFrequency() { }

    private HabitFrequency(HabitFrequencyType type, int? intervalDays, int bitmask)
    {
        Type = type;
        IntervalDays = intervalDays;
        _daysOfWeekBitmask = bitmask;
    }

    public static HabitFrequency CreateInterval(int days)
    {
        if (days <= 0) throw new DomainException("Interval must be at least 1 day.");
        return new HabitFrequency(HabitFrequencyType.Interval, days, 0);
    }

    public static HabitFrequency CreateWeekly(IEnumerable<DayOfWeek> days)
    {
        var daysList = days.ToList();
        if (!daysList.Any()) throw new DomainException("Weekly frequency requires at least one day.");
        
        int bitmask = EncodeBitmask(daysList);
        return new HabitFrequency(HabitFrequencyType.Weekly, null, bitmask);
    }

    private static int EncodeBitmask(IEnumerable<DayOfWeek> days) =>
        days.Aggregate(0, (current, day) => current | (1 << (int)day));

    private static IReadOnlyCollection<DayOfWeek> DecodeBitmask(int bitmask)
    {
        var days = new List<DayOfWeek>();
        for (int i = 0; i < 7; i++)
        {
            if ((bitmask & (1 << i)) != 0) days.Add((DayOfWeek)i);
        }
        return days.AsReadOnly();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return IntervalDays ?? -1;
        yield return _daysOfWeekBitmask;
    }
}
