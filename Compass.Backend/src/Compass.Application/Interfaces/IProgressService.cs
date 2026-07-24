using Compass.Application.DTOs.Analytics;

namespace Compass.Application.Interfaces;

public interface IProgressService
{
    Task<ProgressOverviewDto> GetOverviewAsync(
        Guid userId, 
        string timeRange, 
        string timeZoneId, 
        CancellationToken cancellationToken = default);

    Task<IEnumerable<DailyTimeSliceDto>> GetDailyTimeSeriesAsync(
        Guid userId, 
        string timeRange, 
        string timeZoneId, 
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ProcrastinationHeatmapDto>> GetProcrastinationHeatmapAsync(
        Guid userId, 
        string timeRange, 
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ChronologyPeakDto>> GetChronologyPeaksAsync(
        Guid userId, 
        string timeRange, 
        string timeZoneId, 
        CancellationToken cancellationToken = default);
}