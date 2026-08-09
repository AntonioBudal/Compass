using Compass.Domain.Enums;
using Compass.Domain.Exceptions;

namespace Compass.Domain.Entities;

public class TaskCommitment : Commitment
{
    public int EstimatedDurationMinutes { get; private set; }
    public short EnergyRequired { get; private set; }
    public DateTime? Deadline { get; private set; }

    public DateTime? StartTime { get; private set; }
    public int PostponedCount { get; private set; }

    protected TaskCommitment() { }

    public TaskCommitment(
        Guid userId, 
        string title, 
        int estimatedDurationMinutes = 30, 
        short energyRequired = 2, 
        Guid? projectId = null, 
        DateTime? deadline = null) 
        : base(userId, title, CommitmentType.Task, projectId)
    {
        if (estimatedDurationMinutes < 5)
            throw new DomainException("Toda tarefa executável deve possuir duração estimada maior ou igual a 5 minutos.");

        if (energyRequired < 1 || energyRequired > 3)
            throw new DomainException("A energia requerida deve estar entre 1 (Baixa) e 3 (Alta).");

        EstimatedDurationMinutes = estimatedDurationMinutes;
        EnergyRequired = energyRequired;
        Deadline = deadline;
        PostponedCount = 0;
    }

    public void StartFocus()
    {
        if (Status == CommitmentStatus.Archived || Status == CommitmentStatus.Completed)
            throw new DomainException("Não é possível iniciar foco em uma tarefa concluída ou arquivada.");

        Status = CommitmentStatus.InProgress;
    }

    public override void Complete()
    {
        if (Status == CommitmentStatus.Archived)
            throw new DomainException("Não é possível concluir uma tarefa arquivada.");

        Status = CommitmentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Block()
    {
        if (Status != CommitmentStatus.Completed && Status != CommitmentStatus.Archived)
        {
            Status = CommitmentStatus.Blocked;
        }
    }

    public void Unblock()
    {
        if (Status == CommitmentStatus.Blocked)
        {
            Status = CommitmentStatus.Pending;
        }
    }

    public void Reopen()
    {
        if (Status == CommitmentStatus.Completed)
        {
            Status = CommitmentStatus.Pending;
            CompletedAt = null;
        }
    }

    public void IncrementPostponed()
    {
        if (Status == CommitmentStatus.Pending)
        {
            PostponedCount++;
        }
    }

    public void UpdateTaskDetails(int duration, short energy, DateTime? deadline, DateTime? startTime)
    {
        if (Status == CommitmentStatus.Archived)
            throw new DomainException("Não é possível alterar os detalhes de uma tarefa arquivada.");
        
        if (duration < 5)
            throw new DomainException("Toda tarefa executável deve possuir duração estimada maior ou igual a 5 minutos.");
            
        if (energy < 1 || energy > 3)
            throw new DomainException("A energia requerida deve estar entre 1 (Baixa) e 3 (Alta).");

        EstimatedDurationMinutes = duration;
        EnergyRequired = energy;
        Deadline = deadline;
        StartTime = startTime; //  Salva o horário na memória da entidade
    }
}