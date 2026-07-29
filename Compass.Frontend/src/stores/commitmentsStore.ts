import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { CompassApi } from '@/services/api';
import { useToastStore } from '@/stores/toastStore';
import type { 
  CommitmentDto, 
  CreateCommitmentDto, 
  UpdateCommitmentDto,
  CommitmentStatus 
} from '@/types/index';

export type CommitmentItem = CommitmentDto & {
  _isSyncing?: boolean;
  _syncError?: string | null;
  _lastCompletedDate?: string | null;
};

export const useCommitmentsStore = defineStore('commitments', () => {
  const toastStore = useToastStore();
  const items = ref<CommitmentItem[]>([]);
  const isLoading = ref<boolean>(false);
  const globalError = ref<string | null>(null);

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

  // --- Mutações com UX Defensiva ---

  const createCommitment = async (payload: CreateCommitmentDto) => {
    // 1. BLINDAGEM DE CONTRATO: Evita rejeição do validador FluentValidation no .NET 10
    if (payload.type === 'HABIT' && !payload.cronExpression) {
      payload.cronExpression = '0 8 * * *'; // Recorrência diária padrão às 08:00
    }
    if (payload.type === 'NOTE') {
      payload.estimatedDurationMinutes = 0; // Notas possuem duração líquida zero
    }

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

    // 2. INTERVENÇÃO DEFENSIVA: Criação de Tarefa Fora do Turno Útil (18:00 às 07:00)
    const currentHour = new Date().getHours();
    const isOutsideShift = currentHour >= 18 || currentHour < 7;
    if (payload.type === 'TASK' && isOutsideShift) {
      toastStore.showIntervention({
        code: 'OUTSIDE_SHIFT_CREATION',
        title: 'Seu turno de hoje já terminou.',
        explanation: 'O motor de decisão agendou esta atividade para o início de amanhã, evitando sobrecarregar sua tela Agora.',
        severity: 'info',
        actions: [
          {
            label: 'Mover para amanhã',
            isPrimary: true,
            handler: () => {}
          },
          {
            label: 'Executar hoje (Hora Extra)',
            handler: async () => {
              const target = items.value.find(i => i.id === tempId);
              if (target) {
                target.deadline = new Date().toISOString();
              }
            }
          }
        ]
      });
    }

    // 3. INTERVENÇÃO DEFENSIVA: Tarefa Avulsa (Sem Projeto Vinculado)
    if (payload.type === 'TASK' && !payload.projectId && !isOutsideShift) {
      toastStore.showIntervention({
        code: 'MISSING_PROJECT_BINDING',
        title: 'Atividade criada sem projeto.',
        explanation: 'Tarefas avulsas recebem pontuação menor no Now Engine. Itens vinculados a projetos ativos ganham prioridade de foco.',
        severity: 'warning',
        actions: [
          {
            label: 'Manter como avulsa',
            isPrimary: true,
            handler: () => {}
          },
          {
            label: 'Vincular a um Projeto',
            handler: () => {
              window.dispatchEvent(new CustomEvent('compass:open-project-selector', { detail: { commitmentId: tempId } }));
            }
          }
        ]
      });
    }

    try {
      const created = await CompassApi.createCommitment(payload);
      const index = items.value.findIndex(i => i.id === tempId);
      if (index !== -1) {
        items.value[index] = created;
      }
      return created;
    } catch (err: any) {
      items.value = items.value.filter(i => i.id !== tempId);
      throw err;
    }
  };

  const updateCommitment = async (id: string, payload: UpdateCommitmentDto) => {
    const index = items.value.findIndex(i => i.id === id);
    if (index === -1) return;

    const originalItem = { ...items.value[index] };
    Object.assign(items.value[index], payload, { _isSyncing: true });

    try {
      const updated = await CompassApi.updateCommitment(id, payload);
      items.value[index] = updated;
      toastStore.showToast('Compromisso atualizado.', 'neutral');
    } catch (err: any) {
      items.value[index] = originalItem;
      toastStore.showToast('Falha na edição. Alterações revertidas.', 'error');
      throw err;
    }
  };

  const updateStatus = async (id: string, newStatus: CommitmentStatus) => {
    const index = items.value.findIndex(i => i.id === id);
    if (index === -1) return;

    const targetItem = items.value[index];
    const previousStatus = targetItem.status;
    if (previousStatus === newStatus) return;

    // 4. INTERVENÇÃO DEFENSIVA: Proteção de Consistência (Hábito Concluído 2x no Mesmo Dia)
    const todayIso = new Date().toISOString().slice(0, 10);
    if (targetItem.type === 'HABIT' && newStatus === 'COMPLETED' && targetItem._lastCompletedDate === todayIso) {
      toastStore.showIntervention({
        code: 'HABIT_ALREADY_COMPLETED',
        title: 'Você já registrou este hábito hoje!',
        explanation: `Sua sequência atual de 🔥 ${targetItem.currentStreak || 1} dias já está garantida. Hábitos contam apenas uma vez por dia.`,
        severity: 'info',
        actions: [
          {
            label: 'Entendi, fechar',
            isPrimary: true,
            handler: () => {}
          }
        ]
      });
      return;
    }

    targetItem.status = newStatus;
    targetItem._isSyncing = true;
    targetItem._syncError = null;

    try {
      const response = await CompassApi.updateStatus(id, { newStatus });
      targetItem._isSyncing = false;

      if (newStatus === 'COMPLETED' && targetItem.type === 'HABIT') {
        targetItem._lastCompletedDate = todayIso;
      }

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

      toastStore.showToast(
        `Status alterado para ${newStatus}.`, 
        newStatus === 'COMPLETED' ? 'success' : 'neutral',
        async () => {
          await updateStatus(id, previousStatus);
        }
      );
    } catch (err: any) {
      targetItem.status = previousStatus;
      targetItem._isSyncing = false;
      
      const errorMessage = err.response?.data?.Detail || 'Falha na mutação de estado.';
      targetItem._syncError = errorMessage;
      toastStore.showToast(errorMessage, 'error');
      throw err;
    }
  };

  const deleteCommitment = async (id: string) => {
    const index = items.value.findIndex(i => i.id === id);
    if (index === -1) return;

    const removedItem = items.value[index];
    items.value.splice(index, 1);

    toastStore.showToast(
      `Compromisso removido.`,
      'neutral',
      async () => {
        items.value.splice(index, 0, removedItem);
        try {
          await CompassApi.updateStatus(removedItem.id, { newStatus: 'PENDING' });
        } catch (e) {
          console.error('Falha ao reverter exclusão no servidor', e);
        }
      },
      8000
    );

    setTimeout(async () => {
      const stillDeleted = !items.value.some(i => i.id === id);
      if (stillDeleted) {
        try {
          await CompassApi.deleteCommitment(id);
        } catch (err) {
          console.error(`Erro ao efetivar exclusão no PostgreSQL para o item ${id}`, err);
        }
      }
    }, 8000);
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
    updateCommitment,
    updateStatus,
    deleteCommitment
  };
});