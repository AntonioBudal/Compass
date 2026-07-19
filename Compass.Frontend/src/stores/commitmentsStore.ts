import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { CompassApi } from '@/services/api';
import type { 
  CommitmentDto, 
  CreateCommitmentDto, 
  CommitmentStatus, 
  CommitmentType 
} from '@/types/index';

// Estendendo o DTO localmente para suportar flags visuais transitórias do Optimistic UI
export type CommitmentItem = CommitmentDto & {
  _isSyncing?: boolean;
  _syncError?: string | null;
};

export const useCommitmentsStore = defineStore('commitments', () => {
  // --- Estado Reativo (State) ---
  const items = ref<CommitmentItem[]>([]);
  const isLoading = ref<boolean>(false);
  const globalError = ref<string | null>(null);

  // --- Getters Computados (High SNR & Local-First Filtering) ---
  const activeCandidates = computed(() => 
    items.value.filter(i => 
      (i.status === 'PENDING' || i.status === 'IN_PROGRESS') && 
      (i.type === 'TASK' || i.type === 'HABIT')
    )
  );

  const habitsToday = computed(() => 
    items.value.filter(i => i.type === 'HABIT' && i.status !== 'ARCHIVED')
  );

  const eventsToday = computed(() => 
    items.value.filter(i => i.type === 'EVENT' && i.status !== 'ARCHIVED')
  );

  const projectsSummary = computed(() => {
    const map = new Map<string, { id: string; name: string; count: number }>();
    items.value.forEach(i => {
      if (i.projectId && i.projectName) {
        const existing = map.get(i.projectId) || { id: i.projectId, name: i.projectName, count: 0 };
        existing.count++;
        map.set(i.projectId, existing);
      }
    });
    return Array.from(map.values());
  });

  // --- Ações de Rede & Mutações (Actions) ---
  const fetchAllActive = async () => {
    isLoading.value = true;
    globalError.value = null;
    try {
      const data = await CompassApi.getActiveCommitments();
      items.value = data;
    } catch (err: any) {
      globalError.value = 'Falha ao sincronizar compromissos locais com o servidor.';
      console.error('Erro em fetchAllActive:', err);
    } finally {
      isLoading.value = false;
    }
  };

  const createCommitment = async (payload: CreateCommitmentDto) => {
    // 1. Optimistic UI: Criação local temporária para latência zero
    const tempId = `temp-${Date.now()}`;
    const optimisticItem: CommitmentItem = {
      id: tempId,
      title: payload.title,
      type: payload.type,
      status: 'PENDING',
      estimatedDurationMinutes: payload.estimatedDurationMinutes ?? 30,
      energyRequired: payload.energyRequired ?? 2,
      deadline: payload.deadline ?? null,
      startTime: payload.startTime ?? null,
      endTime: payload.endTime ?? null,
      locationOrLink: payload.locationOrLink ?? null,
      cronExpression: payload.cronExpression ?? null,
      currentStreak: 0,
      bestStreak: 0,
      postponedCount: 0,
      content: payload.content ?? null,
      projectId: payload.projectId ?? null,
      projectName: null,
      _isSyncing: true
    };

    items.value.unshift(optimisticItem);

    // 2. Disparo assíncrono em background
    try {
      const created = await CompassApi.createCommitment(payload);
      // Substitui o item temporário pelo item real consolidado no PostgreSQL
      const index = items.value.findIndex(i => i.id === tempId);
      if (index !== -1) {
        items.value[index] = created;
      }
      return created;
    } catch (err: any) {
      // Rollback: Remove o item otimista em caso de falha (ex: validação)
      items.value = items.value.filter(i => i.id !== tempId);
      throw err;
    }
  };

  const updateStatus = async (id: string, newStatus: CommitmentStatus) => {
    const index = items.value.findIndex(i => i.id === id);
    if (index === -1) return;

    const targetItem = items.value[index];
    const previousStatus = targetItem.status;

    // Se não houve mudança real, ignora o processamento
    if (previousStatus === newStatus) return;

    // 1. Mutação Instantânea em Memória RAM (< 16ms - UI_SPECIFICATION.md Cap. 6.1)
    targetItem.status = newStatus;
    targetItem._isSyncing = true;
    targetItem._syncError = null;

    // 2. Disparo HTTP em background
    try {
      const response = await CompassApi.updateStatus(id, { newStatus });
      targetItem._isSyncing = false;

      // 3. Processamento de Eventos em Cascata (ex: HabitStreakIncremented)
      if (response.cascadedDomainEvents && response.cascadedDomainEvents.length > 0) {
        response.cascadedDomainEvents.forEach(evt => {
          if (evt.eventType === 'HabitStreakIncremented') {
            targetItem.currentStreak = (targetItem.currentStreak || 0) + 1;
            if (targetItem.currentStreak > (targetItem.bestStreak || 0)) {
              targetItem.bestStreak = targetItem.currentStreak;
            }
          }
        });
      }
    } catch (err: any) {
      // 4. Rollback de Otimismo (Resiliência Local-First - Cap. 6.5)
      targetItem.status = previousStatus;
      targetItem._isSyncing = false;
      
      const errorMessage = err.response?.data?.Detail || 'Falha na mutação de estado. Operação revertida.';
      targetItem._syncError = errorMessage;
      
      console.error(` [Rollback de Otimismo]: Revertendo tarefa ${id} para ${previousStatus}. Razão:`, errorMessage);
      throw err;
    }
  };

  return {
    items,
    isLoading,
    globalError,
    activeCandidates,
    habitsToday,
    eventsToday,
    projectsSummary,
    fetchAllActive,
    createCommitment,
    updateStatus
  };
});