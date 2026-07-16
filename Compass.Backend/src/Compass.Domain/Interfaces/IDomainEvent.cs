namespace Compass.Domain.Interfaces;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}