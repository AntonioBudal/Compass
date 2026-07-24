using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Compass.Application.DTOs.Analytics;
using Compass.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Compass.Api.Controllers;

[ApiController]
[Route("api/v1/progress")]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _progressService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProgressController> _logger;

    // TTL Padrão Industrial para séries históricas imutáveis (ontem para trás)
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

    public ProgressController(
        IProgressService progressService, 
        IMemoryCache cache, 
        ILogger<ProgressController> logger)
    {
        _progressService = progressService;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Retorna os KPIs consolidados (Eficácia %, Acurácia EAI, Volume Deep Work).
    /// </summary>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(ProgressOverviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public async Task<IActionResult> GetOverview(
        [FromQuery] string timeRange = "30d", 
        [FromQuery] string timeZoneId = "America/Sao_Paulo", 
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        string cacheKey = $"progress_overview_{userId}_{timeRange.ToLowerInvariant()}_{timeZoneId}";

        return await ServeWithCacheAndETagAsync(cacheKey, async () =>
            await _progressService.GetOverviewAsync(userId, timeRange, timeZoneId, cancellationToken)
        );
    }

    /// <summary>
    /// Retorna a série temporal diária para os gráficos de linhas/barras.
    /// </summary>
    [HttpGet("daily-series")]
    [ProducesResponseType(typeof(IEnumerable<DailyTimeSliceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public async Task<IActionResult> GetDailySeries(
        [FromQuery] string timeRange = "30d", 
        [FromQuery] string timeZoneId = "America/Sao_Paulo", 
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        string cacheKey = $"progress_daily_{userId}_{timeRange.ToLowerInvariant()}_{timeZoneId}";

        return await ServeWithCacheAndETagAsync(cacheKey, async () =>
            await _progressService.GetDailyTimeSeriesAsync(userId, timeRange, timeZoneId, cancellationToken)
        );
    }

    /// <summary>
    /// Retorna a matriz de gargalos operacionais e adiamentos (Heatmap).
    /// </summary>
    [HttpGet("heatmap")]
    [ProducesResponseType(typeof(IEnumerable<ProcrastinationHeatmapDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public async Task<IActionResult> GetHeatmap(
        [FromQuery] string timeRange = "30d", 
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        string cacheKey = $"progress_heatmap_{userId}_{timeRange.ToLowerInvariant()}";

        return await ServeWithCacheAndETagAsync(cacheKey, async () =>
            await _progressService.GetProcrastinationHeatmapAsync(userId, timeRange, cancellationToken)
        );
    }

    /// <summary>
    /// Retorna a distribuição horária para identificação de picos cronobiológicos.
    /// </summary>
    [HttpGet("chronology")]
    [ProducesResponseType(typeof(IEnumerable<ChronologyPeakDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public async Task<IActionResult> GetChronology(
        [FromQuery] string timeRange = "30d", 
        [FromQuery] string timeZoneId = "America/Sao_Paulo", 
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        string cacheKey = $"progress_chronology_{userId}_{timeRange.ToLowerInvariant()}_{timeZoneId}";

        return await ServeWithCacheAndETagAsync(cacheKey, async () =>
            await _progressService.GetChronologyPeaksAsync(userId, timeRange, timeZoneId, cancellationToken)
        );
    }

    /// <summary>
    /// Motor genérico de Caching em Memória e Revalidação HTTP ETag / 304 Not Modified.
    /// </summary>
    private async Task<IActionResult> ServeWithCacheAndETagAsync<T>(string cacheKey, Func<Task<T>> dataFactory)
    {
        // 1. Tentar recuperar o payload ou buscar na infraestrutura e cachear
        if (!_cache.TryGetValue(cacheKey, out CachedPayload<T>? cachedData) || cachedData == null)
        {
            _logger.LogDebug("[ProgressCache] Miss para chave: {CacheKey}. Consultando PostgreSQL...", cacheKey);
            
            T freshData = await dataFactory();
            string etag = GenerateETag(freshData);

            cachedData = new CachedPayload<T>(freshData, etag);

            _cache.Set(cacheKey, cachedData, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheTtl,
                Priority = CacheItemPriority.High
            });
        }
        else
        {
            _logger.LogDebug("[ProgressCache] Hit em memória para chave: {CacheKey}", cacheKey);
        }

        // 2. Aplicar cabeçalhos de controle de cache do navegador (5 minutos local, privado)
        Response.Headers.CacheControl = "private, max-age=300";
        Response.Headers.ETag = $"\"{cachedData.ETag}\"";

        // 3. Avaliar se o cliente já possui a versão mais recente via If-None-Match
        if (Request.Headers.TryGetValue("If-None-Match", out var clientETag))
        {
            string cleanClientETag = clientETag.ToString().Trim('"');
            if (string.Equals(cleanClientETag, cachedData.ETag, StringComparison.Ordinal))
            {
                _logger.LogDebug("[ProgressCache] ETag coincidente ({ETag}). Retornando HTTP 304 Not Modified.", cachedData.ETag);
                return StatusCode(StatusCodes.Status304NotModified);
            }
        }

        return Ok(cachedData.Data);
    }

    private static string GenerateETag<T>(T payload)
    {
        string json = JsonSerializer.Serialize(payload);
        using var md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(json));
        return Convert.ToBase64String(hash);
    }

    private Guid GetUserId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var val) && Guid.TryParse(val, out var parsed))
        {
            return parsed;
        }
        return Guid.Parse("11111111-1111-1111-1111-111111111111"); // Usuário Padrão Local-First MVP
    }

    private record CachedPayload<T>(T Data, string ETag);
}