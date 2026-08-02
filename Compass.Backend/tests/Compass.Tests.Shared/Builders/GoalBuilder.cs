using Compass.Domain.Entities;

namespace Compass.Tests.Shared.Builders;

public class GoalBuilder
{
    private Guid _userId = TestConstants.DefaultUserId;
    private string _title = "Meta Estratégica Padrão";
    private string? _whyDescription = "Descrição gerada pelo Builder de Testes";
    private DateTime? _targetDate = DateTime.UtcNow.AddMonths(3);
    private decimal _progress = 0m;

    public GoalBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public GoalBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public GoalBuilder WithProgress(decimal progress)
    {
        _progress = progress;
        return this;
    }

    public Goal Build()
    {
        // Respeita o construtor blindado do DDD
        var goal = new Goal(_userId, _title, _whyDescription, _targetDate);
        
        if (_progress > 0)
        {
            goal.UpdateProgress(_progress); // Usa o mutador correto
        }

        return goal;
    }
}