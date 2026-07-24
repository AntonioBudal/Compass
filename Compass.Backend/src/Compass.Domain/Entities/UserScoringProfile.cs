using Compass.Domain.Exceptions;

namespace Compass.Domain.Entities;

public class UserScoringProfile
{
    public Guid UserId { get; private set; }
    public double EaiMultiplier { get; private set; }
    public double MorningEnergyBias { get; private set; }
    public double AfternoonEnergyBias { get; private set; }
    public double EveningEnergyBias { get; private set; }
    public double UrgencyWeightAdjust { get; private set; }
    public double StrategyWeightAdjust { get; private set; }
    public int SampleCount { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Construtor protegido para hidratação pelo EF Core
    protected UserScoringProfile() { }

    public UserScoringProfile(
        Guid userId,
        double eaiMultiplier = 1.0,
        double morningEnergyBias = 1.0,
        double afternoonEnergyBias = 1.0,
        double eveningEnergyBias = 1.0,
        double urgencyWeightAdjust = 0.0,
        double strategyWeightAdjust = 0.0,
        int sampleCount = 0)
    {
        ValidateAndSet(
            eaiMultiplier, 
            morningEnergyBias, 
            afternoonEnergyBias, 
            eveningEnergyBias, 
            urgencyWeightAdjust, 
            strategyWeightAdjust, 
            sampleCount);

        UserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Objeto Nulo (Cold Start Pattern): Retorna um perfil neutro imutável 
    /// para usuários novos com menos de 10 amostras no ecossistema.
    /// </summary>
    public static UserScoringProfile Default(Guid userId) => new(userId, 1.0, 1.0, 1.0, 1.0, 0.0, 0.0, 0);

    public void UpdateCalibration(
        double eaiMultiplier,
        double morningEnergyBias,
        double afternoonEnergyBias,
        double eveningEnergyBias,
        double urgencyWeightAdjust,
        double strategyWeightAdjust,
        int sampleCount)
    {
        ValidateAndSet(
            eaiMultiplier, 
            morningEnergyBias, 
            afternoonEnergyBias, 
            eveningEnergyBias, 
            urgencyWeightAdjust, 
            strategyWeightAdjust, 
            sampleCount);

        UpdatedAt = DateTime.UtcNow;
    }

    private void ValidateAndSet(
        double eai, 
        double morning, 
        double afternoon, 
        double evening, 
        double urgency, 
        double strategy, 
        int samples)
    {
        // Proteção anti-poisoning: Winsorização estrita entre 0.25x e 3.0x
        if (eai < 0.25 || eai > 3.0)
            throw new DomainException("O multiplicador de EAI deve estar no limite seguro entre 0.25x e 3.0x.");

        if (morning < 0.5 || morning > 1.5 || afternoon < 0.5 || afternoon > 1.5 || evening < 0.5 || evening > 1.5)
            throw new DomainException("Os vieses cronobiológicos de energia devem estar entre 0.5x e 1.5x.");

        if (urgency < -0.20 || urgency > 0.25 || strategy < -0.15 || strategy > 0.30)
            throw new DomainException("Os ajustes dinâmicos de peso excederam a banda permitida de calibração.");

        if (samples < 0)
            throw new DomainException("A contagem de amostras não pode ser negativa.");

        EaiMultiplier = Math.Round(eai, 2);
        MorningEnergyBias = Math.Round(morning, 2);
        AfternoonEnergyBias = Math.Round(afternoon, 2);
        EveningEnergyBias = Math.Round(evening, 2);
        UrgencyWeightAdjust = Math.Round(urgency, 2);
        StrategyWeightAdjust = Math.Round(strategy, 2);
        SampleCount = samples;
    }
}