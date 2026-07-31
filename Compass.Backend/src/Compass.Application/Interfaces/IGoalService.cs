using Compass.Application.DTOs;

namespace Compass.Application.Interfaces;

public interface IGoalService
{
    // O retorno pode ser um object anônimo ou criar um GoalDto caso não exista
    Task<object> UpdateAsync(Guid userId, Guid goalId, UpdateGoalDto dto, CancellationToken cancellationToken = default);
}