using Compass.Domain.Entities;

namespace Compass.Application.Interfaces;

public interface IUserBehaviorProfilerService
{
    /// <summary>
    /// Agrega o histórico dos últimos 30 dias, calcula estatísticas com proteção contra outliers
    /// e retorna a entidade de perfil calibrada em < 30ms.
    /// </summary>
    Task<UserScoringProfile> CalculateProfileAsync(
        Guid userId, 
        string timeZoneId = "America/Sao_Paulo", 
        CancellationToken cancellationToken = default);
}