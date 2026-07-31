using Compass.Application.DTOs;
using Compass.Application.Interfaces;
using Compass.Domain.Exceptions;
using Compass.Domain.Interfaces;

namespace Compass.Application.Services;

public class GoalService : IGoalService
{
    private readonly IGoalRepository _goalRepo;

    public GoalService(IGoalRepository goalRepo)
    {
        _goalRepo = goalRepo;
    }

    public async Task<object> UpdateAsync(Guid userId, Guid goalId, UpdateGoalDto dto, CancellationToken cancellationToken = default)
    {
        var goal = await _goalRepo.GetByIdAsync(goalId, cancellationToken)
            ?? throw new DomainException("Meta não encontrada.");

        if (goal.UserId != userId)
            throw new DomainException("Acesso negado a esta meta.");

        // Aciona a mutação protegida no Domínio
        goal.UpdateGoalDetails(dto.Title, dto.WhyDescription, dto.TargetDate);

        _goalRepo.Update(goal);
        await _goalRepo.SaveChangesAsync(cancellationToken);

        // Retorna o DTO de projeção
        return new 
        {
            Id = goal.Id,
            Title = goal.Title,
            WhyDescription = goal.WhyDescription,
            TargetDate = goal.TargetDate,
            Status = goal.Status.ToString().ToUpperInvariant(),
            ProgressPercentage = goal.ProgressPercentage
        };
    }
}