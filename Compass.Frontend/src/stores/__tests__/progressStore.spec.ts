import { describe, it, expect, beforeEach, vi } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import axios from 'axios';

// CORRIGIDO: Mock estruturado para suportar axios.create() e interceptores do api.ts
vi.mock('axios', () => {
  const mockInstance = {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
    interceptors: {
      request: { use: vi.fn(), eject: vi.fn() },
      response: { use: vi.fn(), eject: vi.fn() }
    }
  };
  return {
    default: {
      ...mockInstance,
      create: vi.fn(() => mockInstance)
    },
    ...mockInstance,
    create: vi.fn(() => mockInstance)
  };
});

// Importamos as stores DEPOIS da declaração do mock para garantir o içamento limpo do Vitest
import { useProgressStore } from '../progressStore';
import { useCommitmentsStore } from '../commitmentsStore';

const mockedAxios = vi.mocked(axios, true);

describe('ProgressStore — Motor de Fusão Analítica & Resiliência Offline', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    localStorage.clear();
    vi.clearAllMocks();
  });

  it('deve inicializar com valores vazios e carregar dados da API com sucesso', async () => {
    const store = useProgressStore();

    // Mock das chamadas simultâneas da API
    mockedAxios.get.mockImplementation(async (url) => {
      if (url.includes('/overview')) {
        return {
          status: 200,
          headers: { etag: 'W/"hash-teste"' },
          data: {
            totalCompleted: 10,
            totalPlanned: 12,
            completionRatePercentage: 83.3,
            estimationAccuracyIndex: 1.1,
            hasImputedAccuracyData: false,
            totalDeepWorkMinutes: 120,
            totalUsefulMinutes: 300,
            totalPostponements: 2,
            periodStartDateUtc: '2026-06-24T00:00:00Z',
            periodEndDateUtc: '2026-07-23T23:59:59Z'
          }
        };
      }
      return { status: 200, data: [] };
    });

    await store.fetchProgress('30d');

    expect(store.isLoading).toBe(false);
    expect(store.isServingFromCache).toBe(false);
    expect(store.rawOverview?.totalCompleted).toBe(10);
    expect(localStorage.getItem('compass_progress_overview_30d')).toBeDefined();
  });

  it('deve fundir (Merge Engine) o histórico de ontem com as tarefas concluídas HOJE em RAM', async () => {
    const progressStore = useProgressStore();
    const commitmentsStore = useCommitmentsStore();

    // 1. Injeta estado histórico simulado (Até Ontem: 10 concluídas)
    progressStore.rawOverview = {
      totalCompleted: 10,
      totalPlanned: 10,
      completionRatePercentage: 100,
      estimationAccuracyIndex: 1.0,
      hasImputedAccuracyData: false,
      totalDeepWorkMinutes: 60,
      totalUsefulMinutes: 300,
      totalPostponements: 0,
      periodStartDateUtc: '2026-06-01T00:00:00Z',
      periodEndDateUtc: '2026-07-23T23:59:59Z'
    };

    // 2. Injeta na RAM do cliente 2 tarefas concluídas HOJE (Delta de Hoje)
    commitmentsStore.items = [
      {
        id: 'hoje-1',
        title: 'Tarefa Deep Work de Hoje',
        type: 'TASK',
        status: 'COMPLETED',
        estimatedDurationMinutes: 45,
        energyRequired: 3, // Deep Work
        deadline: null, startTime: null, endTime: null, locationOrLink: null, cronExpression: null,
        currentStreak: 0, bestStreak: 0, postponedCount: 0, content: null, projectId: null, projectName: null
      },
      {
        id: 'hoje-2',
        title: 'Tarefa Operacional de Hoje',
        type: 'TASK',
        status: 'COMPLETED',
        estimatedDurationMinutes: 15,
        energyRequired: 2, // Operacional
        deadline: null, startTime: null, endTime: null, locationOrLink: null, cronExpression: null,
        currentStreak: 0, bestStreak: 0, postponedCount: 0, content: null, projectId: null, projectName: null
      }
    ];

    // 3. Acessa o getter computado fundido (mergedOverview)
    const merged = progressStore.mergedOverview;

    // 10 históricas + 2 hoje = 12 concluídas!
    expect(merged?.totalCompleted).toBe(12);
    // 300m históricos + 60m hoje (45 + 15) = 360m!
    expect(merged?.totalUsefulMinutes).toBe(360);
    // 60m históricos + 45m hoje (apenas a de energia 3) = 105m de Deep Work!
    expect(merged?.totalDeepWorkMinutes).toBe(105);
  });

  it('deve hidratar do localStorage (Offline Mode) quando a rede falhar', async () => {
    const store = useProgressStore();

    // 1. Prepara um cache no disco local simulando uma sessão anterior
    const cachedOverview = {
      totalCompleted: 25,
      totalPlanned: 30,
      completionRatePercentage: 83.3,
      estimationAccuracyIndex: 1.05,
      hasImputedAccuracyData: true,
      totalDeepWorkMinutes: 200,
      totalUsefulMinutes: 500,
      totalPostponements: 1,
      periodStartDateUtc: '2026-06-01T00:00:00Z',
      periodEndDateUtc: '2026-07-23T23:59:59Z'
    };
    localStorage.setItem('compass_progress_overview_7d', JSON.stringify(cachedOverview));
    localStorage.setItem('compass_progress_daily_7d', JSON.stringify([]));
    localStorage.setItem('compass_progress_heatmap_7d', JSON.stringify([]));
    localStorage.setItem('compass_progress_chronology_7d', JSON.stringify([]));

    // 2. Simula falha catastrófica de rede (Sem internet / Backend offline)
    mockedAxios.get.mockRejectedValue(new Error('ERR_NETWORK'));

    // 3. Executa a busca
    await store.fetchProgress('7d');

    // Assert: O sistema não quebrou, ativou a flag offline e serviu do disco!
    expect(store.isLoading).toBe(false);
    expect(store.isServingFromCache).toBe(true);
    expect(store.rawOverview?.totalCompleted).toBe(25);
  });
});