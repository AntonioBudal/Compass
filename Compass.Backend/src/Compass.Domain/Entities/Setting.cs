using Compass.Domain.Exceptions;

namespace Compass.Domain.Entities;

public class Setting
{
    public Guid UserId { get; private set; }
    public short DefaultEnergyLevel { get; private set; }
    public string Theme { get; private set; } = string.Empty;
    public bool AutoPostponeEnabled { get; private set; }
    public TimeOnly DailyReviewTime { get; private set; }
    public string PreferencesJson { get; private set; } = string.Empty;
    public DateTime UpdatedAt { get; private set; }

    protected Setting() { }

    public Setting(Guid userId, short defaultEnergyLevel = 2, string theme = "dark")
    {
        if (defaultEnergyLevel < 1 || defaultEnergyLevel > 3)
            throw new DomainException("O nível de energia padrão deve estar entre 1 (Baixa) e 3 (Alta).");

        UserId = userId;
        DefaultEnergyLevel = defaultEnergyLevel;
        Theme = theme;
        AutoPostponeEnabled = true;
        DailyReviewTime = new TimeOnly(20, 0); // 20:00 padrão
        PreferencesJson = "{}";
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePreferences(short energyLevel, string theme, bool autoPostpone, TimeOnly reviewTime, string preferencesJson)
    {
        if (energyLevel < 1 || energyLevel > 3)
            throw new DomainException("O nível de energia deve estar entre 1 e 3.");

        DefaultEnergyLevel = energyLevel;
        Theme = theme;
        AutoPostponeEnabled = autoPostpone;
        DailyReviewTime = reviewTime;
        PreferencesJson = preferencesJson;
        UpdatedAt = DateTime.UtcNow;
    }
}