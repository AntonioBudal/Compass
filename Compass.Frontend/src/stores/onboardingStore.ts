import { defineStore } from 'pinia';
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useToastStore } from '@/stores/toastStore';
import { useProjectsStore } from '@/stores/projectsStore';
import { useCommitmentsStore, type CommitmentItem } from '@/stores/commitmentsStore';
import { useProgressStore } from '@/stores/progressStore';
import { useDecisionStore } from '@/stores/decisionStore';

export interface SandboxCommitment {
  id: string;
  title: string;
  type: 'TASK' | 'EVENT' | 'HABIT' | 'NOTE';
  status: 'pending' | 'completed' | 'postponed';
  estimatedDurationMinutes: number;
  energyRequired: number;
  timeLabel?: string;
}

export const useOnboardingStore = defineStore('onboarding', () => {
  const router = useRouter();
  const toastStore = useToastStore();
  
  const isSandboxActive = ref(false);
  const commitments = ref<SandboxCommitment[]>([]);

  const startTutorialMode = () => {
    isSandboxActive.value = false;
    commitments.value = []; 
  };

  // --- MOTOR DE INJEÇÃO GLOBAL: ECOSSISTEMA DEMO EM RAM (< 5ms) ---
  const activateRichSandbox = () => {
    // Instancia as stores globais em tempo de execução
    const projectsStore = useProjectsStore();
    const commitmentsStore = useCommitmentsStore();
    const progressStore = useProgressStore();
    const decisionStore = useDecisionStore();

    isSandboxActive.value = true;
    sessionStorage.setItem('compass_sandbox_mode', 'true');

    // 1. Injeção de Projetos e Metas (Normalizada)
    const p1 = { id: 'proj-core-101', name: 'compass-core', description: 'Migração do Motor Tático', lastUsedAtUtc: new Date().toISOString() };
    const p2 = { id: 'proj-arch-202', name: 'local-first-book', description: 'Arquiteturas resilientes', lastUsedAtUtc: new Date(Date.now() - 86400000 * 2).toISOString() };
    
    //  ARQ: Injeção Segura na Fonte de Verdade
    projectsStore.entities[p1.id] = p1;
    projectsStore.entities[p2.id] = p2;
    // Precisamos de bypass no TS porque as refs de ID e Catalog são internas na arquitetura do setup store
    // Como é apenas Sandbox de memória, podemos usar assign forçado.
    (projectsStore as any).catalogIds = [p1.id, p2.id];
    projectsStore.isServingFromCache = true;

    // 2. Injeção da Teia de Compromissos (Tasks, Habits, Events)
    const now = new Date();
    const eventStart = new Date(now.setHours(14, 0, 0, 0)).toISOString();
    const eventEnd = new Date(now.setHours(14, 45, 0, 0)).toISOString();

    const mockCommitments: CommitmentItem[] = [
      {
        id: 'task-top-1',
        title: 'Refatorar pipeline assíncrono do Now Engine (@45m !3 #compass-core)',
        type: 'TASK',
        status: 'PENDING',
        estimatedDurationMinutes: 45,
        energyRequired: 3,
        postponedCount: 0,
        deadline: new Date(Date.now() + 86400000).toISOString(),
        startTime: null,
        endTime: null,
        locationOrLink: null,
        cronExpression: null,
        currentStreak: 0,
        bestStreak: 0,
        content: 'Otimizar o LINQ.',
        projectId: 'proj-core-101',
        projectName: 'compass-core',
        _isSyncing: false
      },
      {
        id: 'task-postponed-2',
        title: 'Escrever especificações de testes E2E no Vitest (@60m !2 #local-first-book)',
        type: 'TASK',
        status: 'PENDING',
        estimatedDurationMinutes: 60,
        energyRequired: 2,
        postponedCount: 3, 
        deadline: new Date(Date.now() - 3600000).toISOString(), 
        startTime: null,
        endTime: null,
        locationOrLink: null,
        cronExpression: null,
        currentStreak: 0,
        bestStreak: 0,
        content: null,
        projectId: 'proj-arch-202',
        projectName: 'local-first-book',
        _isSyncing: false
      },
      {
        id: 'habit-streak-3',
        title: 'Revisão Tática Matinal (Zero-Mouse) (@15m !2)',
        type: 'HABIT',
        status: 'PENDING',
        estimatedDurationMinutes: 15,
        energyRequired: 2,
        postponedCount: 0,
        deadline: null,
        startTime: null,
        endTime: null,
        locationOrLink: null,
        cronExpression: '0 8 * * *',
        currentStreak: 12, 
        bestStreak: 21,
        content: 'Verificar Hard Blockers e calibrar a energia.',
        projectId: null,
        projectName: null,
        _isSyncing: false
      },
      {
        id: 'event-calendar-4',
        title: 'Alinhamento de Arquitetura Local-First com Time (@45m !2)',
        type: 'EVENT',
        status: 'PENDING',
        estimatedDurationMinutes: 45,
        energyRequired: 2,
        postponedCount: 0,
        deadline: null,
        startTime: eventStart,
        endTime: eventEnd,
        locationOrLink: 'https://meet.compass.dev/arch',
        cronExpression: null,
        currentStreak: 0,
        bestStreak: 0,
        content: 'Discutir concorrência multi-aba.',
        projectId: 'proj-core-101',
        projectName: 'compass-core',
        _isSyncing: false
      }
    ];

    //  ARQ: Injeção Normalizada
    const activeIds: string[] = [];
    mockCommitments.forEach(c => {
      commitmentsStore.entities[c.id] = c;
      activeIds.push(c.id);
    });
    // O mesmo bypass seguro para RAM Sandbox
    (commitmentsStore as any).activeIds = activeIds;

    // 3. Injeção de Telemetria e Perfil Adaptativo
    if ((progressStore as any).rawOverview !== undefined) {
      (progressStore as any).rawOverview = {
        totalCompleted: 42,
        totalPlanned: 50,
        completionRatePercentage: 84.0,
        estimationAccuracyIndex: 1.4, 
        hasImputedAccuracyData: true,
        totalDeepWorkMinutes: 320,
        totalUsefulMinutes: 580,
        totalPostponements: 6,
        periodStartDateUtc: new Date(Date.now() - 86400000 * 30).toISOString(),
        periodEndDateUtc: new Date().toISOString()
      };
      (progressStore as any).isServingFromCache = true;
    }

    // 4. Calibração do Now Engine
    if ((decisionStore as any).adaptiveProfile !== undefined) {
      (decisionStore as any).adaptiveProfile = {
        isCalibrated: true,
        sampleCount: 18,
        eaiMultiplier: 1.4,
        morningEnergyBias: 1.25, 
        afternoonEnergyBias: 0.85,
        eveningEnergyBias: 0.70
      };
      
      // Projeta o Top Focus aplicando o EAI de 1.4x sobre os 45m nominais
      (decisionStore as any).rawTopActions = [
        {
          commitmentId: 'task-top-1',
          title: mockCommitments[0].title,
          type: 'TASK',
          nominalDurationMinutes: 45,
          effectiveDurationMinutes: 63, 
          energyRequired: 3,
          scorePercentage: 96.5,
          reason: 'Alta compatibilidade com seu pico cronobiológico matinal e projeto prioritário.',
          wasTimeAdjustedByEai: true,
          projectName: 'compass-core'
        }
      ];
      (decisionStore as any).isServingFromCache = true;
    }

    toastStore.showToast('[RAM SANDBOX] Ecossistema analítico carregado na memória!', 'success');
    router.push('/now');
  };

  // --- ENCERRAMENTO DO SANDBOX E RESTAURAÇÃO DE REDE ---
  const finishOnboarding = () => {
    isSandboxActive.value = false;
    commitments.value = [];
    sessionStorage.removeItem('compass_sandbox_mode');
    
    try {
      localStorage.setItem('compass_onboarded', 'true');
    } catch (e) {
      console.warn('[SandboxStore]: Falha ao persistir flag de onboarding.', e);
    }

    window.dispatchEvent(new Event('compass:boot-sequence'));
    router.push('/now');
    toastStore.showToast('Inicialização Completa. Banco de dados local ativado!', 'success');
  };

  const skipOnboarding = () => finishOnboarding();

  // Mutações isoladas da etapa inicial do tutorial
  const seedSandboxData = () => {
    commitments.value = [
      { id: 'box-1', title: 'Explorar a interface monocromática do Compass', type: 'TASK', status: 'pending', estimatedDurationMinutes: 15, energyRequired: 1 },
      { id: 'box-2', title: 'Alinhamento de Arquitetura Local-First', type: 'EVENT', status: 'pending', estimatedDurationMinutes: 45, energyRequired: 3, timeLabel: '14:00 - 14:45' },
      { id: 'box-3', title: 'Beber 500ml de água e alongar a coluna', type: 'HABIT', status: 'pending', estimatedDurationMinutes: 5, energyRequired: 1 }
    ];
    isSandboxActive.value = true;
  };

  const toggleComplete = (id: string) => {
    const item = commitments.value.find(c => c.id === id);
    if (item) {
      item.status = item.status === 'completed' ? 'pending' : 'completed';
      toastStore.showToast(item.status === 'completed' ? 'Item concluído (RAM)!' : 'Item restaurado (RAM).', item.status === 'completed' ? 'success' : 'neutral');
    }
  };

  const postponeItem = (id: string) => {
    const item = commitments.value.find(c => c.id === id);
    if (item) {
      item.status = 'postponed';
      toastStore.showToast('Compromisso adiado para o próximo turno.', 'urgent');
    }
  };

  const addSandboxItem = (title: string, type: 'TASK' | 'EVENT' | 'HABIT' | 'NOTE' = 'TASK') => {
    commitments.value.unshift({ id: 'box-' + Math.random().toString(36).substring(2, 7), title, type, status: 'pending', estimatedDurationMinutes: 25, energyRequired: 2 });
    toastStore.showToast('Novo item simulado adicionado à memória.', 'success');
  };

  return {
    isSandboxActive,
    commitments,
    activateRichSandbox,
    seedSandboxData,
    toggleComplete,
    postponeItem,
    addSandboxItem,
    finishOnboarding,
    skipOnboarding,
    startTutorialMode
  };
});