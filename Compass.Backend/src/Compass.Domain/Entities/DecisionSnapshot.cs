using Compass.Domain.Exceptions;

namespace Compass.Domain.Entities;

public class DecisionSnapshot
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public int AvailableWindowMinutes { get; private set; }
    public short UserEnergyContext { get; private set; }
    public Guid? Top1Id { get; private set; }
    public Guid? Top2Id { get; private set; }
    public Guid? Top3Id { get; private set; }
    public Guid? ChosenCommitmentId { get; private set; }
    public bool WasIgnored { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected DecisionSnapshot() { }

    public DecisionSnapshot(
        Guid userId, 
        int availableWindowMinutes, 
        short userEnergyContext, 
        Guid? top1Id = null, 
        Guid? top2Id = null, 
        Guid? top3Id = null)
    {
        if (availableWindowMinutes < 0)
            throw new DomainException("A janela de tempo disponível não pode ser negativa.");

        if (userEnergyContext < 1 || userEnergyContext > 3)
            throw new DomainException("O contexto de energia deve estar entre 1 (Baixa) e 3 (Alta).");

        Id = Guid.NewGuid();
        UserId = userId;
        AvailableWindowMinutes = availableWindowMinutes;
        UserEnergyContext = userEnergyContext;
        Top1Id = top1Id;
        Top2Id = top2Id;
        Top3Id = top3Id;
        WasIgnored = true; // Nasce como ignorado até o usuário interagir
        CreatedAt = DateTime.UtcNow;
    }

    public void RegisterChoice(Guid chosenCommitmentId)
    {
        if (chosenCommitmentId != Top1Id && chosenCommitmentId != Top2Id && chosenCommitmentId != Top3Id)
            throw new DomainException("O compromisso escolhido deve ser uma das 3 opções sugeridas pelo motor.");

        ChosenCommitmentId = chosenCommitmentId;
        WasIgnored = false;
    }
}