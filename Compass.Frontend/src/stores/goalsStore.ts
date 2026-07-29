import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { useToastStore } from './toastStore';

export interface GoalChildItem {
  id: string;
  name: string;
  status: 'PENDING' | 'IN_PROGRESS' | 'COMPLETED';
  progress: number;
}

export interface GoalItem {
  id: string;
  title: string;
  why: string;
  targetDate: string;
  progressPercentage: number;
  status: 'ACTIVE' | 'COMPLETED' | 'ARCHIVED';
  children: GoalChildItem[];
}

const STORAGE_KEY = 'compass_goals_cache_v1';

export const useGoalsStore = defineStore('goals', () => {
  const toastStore = useToastStore();
  const goals = ref<GoalItem[]>([]);
  const isLoaded = ref(false);

  // Carrega do disco local
  const loadFromDisk = () => {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (raw) {
        goals.value = JSON.parse(raw);
        isLoaded.value = true;
        return;
      }
    } catch (e) {
      console.warn('[GoalsStore] Erro ao carregar metas do localStorage', e);
    }

    // Dados iniciais padrão se o disco estiver vazio
    goals.value = [
      {
        id: 'goal-1',
        title: 'Lançamento do Compass MVP (Q3 2026)',
        why: 'Provar a viabilidade de um software de produtividade local-first em .NET 10 e Vue 3.',
        targetDate: '30/09/2026',
        progressPercentage: 65,
        status: 'ACTIVE',
        children: [
          { id: 'child-101', name: 'Compass Backend Core (.NET 10 REST API)', status: 'IN_PROGRESS', progress: 80 },
          { id: 'child-102', name: 'Auth & Identity JWT', status: 'PENDING', progress: 50 },
          { id: 'child-103', name: 'Design System UI (Zinc Monocromático)', status: 'COMPLETED', progress: 100 }
        ]
      },
      {
        id: 'goal-2',
        title: 'Excelência em Engenharia e Arquitetura Limpa',
        why: 'Dominar os padrões DDD e CQRS em ambientes de missão crítica.',
        targetDate: '15/12/2026',
        progressPercentage: 40,
        status: 'ACTIVE',
        children: [
          { id: 'child-201', name: 'Leitura de Arquitetura de Software (Hábito)', status: 'IN_PROGRESS', progress: 60 },
          { id: 'child-202', name: 'Refatoração do Motor de Scoring', status: 'IN_PROGRESS', progress: 20 }
        ]
      }
    ];
    saveToDisk();
    isLoaded.value = true;
  };

  const saveToDisk = () => {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(goals.value));
    } catch (e) {
      console.warn('[GoalsStore] Falha ao persistir metas.', e);
    }
  };

  // Mutações interativas e cálculo automático de progresso
  const recalculateProgress = (goal: GoalItem) => {
    if (!goal.children || goal.children.length === 0) {
      goal.progressPercentage = 0;
      return;
    }
    const sum = goal.children.reduce((acc, curr) => acc + curr.progress, 0);
    goal.progressPercentage = Math.round(sum / goal.children.length);

    if (goal.progressPercentage === 100) goal.status = 'COMPLETED';
    else if (goal.status === 'COMPLETED' && goal.progressPercentage < 100) goal.status = 'ACTIVE';
  };

  const updateGoalTitle = (id: string, newTitle: string) => {
    const goal = goals.value.find(g => g.id === id);
    if (goal && newTitle.trim()) {
      goal.title = newTitle.trim();
      saveToDisk();
      toastStore.showToast('Título da meta atualizado.', 'neutral');
    }
  };

  const updateGoalStatus = (id: string, newStatus: GoalItem['status']) => {
    const goal = goals.value.find(g => g.id === id);
    if (goal) {
      goal.status = newStatus;
      saveToDisk();
      toastStore.showToast(`Meta alterada para ${newStatus}.`, 'success');
    }
  };

  const addChildModule = (goalId: string, name: string) => {
    const goal = goals.value.find(g => g.id === goalId);
    if (goal && name.trim()) {
      goal.children.push({
        id: `child-${Date.now()}`,
        name: name.trim(),
        status: 'PENDING',
        progress: 0
      });
      recalculateProgress(goal);
      saveToDisk();
      toastStore.showToast('Módulo adicionado à meta.', 'success');
    }
  };

  const updateChildProgress = (goalId: string, childId: string, progress: number) => {
    const goal = goals.value.find(g => g.id === goalId);
    if (!goal) return;
    const child = goal.children.find(c => c.id === childId);
    if (child) {
      child.progress = Math.min(100, Math.max(0, progress));
      child.status = child.progress === 100 ? 'COMPLETED' : child.progress === 0 ? 'PENDING' : 'IN_PROGRESS';
      recalculateProgress(goal);
      saveToDisk();
    }
  };

  const activeGoals = computed(() => goals.value.filter(g => g.status !== 'ARCHIVED'));

  return {
    goals,
    activeGoals,
    loadFromDisk,
    updateGoalTitle,
    updateGoalStatus,
    addChildModule,
    updateChildProgress
  };
});