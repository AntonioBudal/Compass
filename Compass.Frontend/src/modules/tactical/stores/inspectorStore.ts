import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { useCommitmentsStore, type CommitmentItem } from './commitmentsStore';

import cloneDeep from 'lodash/cloneDeep';
import { DraftCommitmentSchema } from '@/modules/tactical/schemas/draftSchema';
import { useToastStore } from '@/shared/stores/toastStore';
import { useGoalsStore } from '@/modules/strategy/stores/goalsStore';
import { useProjectsStore } from '@/modules/strategy/stores/projectsStore';

export type InspectableType = 'COMMITMENT' | 'PROJECT' | 'GOAL';

export interface DraftState {
  entityType: InspectableType;
  entityId: string;
  originalPayload: any;
  mutatedPayload: any;
}

export type SyncStatus = 'IDLE' | 'EDITING' | 'SYNCING' | 'SAVED' | 'ERROR' | 'PAUSED_FOR_CONFIRMATION';

export const useInspectorStore = defineStore('inspector', () => {
  const commitmentsStore = useCommitmentsStore();
  const toastStore = useToastStore();
  const goalsStore = useGoalsStore();
  const projectsStore = useProjectsStore();

  const isOpen = ref(false);
  const syncStatus = ref<SyncStatus>('IDLE');
  const draft = ref<DraftState | null>(null);
  
  let debounceTimer: ReturnType<typeof setTimeout> | null = null;
  const AUTO_SAVE_DELAY_MS = 800;

  const isDirty = computed(() => {
    if (!draft.value) return false;
    return JSON.stringify(draft.value.originalPayload) !== JSON.stringify(draft.value.mutatedPayload);
  });

  const openInspector = (entity: any, type: InspectableType) => {
    if (debounceTimer) clearTimeout(debounceTimer);
    draft.value = {
      entityType: type,
      entityId: entity.id,
      originalPayload: cloneDeep(entity),
      mutatedPayload: cloneDeep(entity)
    };
    syncStatus.value = 'IDLE';
    isOpen.value = true;
  };

  const markAsEditing = () => {
    if (!draft.value) return;
    if (syncStatus.value === 'PAUSED_FOR_CONFIRMATION') return;

    syncStatus.value = 'EDITING';
    if (debounceTimer) clearTimeout(debounceTimer);
    
    debounceTimer = setTimeout(() => {
      executeAutoSave();
    }, AUTO_SAVE_DELAY_MS);
  };

  const flushAndClose = async () => {
    if (debounceTimer) clearTimeout(debounceTimer);
    if (isDirty.value && syncStatus.value !== 'SYNCING' && syncStatus.value !== 'PAUSED_FOR_CONFIRMATION') {
      await executeAutoSave();
    }
    closeInspector();
  };

  const closeInspector = () => {
    isOpen.value = false;
    setTimeout(() => {
      draft.value = null;
      syncStatus.value = 'IDLE';
    }, 200);
  };

  // --- 1. MOTOR DE UX DEFENSIVA: AVALIAÇÃO DE EXCLUSÃO RELACIONAL EM CASCATA ---
  const requestDeletion = () => {
    if (!draft.value) return;
    const { entityType, entityId, originalPayload } = draft.value;

    //  ARQ: Avaliação de Exclusão de META (Top-Level)
    if (entityType === 'GOAL') {
      const allKnownProjects = Object.values(projectsStore.entities);
      const linkedProjects = allKnownProjects.filter(p => p.goalId === entityId);
      
      if (linkedProjects.length > 0) {
        syncStatus.value = 'PAUSED_FOR_CONFIRMATION';
        toastStore.showIntervention({
          code: 'GOAL_HAS_ORPHANS',
          title: 'Atenção ao excluir a Meta Estratégica',
          explanation: `Esta meta possui ${linkedProjects.length} projeto(s) vinculado(s). Excluir a meta NÃO apagará os projetos; eles ficarão "avulsos" (sem bússola).`,
          severity: 'warning',
          actions: [
            { label: 'Cancelar', isPrimary: true, handler: () => { syncStatus.value = 'IDLE'; } },
            { label: 'Descartar Meta (Arquivar)', handler: async () => { 
                await goalsStore.updateGoalStatus(entityId, 'ARCHIVED');
                closeInspector(); 
              } 
            },
            { label: 'Excluir Meta', handler: async () => { await executeDelete(entityType, entityId); } }
          ]
        });
        return;
      }
    }

    // Avaliação de Exclusão de PROJETO (Mid-Level)
    else if (entityType === 'PROJECT') {
      const allKnownCommitments = Object.values(commitmentsStore.entities);
      const linkedTasks = allKnownCommitments.filter(i => i.projectId === entityId);
      
      if (linkedTasks.length > 0) {
        syncStatus.value = 'PAUSED_FOR_CONFIRMATION';
        toastStore.showIntervention({
          code: 'PROJECT_HAS_ORPHANS',
          title: 'Atenção ao excluir o projeto',
          explanation: `Este projeto possui ${linkedTasks.length} tarefa(s) vinculada(s). Excluir o projeto NÃO apagará as tarefas; elas ficarão "avulsas" na sua lista.`,
          severity: 'warning',
          actions: [
            { label: 'Cancelar', isPrimary: true, handler: () => { syncStatus.value = 'IDLE'; } },
            //  ARQ: O projeto agora suporta edição total, podemos usar updateProject para simular status se precisarmos
            { label: 'Excluir Projeto e Desvincular Tarefas', handler: async () => { await executeDelete(entityType, entityId); } }
          ]
        });
        return;
      }
    } 
    
    // Avaliação de Exclusão de HÁBITOS (Base-Level)
    else if (entityType === 'COMMITMENT') {
      const item = originalPayload as CommitmentItem;
      if (item.type === 'HABIT' && (item.currentStreak > 3 || item.bestStreak > 3)) {
        syncStatus.value = 'PAUSED_FOR_CONFIRMATION';
        toastStore.showIntervention({
          code: 'HABIT_DELETE_STREAK',
          title: 'Destruição de Histórico Local',
          explanation: `Você está prestes a excluir um hábito com histórico de ${item.currentStreak || item.bestStreak} dias. Essa ação destruirá estas estatísticas para sempre.`,
          severity: 'warning',
          actions: [
            { label: 'Cancelar', isPrimary: true, handler: () => { syncStatus.value = 'IDLE'; } },
            { label: 'Arquivar Hábito', handler: async () => { 
                await commitmentsStore.updateStatus(entityId, 'ARCHIVED');
                closeInspector(); 
              } 
            },
            { label: 'Excluir Permanentemente', handler: async () => { await executeDelete(entityType, entityId); } }
          ]
        });
        return;
      }
    }

    // Sem impactos críticos = Execução Limpa.
    executeDelete(entityType, entityId);
  };

  const executeDelete = async (type: InspectableType, id: string) => {
    closeInspector(); 
    if (type === 'COMMITMENT') {
      await commitmentsStore.deleteCommitment(id);
    } else if (type === 'PROJECT') {
      await projectsStore.deleteProject(id);
    } else if (type === 'GOAL') {
      await goalsStore.deleteGoal(id); 
    }
  };

  // --- 2. MOTOR DE UX DEFENSIVA: AUTO-SAVE ---
  const executeAutoSave = async () => {
    if (!draft.value || !isDirty.value) {
      syncStatus.value = 'SAVED';
      return;
    }

    const currentSnapshot = cloneDeep(draft.value.originalPayload);
    const targetPayload = cloneDeep(draft.value.mutatedPayload);
    const targetId = draft.value.entityId;
    const targetType = draft.value.entityType;

    if (targetType === 'COMMITMENT') {
      const validation = DraftCommitmentSchema.safeParse(targetPayload);
      if (!validation.success) {
        syncStatus.value = 'ERROR';
        toastStore.showToast('Erro de validação local. Verifique os campos.', 'error');
        return; 
      }
    }

    if (targetType === 'COMMITMENT') {
      const original = currentSnapshot as CommitmentItem;
      const mutado = targetPayload as CommitmentItem;

      if (original.type === 'HABIT' && original.cronExpression !== mutado.cronExpression && (original.currentStreak > 0 || original.bestStreak > 0)) {
        syncStatus.value = 'PAUSED_FOR_CONFIRMATION';
        
        toastStore.showIntervention({
          code: 'HABIT_CRON_CHANGED',
          title: 'Aviso: Alteração de Recorrência',
          explanation: `Este hábito possui um histórico ativo de ${original.currentStreak || original.bestStreak} dias. Mudar a recorrência dele pode distorcer suas métricas de consistência passada.`,
          severity: 'warning',
          actions: [
            {
              label: 'Desfazer',
              isPrimary: true,
              handler: async () => {
                if (draft.value) draft.value.mutatedPayload.cronExpression = original.cronExpression;
                syncStatus.value = 'IDLE';
              }
            },
            {
              label: 'Compreendo, alterar mesmo assim',
              handler: async () => {
                await commitPayload(targetType, targetId, targetPayload, currentSnapshot);
              }
            }
          ]
        });
        return; 
      }
    }

    await commitPayload(targetType, targetId, targetPayload, currentSnapshot);
  };

  const commitPayload = async (type: InspectableType, id: string, payload: any, snapshot: any) => {
    syncStatus.value = 'SYNCING';
    try {
      if (type === 'COMMITMENT') {
        await commitmentsStore.updateCommitment(id, payload);
      } else if (type === 'GOAL') {
        await goalsStore.updateGoal(id, payload);
      } else if (type === 'PROJECT') { 
        await projectsStore.updateProject(id, payload);
      }
      
      syncStatus.value = 'SAVED';
      if (draft.value) draft.value.originalPayload = cloneDeep(payload);

      toastStore.showToast(
        'Alterações sincronizadas.', 
        'neutral',
        async () => { await executeRollback(type, id, snapshot); },
        5000
      );

    } catch (e) {
      syncStatus.value = 'ERROR';
    }
  };

  const executeRollback = async (type: InspectableType, id: string, snapshot: any) => {
    try {
      if (type === 'COMMITMENT') await commitmentsStore.updateCommitment(id, snapshot);
      else if (type === 'GOAL') await goalsStore.updateGoal(id, snapshot);
      else if (type === 'PROJECT') await projectsStore.updateProject(id, snapshot);
      
      if (isOpen.value && draft.value?.entityId === id) {
        draft.value.originalPayload = cloneDeep(snapshot);
        draft.value.mutatedPayload = cloneDeep(snapshot);
        syncStatus.value = 'IDLE';
      }
    } catch (e) {
      toastStore.showToast('Falha ao restaurar versão anterior.', 'error');
    }
  };

  return {
    isOpen, syncStatus, draft,
    openInspector, markAsEditing, flushAndClose, closeInspector, requestDeletion
  };
});