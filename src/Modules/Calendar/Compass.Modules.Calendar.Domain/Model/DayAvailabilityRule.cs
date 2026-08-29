namespace Compass.Modules.Calendar.Domain.Model;

public class DayAvailabilityRule
{
    public Guid Id { get; private set; }
    public Guid ScheduleProfileId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public IReadOnlyList<TimeWindow> Windows { get; private set; } = [];

    // EF Core Constructor
    private DayAvailabilityRule() { }

    public DayAvailabilityRule(DayOfWeek dayOfWeek, IEnumerable<TimeWindow> windows)
    {
        Id = Guid.CreateVersion7();
        DayOfWeek = dayOfWeek;
        Windows = TimeWindow.Normalize(windows);
    }

    public void SetScheduleProfileId(Guid profileId)
    {
        ScheduleProfileId = profileId;
    }

    public void UpdateWindows(IEnumerable<TimeWindow> windows)
    {
        Windows = TimeWindow.Normalize(windows);
    }
}
