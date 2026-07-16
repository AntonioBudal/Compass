using Compass.Domain.Exceptions;

namespace Compass.Domain.Entities;

public class Reminder
{
    public Guid Id { get; private set; }
    public Guid CommitmentId { get; private set; }
    public DateTime TriggerTime { get; private set; }
    public bool IsSent { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected Reminder() { }

    public Reminder(Guid commitmentId, DateTime triggerTime)
    {
        Id = Guid.NewGuid();
        CommitmentId = commitmentId;
        TriggerTime = triggerTime;
        IsSent = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void Reschedule(DateTime newTriggerTime)
    {
        if (IsSent)
            throw new DomainException("Não é possível reagendar um lembrete que já foi disparado.");

        TriggerTime = newTriggerTime;
    }

    public void MarkAsSent()
    {
        IsSent = true;
    }
}