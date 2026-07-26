import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import { useDecisionStore, type DecisionResponseDto } from '../decisionStore';
import axios from 'axios';

vi.mock('axios');
const mockedAxios = vi.mocked(axios, true);

describe('decisionStore — Now Engine & Resiliência Offline', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    localStorage.clear();
    vi.clearAllMocks();
  });

  afterEach(() => {
    localStorage.clear();
  });

  const mockApiResponse: DecisionResponseDto = {
    generatedAtUtc: '2026-07-26T18:00:00Z',
    availableWindowMinutes: 60,
    operatorEnergyLevel: 2,
    adaptiveProfile: {
      isCalibrated: true,
      sampleCount: 15,
      eaiMultiplier: 1.5,
      morningEnergyBias: 1.2,
      afternoonEnergyBias: 1.0,
      eveningEnergyBias: 0.8
    },
    topActions: [
      {
        commitmentId: 'id-1',
        title: 'Revisar PR de Arquitetura',
        type: 'TASK',
        nominalDurationMinutes: 30,
        effectiveDurationMinutes: 45, // Calibrado pelo EAI de 1.5x
        energyRequired: 2,
        scorePercentage: 94.5,
        reason: 'Alinhado com seu pico de energia e EAI calibrado.',
        wasTimeAdjustedByEai: true,
        projectName: 'Compass Core'
      }
    ]
  };

  it('deve hidratar o perfil adaptativo da API e persistir no localStorage com sucesso', async () => {
    mockedAxios.get.mockResolvedValueOnce({ status: 200, data: mockApiResponse });
    const store = useDecisionStore();

    await store.fetchDecisions(60, 2);

    expect(store.topActions.length).toBe(1);
    expect(store.adaptiveProfile.isCalibrated).toBe(true);
    expect(store.adaptiveProfile.eaiMultiplier).toBe(1.5);
    expect(store.primaryFocus?.title).toBe('Revisar PR de Arquitetura');
    expect(store.isServingFromCache).toBe(false);

    // Checa se gravou no disco para futura resiliência
    const diskCache = localStorage.getItem('compass_now_engine_cache_v3');
    expect(diskCache).not.toBeNull();
    expect(JSON.parse(diskCache!).profile.eaiMultiplier).toBe(1.5);
  });

  it('deve ativar o fallback do localStorage quando a API estiver offline sem quebrar a UI', async () => {
    // 1. Prepara o cache local no disco simulando uma sessão anterior
    const offlinePayload = {
      timestamp: new Date().toISOString(),
      profile: mockApiResponse.adaptiveProfile,
      actions: mockApiResponse.topActions,
      window: 45,
      energy: 1
    };
    localStorage.setItem('compass_now_engine_cache_v3', JSON.stringify(offlinePayload));

    // 2. Simula falha de rede (Offline / Timeout) no Axios
    mockedAxios.get.mockRejectedValueOnce(new Error('Network Error - Offline'));
    const store = useDecisionStore();

    // Act
    await store.fetchDecisions(45, 1);

    // Assert: O store deve ter absorvido a falha e carregado os dados da memória RAM/Disco
    expect(store.topActions.length).toBe(1);
    expect(store.primaryFocus?.title).toBe('Revisar PR de Arquitetura');
    expect(store.adaptiveProfile.eaiMultiplier).toBe(1.5);
    expect(store.isServingFromCache).toBe(true); // Flag de transição offline ativa!
  });

  it('deve manter o alias fetchNow funcional para retrocompatibilidade com componentes das Semanas 1 e 2', async () => {
    mockedAxios.get.mockResolvedValueOnce({ status: 200, data: mockApiResponse });
    const store = useDecisionStore();

    // Chama pelo nome antigo
    await store.fetchNow(60, 2);

    expect(store.topActions.length).toBe(1);
    expect(store.primaryFocus?.scorePercentage).toBe(94.5);
  });

  it('deve executar mutações e derivações em memória com latência inferior a 16ms (< 16ms)', () => {
    const store = useDecisionStore();
    
    // Injeta 100 tarefas simuladas para teste de estresse em getters
    const stressActions = Array.from({ length: 100 }, (_, idx) => ({
      ...mockApiResponse.topActions[0],
      commitmentId: `id-${idx}`,
      scorePercentage: 100 - idx
    }));

    const start = performance.now();
    store.topActions = stressActions;
    
    // Força a avaliação dos getters computados
    const primary = store.primaryFocus;
    const secondaries = store.secondaryActions;
    const end = performance.now();

    const durationMs = end - start;

    expect(primary?.commitmentId).toBe('id-0');
    expect(secondaries.length).toBe(99);
    expect(durationMs).toBeLessThan(16.0); // Garante SLA de renderização a 60 FPS!
  });
});