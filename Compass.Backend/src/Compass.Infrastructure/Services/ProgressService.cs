using Compass.Application.DTOs.Analytics;
using Compass.Application.Interfaces;
using Compass.Domain.Entities;
using Compass.Domain.Enums;
using Compass.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compass.Infrastructure.Services;

public class ProgressService : IProgressService
{
    private readonly CompassDbContext _context;

    public ProgressService(CompassDbContext context)
    {
        _context = context;
    }

    public async Task<ProgressOverviewDto> GetOverviewAsync(
        Guid userId, 
        string timeRange, 
        string timeZoneId, 
        CancellationToken cancellationToken = default)
    {
        var (startUtc, endUtc) = CalculateHistoricalRangeUtc(timeRange, timeZoneId);

        var commitmentsQuery = _context.Commitments
            .AsNoTracking()
            .Where(c => c.UserId == userId && c.CreatedAt >= startUtc && c.CreatedAt <= endUtc && c.Status != CommitmentStatus.Archived);

        var totalPlanned = await commitmentsQuery.CountAsync(cancellationToken);
        
        var completedList = await commitmentsQuery
            .Where(c => c.Status == CommitmentStatus.Completed && c.CompletedAt != null && c.CompletedAt <= endUtc)
            .Select(c => new 
            {
                c.Id,
                c.Type,
                Duration = EF.Property<int?>(c, "EstimatedDurationMinutes") ?? 0,
                Energy = EF.Property<short?>(c, "EnergyRequired") ?? (short)0,
                Postponed = EF.Property<int?>(c, "PostponedCount") ?? 0
            })
            .ToListAsync(cancellationToken);

        int totalCompleted = completedList.Count;
        double completionRate = totalPlanned > 0 ? Math.Round((double)totalCompleted / totalPlanned * 100.0, 1) : 0.0;
        
        int totalPostponements = completedList.Where(c => c.Type == CommitmentType.Task).Sum(c => c.Postponed);
        int totalUsefulMinutes = completedList.Sum(c => c.Duration);
        int totalDeepWorkMinutes = completedList.Where(c => c.Energy == 3).Sum(c => c.Duration);

        var completedIds = completedList.Select(c => c.Id).ToHashSet();
        var focusSessions = await _context.FocusSessions
            .AsNoTracking()
            .Where(f => f.UserId == userId && completedIds.Contains(f.CommitmentId) && f.StartTime >= startUtc && f.EndTime <= endUtc)
            .GroupBy(f => f.CommitmentId)
            .Select(g => new { CommitmentId = g.Key, ActualTotal = g.Sum(f => f.ActualDurationMinutes) })
            .ToDictionaryAsync(x => x.CommitmentId, x => x.ActualTotal, cancellationToken);

        double totalEstimatedForAccuracy = 0;
        double totalActualForAccuracy = 0;
        bool hasImputedData = false;

        foreach (var item in completedList)
        {
            if (item.Duration <= 0) continue;

            totalEstimatedForAccuracy += item.Duration;
            if (focusSessions.TryGetValue(item.Id, out int actualMinutes))
            {
                totalActualForAccuracy += Math.Min(actualMinutes, item.Duration * 3);
            }
            else
            {
                totalActualForAccuracy += item.Duration;
                hasImputedData = true;
            }
        }

        double eai = totalEstimatedForAccuracy > 0 
            ? Math.Round(totalActualForAccuracy / totalEstimatedForAccuracy, 2) 
            : 1.0;

        return new ProgressOverviewDto(
            totalCompleted,
            totalPlanned,
            completionRate,
            eai,
            hasImputedData,
            totalDeepWorkMinutes,
            totalUsefulMinutes,
            totalPostponements,
            startUtc,
            endUtc
        );
    }

    public async Task<IEnumerable<DailyTimeSliceDto>> GetDailyTimeSeriesAsync(
        Guid userId, 
        string timeRange, 
        string timeZoneId, 
        CancellationToken cancellationToken = default)
    {
        var (startUtc, endUtc) = CalculateHistoricalRangeUtc(timeRange, timeZoneId);

        var completedItems = await _context.Commitments
            .AsNoTracking()
            .Where(c => c.UserId == userId 
                     && c.Status == CommitmentStatus.Completed 
                     && c.CompletedAt != null 
                     && c.CompletedAt >= startUtc 
                     && c.CompletedAt <= endUtc)
            .Select(c => new
            {
                c.CompletedAt,
                c.Type,
                Duration = EF.Property<int?>(c, "EstimatedDurationMinutes") ?? 0,
                Energy = EF.Property<short?>(c, "EnergyRequired") ?? (short)0,
                Postponed = EF.Property<int?>(c, "PostponedCount") ?? 0
            })
            .ToListAsync(cancellationToken);

        var tz = GetTimeZoneSafe(timeZoneId);

        var rawSeries = completedItems
            .GroupBy(c => TimeZoneInfo.ConvertTimeFromUtc(c.CompletedAt!.Value, tz).Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailyTimeSliceDto(
                g.Key.ToString("yyyy-MM-dd"),
                g.Count(),
                g.Sum(x => x.Duration),
                g.Sum(x => x.Duration),
                g.Where(x => x.Energy == 3).Sum(x => x.Duration),
                g.Where(x => x.Type == CommitmentType.Task).Sum(x => x.Postponed)
            ));

        return rawSeries;
    }

    public async Task<IEnumerable<ProcrastinationHeatmapDto>> GetProcrastinationHeatmapAsync(
        Guid userId, 
        string timeRange, 
        CancellationToken cancellationToken = default)
    {
        var (startUtc, endUtc) = CalculateHistoricalRangeUtc(timeRange, "UTC");

        var heatmapItems = await _context.Commitments
            .AsNoTracking()
            .Where(c => c.UserId == userId && c.CreatedAt >= startUtc && c.CreatedAt <= endUtc && c.Status != CommitmentStatus.Archived)
            .Select(c => new
            {
                c.Type,
                Energy = EF.Property<short?>(c, "EnergyRequired") ?? (short)1,
                Postponed = EF.Property<int?>(c, "PostponedCount") ?? 0
            })
            .ToListAsync(cancellationToken);

        var heatmapSeries = heatmapItems
            .GroupBy(c => new { c.Type, c.Energy })
            .Select(g =>
            {
                int total = g.Count();
                int postponed = g.Count(x => x.Postponed > 0);
                double rate = total > 0 ? Math.Round((double)postponed / total * 100.0, 1) : 0.0;

                return new ProcrastinationHeatmapDto(
                    g.Key.Type.ToString().ToUpperInvariant(),
                    g.Key.Energy,
                    total,
                    postponed,
                    rate
                );
            });

        return heatmapSeries;
    }

    public async Task<IEnumerable<ChronologyPeakDto>> GetChronologyPeaksAsync(
        Guid userId, 
        string timeRange, 
        string timeZoneId, 
        CancellationToken cancellationToken = default)
    {
        var (startUtc, endUtc) = CalculateHistoricalRangeUtc(timeRange, timeZoneId);

        var completedItems = await _context.Commitments
            .AsNoTracking()
            .Where(c => c.UserId == userId 
                     && c.Status == CommitmentStatus.Completed 
                     && c.CompletedAt != null 
                     && c.CompletedAt >= startUtc 
                     && c.CompletedAt <= endUtc)
            .Select(c => new
            {
                c.CompletedAt,
                Duration = EF.Property<int?>(c, "EstimatedDurationMinutes") ?? 0,
                Energy = EF.Property<short?>(c, "EnergyRequired") ?? (short)0
            })
            .ToListAsync(cancellationToken);

        var tz = GetTimeZoneSafe(timeZoneId);

        var groupedBuckets = completedItems
            .Select(x => new
            {
                Hour = TimeZoneInfo.ConvertTimeFromUtc(x.CompletedAt!.Value, tz).Hour,
                x.Duration,
                IsDeep = x.Energy == 3
            })
            .GroupBy(x => x.Hour switch
            {
                >= 6 and < 12 => "Morning",
                >= 12 and < 18 => "Afternoon",
                >= 18 and < 24 => "Evening",
                _ => "Night"
            })
            .Select(g =>
            {
                int count = g.Count();
                int deepMinutes = g.Where(x => x.IsDeep).Sum(x => x.Duration);
                int totalMinutes = g.Sum(x => x.Duration);
                double efficiency = totalMinutes > 0 ? Math.Round((double)deepMinutes / totalMinutes * 100.0, 1) : 0.0;

                return new ChronologyPeakDto(g.Key, count, deepMinutes, efficiency);
            });

        var allBuckets = new[] { "Morning", "Afternoon", "Evening", "Night" };
        return allBuckets.GroupJoin(
            groupedBuckets,
            bucket => bucket,
            result => result.TimeBucket,
            (bucket, results) => results.FirstOrDefault() ?? new ChronologyPeakDto(bucket, 0, 0, 0.0)
        );
    }

    private static TimeZoneInfo GetTimeZoneSafe(string timeZoneId)
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId); }
        catch { return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo"); }
    }

    private static (DateTime StartUtc, DateTime EndUtc) CalculateHistoricalRangeUtc(string timeRange, string timeZoneId)
    {
        var tz = GetTimeZoneSafe(timeZoneId);
        DateTime localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        
        DateTime localYesterdayEnd = localNow.Date.AddTicks(-1);
        
        int daysBack = timeRange.ToLowerInvariant() switch
        {
            "7d" => 7,
            "30d" => 30,
            "90d" => 90,
            "1y" => 365,
            _ => 30
        };

        DateTime localStart = localNow.Date.AddDays(-daysBack);

        DateTime startUtc = TimeZoneInfo.ConvertTimeToUtc(localStart, tz);
        DateTime endUtc = TimeZoneInfo.ConvertTimeToUtc(localYesterdayEnd, tz);

        return (startUtc, endUtc);
    }
}