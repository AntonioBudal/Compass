using Compass.Domain.Exceptions;

namespace Compass.Domain.Entities;

public class DailyReview
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    
    // Mapeado para o tipo nativo 'date' no PostgreSQL
    public DateOnly ReviewDate { get; private set; }
    
    public int CompletedCount { get; private set; }
    public int PostponedCount { get; private set; }
    public int TotalFocusMinutes { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    protected DailyReview() { }

    public DailyReview(
        Guid userId, 
        DateOnly reviewDate, 
        int completedCount, 
        int postponedCount, 
        int totalFocusMinutes, 
        string notes = "")
    {
        if (completedCount < 0 || postponedCount < 0 || totalFocusMinutes < 0)
            throw new DomainException("As métricas da revisão diária não podem conter valores negativos.");

        Id = Guid.NewGuid();
        UserId = userId;
        ReviewDate = reviewDate;
        CompletedCount = completedCount;
        PostponedCount = postponedCount;
        TotalFocusMinutes = totalFocusMinutes;
        Notes = notes.Trim();
        CreatedAt = DateTime.UtcNow;
    }
}