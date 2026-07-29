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
    // Se o sandbox rico estiver ativo, desliga momentaneamente para o tutorial
    isSandboxActive.value = false;
    commitments.value = []; // Limpa a RAM para o usuário testar passo a passo
  };

  // --- MOTOR DE INJEÇÃO GLOBAL: ECOSSISTEMA DEMO EM RAM (< 5ms) ---
  const activateRichSandbox = () => {
    // Instancia as stores globais em tempo de execução para evitar ciclos de dependência
    const projectsStore = useProjectsStore();
    const commitmentsStore = useCommitmentsStore();
    const progressStore = useProgressStore();
    const decisionStore = useDecisionStore();

    isSandboxActive.value = true;
    sessionStorage.setItem('compass_sandbox_mode', 'true');

    // 1. Injeção de Projetos e Metas (LRU Catalog)
    projectsStore.catalog = [
      {
        id: 'proj-core-101',
        name: 'compass-core',
        description: 'Migração do Motor Tático para .NET 10 & Vue 3',
        lastUsedAtUtc: new Date().toISOString()
      },
      {
        id: 'proj-arch-202',
        name: 'local-first-book',
        description: 'Pesquisa e escrita sobre arquiteturas resilientes offline',
        lastUsedAtUtc: new Date(Date.now() - 86400000 * 2).toISOString()
      }
    ];
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
        content: 'Otimizar o LINQ com proteções de Winsorização para evitar distorções no EAI.',
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
        postponedCount: 3, // Aciona alerta de procrastinação na UI
        deadline: new Date(Date.now() - 3600000).toISOString(), // Atrasada
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
        currentStreak: 12, // Sequência ativa visual 🔥
        bestStreak: 21,
        content: 'Verificar Hard Blockers e calibrar a energia do dia.',
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
        content: 'Discutir concorrência multi-aba com BroadcastChannel.',
        projectId: 'proj-core-101',
        projectName: 'compass-core',
        _isSyncing: false
      }
    ];

    commitmentsStore.items = mockCommitments;

    // 3. Injeção de Telemetria e Perfil Adaptativo (EAI = 1.4x)
    progressStore.rawOverview = {
      totalCompleted: 42,
      totalPlanned: 50,
      completionRatePercentage: 84.0,
      estimationAccuracyIndex: 1.4, // EAI Calibrado: Tarefas levam 40% mais tempo!
      hasImputedAccuracyData: true,
      totalDeepWorkMinutes: 320,
      totalUsefulMinutes: 580,
      totalPostponements: 6,
      periodStartDateUtc: new Date(Date.now() - 86400000 * 30).toISOString(),
      periodEndDateUtc: new Date().toISOString()
    };
    progressStore.isServingFromCache = true;

    // 4. Calibração do Now Engine (Refletindo na tela Agora)
    decisionStore.adaptiveProfile = {
      isCalibrated: true,
      sampleCount: 18,
      eaiMultiplier: 1.4,
      morningEnergyBias: 1.25, // Pico de energia matinal
      afternoonEnergyBias: 0.85,
      eveningEnergyBias: 0.70
    };
    
    // Projeta o Top Focus aplicando o EAI de 1.4x sobre os 45m nominais (45 * 1.4 = 63m)
    decisionStore.topActions = [
      {
        commitmentId: 'task-top-1',
        title: mockCommitments[0].title,
        type: 'TASK',
        nominalDurationMinutes: 45,
        effectiveDurationMinutes: 63, // Tempo calibrado pelo EAI
        energyRequired: 3,
        scorePercentage: 96.5,
        reason: 'Alta compatibilidade com seu pico cronobiológico matinal (1.25x) e projeto prioritário.',
        wasTimeAdjustedByEai: true,
        projectName: 'compass-core'
      }
    ];
    decisionStore.isServingFromCache = true;

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

  // NOVA ABORDAGEM: Aciona o Boot Cinematográfico e manda para a tela /now
  window.dispatchEvent(new Event('compass:boot-sequence'));
  router.push('/now');

  toastStore.showToast('Inicialização Completa. Banco de dados local ativado!', 'success');
};

  const skipOnboarding = () => finishOnboarding();

  // Mutações isoladas da etapa inicial do tutorial (Intactas)
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