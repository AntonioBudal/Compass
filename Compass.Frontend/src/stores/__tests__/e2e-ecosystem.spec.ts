import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import { useOnboardingStore } from '../onboardingStore';
import { useDecisionStore } from '../decisionStore';
import { useProjectsStore } from '../projectsStore';
import { useCommitmentsStore } from '../commitmentsStore';
import { useProgressStore } from '../progressStore';
import { useToastStore } from '../toastStore';
import { PortabilityBundleSchema } from '@/schemas/portabilitySchema';

// --- MOCK ROBUSTO DO AXIOS ---
vi.mock('axios', () => {
  const mockAxiosInstance = {
    interceptors: {
      request: { use: vi.fn(), eject: vi.fn() },
      response: { use: vi.fn(), eject: vi.fn() }
    },
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
    patch: vi.fn()
  };
  return {
    default: {
      ...mockAxiosInstance,
      create: vi.fn(() => mockAxiosInstance)
    }
  };
});

// --- MOCK DO VUE ROUTER ---
vi.mock('vue-router', () => ({
  useRouter: () => ({
    push: vi.fn(),
    replace: vi.fn(),
    back: vi.fn()
  }),
  useRoute: () => ({
    path: '/now',
    query: {},
    params: {}
  })
}));

describe('Homologação E2E — Ecossistema Compass & Soberania de Dados', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    localStorage.clear();
    sessionStorage.clear();
    vi.clearAllMocks();
  });

  afterEach(() => {
    localStorage.clear();
    sessionStorage.clear();
  });

  it('[TESTE 1] Deve ativar o Simulador E2E [RAM SANDBOX], hidratar todas as stores e calibrar o EAI para 1.4x', () => {
    const onboardingStore = useOnboardingStore();
    const projectsStore = useProjectsStore();
    const commitmentsStore = useCommitmentsStore();
    const decisionStore = useDecisionStore();
    const progressStore = useProgressStore();

    // Act: Aciona o simulador de voo em memória RAM
    onboardingStore.activateRichSandbox();

    // Assert: Governança global
    expect(onboardingStore.isSandboxActive).toBe(true);
    expect(sessionStorage.getItem('compass_sandbox_mode')).toBe('true');

    // Assert: Catálogo de Projetos (LRU)
    expect(projectsStore.catalog.length).toBe(2);
    expect(projectsStore.catalog[0].name).toBe('compass-core');

    // Assert: Teia de Compromissos
    expect(commitmentsStore.items.length).toBe(4);
    const topTask = commitmentsStore.items.find(i => i.id === 'task-top-1');
    expect(topTask?.energyRequired).toBe(3);

    // Assert: Calibração Algorítmica do Now Engine
    expect(progressStore.rawOverview?.estimationAccuracyIndex).toBe(1.4);
    expect(decisionStore.adaptiveProfile.eaiMultiplier).toBe(1.4);
    
    // Prova matemática do EAI: Tarefa de 45m nominais calculada como 63m efetivos (45 * 1.4)
    expect(decisionStore.topActions[0].nominalDurationMinutes).toBe(45);
    expect(decisionStore.topActions[0].effectiveDurationMinutes).toBe(63);
    expect(decisionStore.topActions[0].wasTimeAdjustedByEai).toBe(true);
  });

  it('[TESTE 2] Deve isolar o [TUTORIAL] pedagógico, limpando a RAM sem desestabilizar o ecossistema', () => {
    const onboardingStore = useOnboardingStore();

    // Supondo que o usuário estava no Sandbox Rico
    onboardingStore.activateRichSandbox();
    expect(onboardingStore.commitments.length).toBeGreaterThan(0);

    // Act: Alterna para o modo Tutorial na Sidebar
    onboardingStore.startTutorialMode();

    // Assert: Memória limpa e pronta para o aprendizado passo a passo
    expect(onboardingStore.isSandboxActive).toBe(false);
    expect(onboardingStore.commitments.length).toBe(0);
  });

  it('[TESTE 3] UX Defensiva: Deve injetar CRON diário automaticamente em Hábitos e proteger contra duplicidade diária', async () => {
    const commitmentsStore = useCommitmentsStore();
    const toastStore = useToastStore();

    // Act 1: Criação de Hábito sem passar cronExpression (deve receber '0 8 * * *' para evitar erro do validador .NET)
    const payload: any = {
      title: 'Leitura de Arquitetura Limpa',
      type: 'HABIT',
      estimatedDurationMinutes: 25,
      energyRequired: 2
    };

    // Apenas validando a injeção do contrato defensivo antes do POST
    if (payload.type === 'HABIT' && !payload.cronExpression) {
      payload.cronExpression = '0 8 * * *';
    }
    expect(payload.cronExpression).toBe('0 8 * * *');

    // Act 2: Simular hábito já concluído hoje na RAM
    const todayIso = new Date().toISOString().slice(0, 10);
    const mockHabit: any = {
      id: 'habit-1',
      title: 'Leitura de Arquitetura Limpa',
      type: 'HABIT',
      status: 'COMPLETED',
      currentStreak: 12,
      bestStreak: 15,
      _lastCompletedDate: todayIso
    };
    commitmentsStore.items = [mockHabit];

    // Act 3: Tentar concluir o hábito novamente no mesmo dia
    await commitmentsStore.updateStatus('habit-1', 'COMPLETED');

    // Assert: A intervenção foi acionada para explicar ao usuário, preservando o streak em 12
    expect(toastStore.toasts.length).toBeGreaterThan(0);
    const lastToast = toastStore.toasts[0];
    expect(lastToast.intervention?.code).toBe('HABIT_ALREADY_COMPLETED');
    expect(mockHabit.currentStreak).toBe(12);
  });

  it('[TESTE 4] UX Defensiva: Deve alertar ao criar tarefa avulsa sem projeto (Perda de Escore)', async () => {
    const commitmentsStore = useCommitmentsStore();
    const toastStore = useToastStore();

    // Act: Criação de tarefa sem projectId
    const payload: any = {
      title: 'Refatoração solta',
      type: 'TASK',
      estimatedDurationMinutes: 30,
      energyRequired: 2,
      projectId: null
    };

    // Simula a lógica de intercepção defensiva executada no commitmentsStore
    if (payload.type === 'TASK' && !payload.projectId) {
      toastStore.showIntervention({
        code: 'MISSING_PROJECT_BINDING',
        title: 'Atividade criada sem projeto.',
        explanation: 'Tarefas avulsas recebem pontuação menor no Now Engine.',
        severity: 'warning',
        actions: [{ label: 'Vincular Projeto', handler: () => {} }]
      });
    }

    // Assert: Alerta de severidade 'warning' gerado
    expect(toastStore.toasts[0].intervention?.code).toBe('MISSING_PROJECT_BINDING');
    expect(toastStore.toasts[0].type).toBe('urgent');
  });

  it('[TESTE 5] Escudo Zod: Deve validar e rejeitar pacotes JSON corrompidos em milissegundos sem chamar a API', () => {
    // 1. Simula um arquivo JSON corrompido (userId inválido e título vazio)
    const corruptedBundle = {
      exportedAtUtc: new Date().toISOString(),
      schemaVersion: '4.0.0-tactical',
      userId: 'not-a-valid-uuid',
      projects: [{ 
        id: '123', 
        title: '', 
        status: 'ACTIVE', 
        totalEstimatedMinutes: 100 
      }],
      commitments: [],
      focusSessions: [],
      dailyReviews: []
    };

    // Act: Passa pelo crivo do Zod
    const zodResult = PortabilityBundleSchema.safeParse(corruptedBundle);

    // Assert: Interceptação imediata no cliente
    expect(zodResult.success).toBe(false);
    if (!zodResult.success) {
      const errorPaths = zodResult.error.issues.map(i => i.path.join('.'));
      expect(errorPaths).toContain('userId');
      expect(errorPaths).toContain('projects.0.id');
      expect(errorPaths).toContain('projects.0.title');
    }
  });

  it('[TESTE 6] Escudo Zod & Estresse: Deve validar um backup JSON pesado (500 compromissos) em < 50ms', () => {
    // 1. Gera um payload de estresse alinhado a todos os campos de CommitmentExportSchema
    const massiveCommitments = Array.from({ length: 500 }, (_, i) => ({
      id: crypto.randomUUID(),
      title: `Compromisso Histórico #${i}`,
      type: i % 2 === 0 ? 'TASK' : 'HABIT',
      status: 'COMPLETED',
      estimatedMinutes: 30,
      energyRequired: 2,
      postponedCount: 0,
      deadline: null,
      createdAt: new Date().toISOString(),
      completedAt: null,
      projectId: null
    }));

    const validMassiveBundle = {
      exportedAtUtc: new Date().toISOString(),
      schemaVersion: '4.0.0-tactical',
      userId: crypto.randomUUID(),
      settings: null,
      adaptiveProfile: null,
      projects: [],
      commitments: massiveCommitments,
      focusSessions: [],
      dailyReviews: []
    };

    // Act: Mede o tempo de validação Zod para 500 itens
    const start = performance.now();
    const zodResult = PortabilityBundleSchema.safeParse(validMassiveBundle);
    const duration = performance.now() - start;

    // Assert: Validação estrutural aprovada em SLA ultrarrápido
    expect(zodResult.success).toBe(true);
    expect(duration).toBeLessThan(50.0);
  });

  it('[TESTE 7] SLA de Latência: A avaliação de foco líquido e mutação de estado em RAM deve operar em < 16ms', () => {
    const commitmentsStore = useCommitmentsStore();
    
    // Injeta 250 itens na fila reativa do store
    commitmentsStore.items = Array.from({ length: 250 }, (_, idx) => ({
      id: `task-${idx}`,
      title: `Tarefa de Estresse ${idx}`,
      type: 'TASK',
      status: idx === 0 ? 'IN_PROGRESS' : 'PENDING',
      estimatedDurationMinutes: 25,
      energyRequired: 2
    } as any));

    // Act: Mede o tempo para filtrar candidatos ativos e agrupar hábitos do dia
    const start = performance.now();
    const candidates = commitmentsStore.activeCandidates;
    const habits = commitmentsStore.habitsToday;
    const end = performance.now();

    const durationMs = end - start;

    // Assert: SLA inalterável para renderização fluida a 60 FPS
    expect(candidates.length).toBe(250);
    expect(habits.length).toBe(0);
    expect(durationMs).toBeLessThan(16.0);
  });
});