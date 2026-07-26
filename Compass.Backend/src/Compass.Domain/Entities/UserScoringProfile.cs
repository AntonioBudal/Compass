using Compass.Domain.Exceptions;

namespace Compass.Domain.Entities;

public class UserScoringProfile
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    
    // Contagem de ações concluídas analisadas para validar calibração (> 10 amostras)
    public int SampleCount { get; private set; }
    
    // Ajustes Aditivos de Pesos (Delta sobre a base)
    public double UrgencyWeightAdjust { get; private set; }
    public double StrategyWeightAdjust { get; private set; }
    public double EnergyAlignmentWeight { get; private set; }
    public double PostponementPenaltyWeight { get; private set; }
    
    // Multiplicador do Índice de Acurácia de Estimativa (EAI)
    public double EaiMultiplier { get; private set; }
    
    // Vieses Cronobiológicos por Turno (1.0 = Neutro; > 1.0 = Afinidade alta)
    public double MorningEnergyBias { get; private set; }
    public double AfternoonEnergyBias { get; private set; }
    public double EveningEnergyBias { get; private set; }
    public double NightEnergyBias { get; private set; }
    
    public DateTime UpdatedAt { get; private set; }
    
    // Token de concorrência nativo do PostgreSQL (xmin)
    public uint Version { get; private set; }

    protected UserScoringProfile() { }

    public UserScoringProfile(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        SampleCount = 0;
        
        // Inicialização neutra (Deltas em 0.0 e Multiplicadores em 1.0)
        UrgencyWeightAdjust = 0.0;
        StrategyWeightAdjust = 0.0;
        EnergyAlignmentWeight = 1.0;
        PostponementPenaltyWeight = 1.0;
        EaiMultiplier = 1.0;
        
        MorningEnergyBias = 1.0;
        AfternoonEnergyBias = 1.0;
        EveningEnergyBias = 1.0;
        NightEnergyBias = 1.0;
        
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Fábrica estática para o Padrão Objeto Nulo (Null Object Pattern).
    /// Retorna um perfil basal neutro em memória quando o usuário ainda não tem calibração suficiente.
    /// </summary>
    public static UserScoringProfile Default(Guid userId) => new(userId);

    /// <summary>
    /// Atualiza as estatísticas e os deltas do perfil com blindagem de limites (Clamping).
    /// </summary>
    public void UpdateProfile(
        int sampleCount,
        double urgencyAdjust, 
        double strategyAdjust, 
        double energyWeight, 
        double penaltyWeight, 
        double eaiMultiplier)
    {
        SampleCount = Math.Max(0, sampleCount);
        UrgencyWeightAdjust = Clamp(urgencyAdjust, -0.20, 0.25);
        StrategyWeightAdjust = Clamp(strategyAdjust, -0.10, 0.30);
        EnergyAlignmentWeight = Clamp(energyWeight, 0.1, 3.0);
        PostponementPenaltyWeight = Clamp(penaltyWeight, 0.1, 3.0);
        EaiMultiplier = Clamp(eaiMultiplier, 0.5, 2.0);
        
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateChronologyBiases(double morning, double afternoon, double evening, double night)
    {
        MorningEnergyBias = Clamp(morning, 0.5, 2.0);
        AfternoonEnergyBias = Clamp(afternoon, 0.5, 2.0);
        EveningEnergyBias = Clamp(evening, 0.5, 2.0);
        NightEnergyBias = Clamp(night, 0.5, 2.0);
        
        UpdatedAt = DateTime.UtcNow;
    }

    private static double Clamp(double val, double min, double max)
    {
        if (double.IsNaN(val) || double.IsInfinity(val)) return 0.0;
        return Math.Max(min, Math.Min(max, val));
    }
}