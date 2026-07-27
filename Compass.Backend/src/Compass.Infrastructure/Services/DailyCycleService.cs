using Compass.Application.DTOs.DailyCycle;
using Compass.Domain.Entities;
using Compass.Domain.Enums;
using Compass.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Compass.Infrastructure.Services;

public interface IDailyCycleService
{
    Task<MorningBriefingDto> GetMorningBriefingAsync(Guid userId, string timeZoneId = "America/Sao_Paulo", CancellationToken cancellationToken = default);
    Task<DailyShutdownResponseDto> ExecuteShutdownAsync(Guid userId, DailyShutdownRequestDto request, string timeZoneId = "America/Sao_Paulo", CancellationToken cancellationToken = default);
}

public class DailyCycleService : IDailyCycleService
{
    private readonly CompassDbContext _context;
    private readonly ILogger<DailyCycleService> _logger;

    public DailyCycleService(CompassDbContext context, ILogger<DailyCycleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MorningBriefingDto> GetMorningBriefingAsync(
        Guid userId, 
        string timeZoneId = "America/Sao_Paulo", 
        CancellationToken cancellationToken = default)
    {
        var tz = GetTimeZoneSafe(timeZoneId);
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        var today = DateOnly.FromDateTime(nowLocal);

        var pendingItems = await _context.Commitments
            .AsNoTracking()
            .Where(c => c.UserId == userId && c.Status == CommitmentStatus.Pending)
            .Select(c => new
            {
                c.Title,
                c.Type,
                Deadline = EF.Property<DateTime?>(c, "Deadline"),
                Duration = EF.Property<int?>(c, "EstimatedDurationMinutes") ?? 30
            })
            .ToListAsync(cancellationToken);

        int pendingCount = pendingItems.Count(x => x.Type == CommitmentType.Task);
        int habitsCount = pendingItems.Count(x => x.Type == CommitmentType.Habit);
        int overdueCount = pendingItems.Count(x => x.Deadline.HasValue && x.Deadline.Value < DateTime.UtcNow);
        int totalMinutes = pendingItems.Sum(x => x.Duration);

        var topItem = pendingItems.FirstOrDefault()?.Title ?? "Nenhuma pendência prioritária!";
        string greeting = nowLocal.Hour < 12 ? "Bom dia, Operador." : "Boa tarde, Operador.";

        return new MorningBriefingDto(today, pendingCount, overdueCount, habitsCount, totalMinutes, topItem, greeting);
    }

    public async Task<DailyShutdownResponseDto> ExecuteShutdownAsync(
        Guid userId, 
        DailyShutdownRequestDto request, 
        string timeZoneId = "America/Sao_Paulo", 
        CancellationToken cancellationToken = default)
    {
        var tz = GetTimeZoneSafe(timeZoneId);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz));

        _logger.LogInformation("[DailyShutdown] Processando encerramento de {Date} para usuário {UserId}", today, userId);

        // FORMATAÇÃO TÁTICA DE DIVERGÊNCIAS: Injeta as tags no início das notas analíticas
        string formattedNotes = request.Notes?.Trim() ?? "";
        if (request.DivergenceTags != null && request.DivergenceTags.Any())
        {
            var cleanTags = request.DivergenceTags.Select(t => t.StartsWith("#") ? t : $"#{t}");
            formattedNotes = $"[TAGS: {string.Join(" ", cleanTags)}] {formattedNotes}".Trim();
        }

        // Verifica se já houve encerramento hoje e faz atualização (Upsert funcional)
        var existingReview = await _context.DailyReviews
            .FirstOrDefaultAsync(d => d.UserId == userId && d.ReviewDate == today, cancellationToken);

        Guid reviewId;
        if (existingReview != null)
        {
            _logger.LogDebug("[DailyShutdown] Revisão já existente para hoje. Substituindo via deleção atômica e recriação.");
            _context.DailyReviews.Remove(existingReview);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var newReview = new DailyReview(
            userId: userId,
            reviewDate: today,
            completedCount: request.CompletedCount,
            postponedCount: request.PostponedCount,
            totalFocusMinutes: request.TotalFocusMinutes,
            notes: formattedNotes
        );

        _context.DailyReviews.Add(newReview);
        await _context.SaveChangesAsync(cancellationToken);
        reviewId = newReview.Id;

        _logger.LogInformation("[DailyShutdown] Revisão diária {ReviewId} registrada com sucesso. Tags: {Tags}", reviewId, string.Join(",", request.DivergenceTags ?? new List<string>()));

        return new DailyShutdownResponseDto(reviewId, today, "Encerramento diário e telemetria analítica salvos com sucesso!", true);
    }

    private static TimeZoneInfo GetTimeZoneSafe(string timeZoneId)
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId); }
        catch { return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo"); }
    }
}