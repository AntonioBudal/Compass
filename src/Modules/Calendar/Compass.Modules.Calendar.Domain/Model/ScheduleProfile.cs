using Compass.Modules.Calendar.Domain.Exceptions;

namespace Compass.Modules.Calendar.Domain.Model;

public class ScheduleProfile
{
    private readonly List<DayAvailabilityRule> _weeklyAvailability = [];

    public Guid Id { get; private set; }
    public TimeZoneId TimeZone { get; private set; } = null!;
    public IReadOnlyCollection<DayAvailabilityRule> WeeklyAvailability => _weeklyAvailability.AsReadOnly();
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // EF Core Constructor
    private ScheduleProfile() { }

    public static ScheduleProfile Create(
        TimeZoneId timeZone,
        IEnumerable<DayAvailabilityRule>? weeklyAvailability,
        DateTimeOffset? now = null)
    {
        ArgumentNullException.ThrowIfNull(timeZone);

        var timestamp = (now ?? DateTimeOffset.UtcNow).ToUniversalTime();

        var profile = new ScheduleProfile
        {
            Id = Guid.CreateVersion7(),
            TimeZone = timeZone,
            CreatedAt = timestamp,
            UpdatedAt = timestamp
        };

        if (weeklyAvailability != null)
        {
            foreach (var rule in weeklyAvailability)
            {
                rule.SetScheduleProfileId(profile.Id);
                profile._weeklyAvailability.Add(rule);
            }
        }

        return profile;
    }

    public void UpdateWeeklyAvailability(IEnumerable<DayAvailabilityRule> rules, DateTimeOffset? now = null)
    {
        ArgumentNullException.ThrowIfNull(rules);

        _weeklyAvailability.Clear();
        foreach (var rule in rules)
        {
            rule.SetScheduleProfileId(Id);
            _weeklyAvailability.Add(rule);
        }

        UpdatedAt = (now ?? DateTimeOffset.UtcNow).ToUniversalTime();
    }

    public void UpdateTimeZone(TimeZoneId timeZone, DateTimeOffset? now = null)
    {
        TimeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
        UpdatedAt = (now ?? DateTimeOffset.UtcNow).ToUniversalTime();
    }
}
