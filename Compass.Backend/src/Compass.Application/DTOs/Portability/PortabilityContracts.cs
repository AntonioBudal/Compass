namespace Compass.Application.DTOs.Portability;

public record SettingExportDto(short DefaultEnergyLevel, string Theme, bool AutoPostponeEnabled, string DailyReviewTime, string PreferencesJson);
public record ScoringProfileExportDto(int SampleCount, double EaiMultiplier, double MorningBias, double AfternoonBias, double EveningBias, double NightBias);
public record ProjectExportDto(Guid Id, string Title, string Status, int TotalEstimatedMinutes, DateTime? LastUsedAt);
public record CommitmentExportDto(Guid Id, string Title, string Type, string Status, int EstimatedMinutes, short EnergyRequired, int PostponedCount, DateTime? Deadline, DateTime CreatedAt, DateTime? CompletedAt, Guid? ProjectId);
public record FocusSessionExportDto(Guid Id, Guid CommitmentId, DateTime StartTimeUtc, DateTime? EndTimeUtc, int ActualMinutes);
public record DailyReviewExportDto(Guid Id, string ReviewDate, int CompletedCount, int PostponedCount, int TotalFocusMinutes, string Notes);

// Pacote Raiz Autossuficiente
public record PortabilityBundleDto(
    string ExportedAtUtc,
    string SchemaVersion,
    Guid UserId,
    SettingExportDto? Settings,
    ScoringProfileExportDto? AdaptiveProfile,
    IReadOnlyList<ProjectExportDto> Projects,
    IReadOnlyList<CommitmentExportDto> Commitments,
    IReadOnlyList<FocusSessionExportDto> FocusSessions,
    IReadOnlyList<DailyReviewExportDto> DailyReviews
);

// Resultado estatístico do processamento de importação
public record PortabilityImportResultDto(
    bool Success,
    int ProjectsImported,
    int CommitmentsImported,
    int FocusSessionsImported,
    int DailyReviewsImported,
    bool ProfileUpdated,
    bool SettingsUpdated,
    string Message
);