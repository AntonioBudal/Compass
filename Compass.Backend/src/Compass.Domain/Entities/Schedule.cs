using Compass.Domain.Exceptions;

namespace Compass.Domain.Entities;

public class Schedule
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public short DayOfWeek { get; private set; }
    public TimeOnly WorkStart { get; private set; }
    public TimeOnly WorkEnd { get; private set; }
    public bool IsActive { get; private set; }

    protected Schedule() { }

    public Schedule(Guid userId, short dayOfWeek, TimeOnly workStart, TimeOnly workEnd)
    {
        if (dayOfWeek < 0 || dayOfWeek > 6)
            throw new DomainException("O dia da semana deve estar entre 0 (Domingo) e 6 (Sábado).");

        if (workEnd <= workStart)
            throw new DomainException($"O horário de término ({workEnd}) deve ser posterior ao horário de início ({workStart}).");

        Id = Guid.NewGuid();
        UserId = userId;
        DayOfWeek = dayOfWeek;
        WorkStart = workStart;
        WorkEnd = workEnd;
        IsActive = true;
    }

    public void UpdateTimes(TimeOnly workStart, TimeOnly workEnd)
    {
        if (workEnd <= workStart)
            throw new DomainException("O horário de término deve ser posterior ao horário de início.");

        WorkStart = workStart;
        WorkEnd = workEnd;
    }

    public void ToggleActive(bool isActive)
    {
        IsActive = isActive;
    }
}