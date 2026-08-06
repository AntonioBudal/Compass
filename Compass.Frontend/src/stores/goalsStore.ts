import { defineStore } from 'pinia';
import { ref, computed, watch } from 'vue';
import { useToastStore } from './toastStore';
import { useProjectsStore } from './projectsStore'; //  ARQ: Integração Top-Down
import { GoalProvider } from '@/utils/autocomplete/AutocompleteEngine';
import { CompassApi } from '@/services/api';

//  DTO PURIFICADO: Adeus `children` falsos, adeus `progressPercentage` manual.
export interface GoalItemDto {
  id: string;
  title: string;
  why: string;
  targetDate: string;
  status: 'ACTIVE' | 'COMPLETED' | 'ARCHIVED';
}

const STORAGE_KEY = 'compass_goals_cache_v2';

export const useGoalsStore = defineStore('goals', () => {
  const toastStore = useToastStore();
  const projectsStore = useProjectsStore();

  const entities = ref<Record<string, GoalItemDto>>({});
  const goalIds = ref<string[]>([]);
  
  const isLoaded = ref(false);

  const rawGoals = computed(() => goalIds.value.map(id => entities.value[id]).filter(Boolean));

  //  MÁGICA CASCATA (Top-Down): Calcula o avanço da Meta baseado nos Projetos (que vêm das Tarefas)
  const enrichedGoals = computed(() => {
    const allEnrichedProjects = projectsStore.enrichedProjects;

    return rawGoals.value.map(goal => {
      // Pega os projetos atrelados a esta Meta
      const goalProjects = allEnrichedProjects.filter(p => p.goalId === goal.id);
      
      let progressPercentage = 0;
      if (goalProjects.length > 0) {
        const sum = goalProjects.reduce((acc, p) => acc + p.progressPercentage, 0);
        progressPercentage = Math.round(sum / goalProjects.length);
      }

      // O Status continua manual (o usuário decide quando a Meta inteira foi batida),
      // mas o progresso reflete a matemática real da execução diária.
      return {
        ...goal,
        progressPercentage,
        projectCount: goalProjects.length,
        projects: goalProjects // Enviamos os projetos hidratados para a UI exibir dentro da Meta
      };
    });
  });

  const activeGoals = computed(() => enrichedGoals.value.filter(g => g.status !== 'ARCHIVED'));

  watch(() => rawGoals.value, (newGoals) => {
    if (newGoals && newGoals.length > 0) {
      GoalProvider.syncData(newGoals.map(g => ({ id: g.id, name: g.title })));
    }
  }, { deep: true, immediate: true });

  const saveToDisk = () => {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(rawGoals.value));
    } catch (e) {}
  };

  const loadFromDisk = () => {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (raw) {
        const parsed: GoalItemDto[] = JSON.parse(raw);
        const newEntities: Record<string, GoalItemDto> = {};
        const newIds: string[] = [];
        
        parsed.forEach(g => {
          const { children, progressPercentage, ...cleanGoal } = g as any;
          newEntities[cleanGoal.id] = cleanGoal;
          newIds.push(cleanGoal.id);
        });

        entities.value = newEntities;
        goalIds.value = newIds;
        isLoaded.value = true;
      }
    } catch (e) {}
  };

  const fetchGoals = async () => {
    try {
      // 🚀 Agora sim! Busca as Metas reais (com Guids reais) do Banco de Dados
      const res = await CompassApi.getActiveGoals();
      const newEntities: Record<string, GoalItemDto> = {};
      const newIds: string[] = [];
      
      res.forEach(g => {
        newEntities[g.id] = g;
        newIds.push(g.id);
      });

      entities.value = newEntities;
      goalIds.value = newIds;
      
      saveToDisk();
      isLoaded.value = true;
    } catch (e) {
      console.warn('[GoalsStore] API indisponível. Carregando cache local.');
      loadFromDisk();
    }
  };

  const updateGoalStatus = (id: string, newStatus: GoalItemDto['status']) => {
    const goal = entities.value[id];
    if (goal) {
      goal.status = newStatus;
      saveToDisk();
      toastStore.showToast(`Meta alterada para ${newStatus}.`, 'success');
    }
  };

  //  ARQ: Motor de Criação de Metas
  const createGoal = async (payload: { title: string, whyDescription?: string | null, targetDate?: string | null }) => {
    console.log('[GoalsStore] 1. Tentando criar meta na API...', payload);
    
    try {
      // 🚀 Chamada rigorosa para o Backend
      const created = await CompassApi.createGoal(payload);
      
      console.log('[GoalsStore] 2. API retornou sucesso com Guid Real:', created);
      
      entities.value[created.id] = created;
      goalIds.value.unshift(created.id);
      saveToDisk();
      
      return created;
    } catch (error) {
      console.error('[GoalsStore] ERRO FATAL ao criar Meta:', error);
      throw error;
    }
  };

  const updateGoal = async (id: string, payload: Partial<GoalItemDto>, isSilent: boolean = false) => {
    if (!entities.value[id]) return;

    const originalItem = { ...entities.value[id] };
    Object.assign(entities.value[id], payload);

    try {
      
      await CompassApi.updateGoal(id, {
        title: entities.value[id].title,
        whyDescription: entities.value[id].why, // Tratamos a diferença de nomenclatura (why -> whyDescription)
        targetDate: entities.value[id].targetDate
      });
      saveToDisk();
      if (!isSilent) toastStore.showToast('Meta atualizada com sucesso.', 'neutral');
    } catch (err: any) {
      Object.assign(entities.value[id], originalItem);
      if (!isSilent) toastStore.showToast('Falha na edição. Revertido.', 'error');
      throw err;
    }
  };

  const deleteGoal = async (id: string) => {
    if (!entities.value[id]) return;
    
    // Otimismo Visual
    const goalToDelete = { ...entities.value[id] };
    delete entities.value[id];
    goalIds.value = goalIds.value.filter(gId => gId !== id);

    try {
     
      await CompassApi.deleteGoal(id);
      saveToDisk();
      toastStore.showToast('Meta excluída com sucesso.', 'neutral');
    } catch (err) {
      // Rollback se falhar
      entities.value[id] = goalToDelete;
      goalIds.value.unshift(id);
      toastStore.showToast('Falha ao excluir a meta.', 'error');
      throw err;
    }
  };

  

  return {
    entities,
    goals: enrichedGoals, 
    activeGoals,
    isLoaded, // 
    loadFromDisk,
    fetchGoals,
    updateGoalStatus,
    createGoal,
    deleteGoal,
    updateGoal
  };
});
