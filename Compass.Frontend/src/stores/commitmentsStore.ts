import { defineStore } from 'pinia';
import axios from 'axios';
import { ref, computed, watch } from 'vue';
import { CompassApi } from '@/services/api';
import { useToastStore } from '@/stores/toastStore';
import { GlobalHistoryProvider } from '@/utils/autocomplete/providers/HistoryProvider';
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



const databaseItems = ref<CommitmentItem[]>([]);
  const databaseTotal = ref<number>(0);
  const isDatabaseLoading = ref<boolean>(false);

  export interface DatabaseFilters {
    search?: string;
    type?: 'TASK' | 'EVENT' | 'HABIT' | 'NOTE' | '';
    status?: 'PENDING' | 'IN_PROGRESS' | 'COMPLETED' | 'ARCHIVED' | '';
    projectId?: string;
  }

export const useCommitmentsStore = defineStore('commitments', () => {
  const toastStore = useToastStore();
  const items = ref<CommitmentItem[]>([]);
  const isLoading = ref<boolean>(false);
  const globalError = ref<string | null>(null);
  const isLoaded = ref<boolean>(false);

  

  watch(() => items.value, (newItems) => {
    if (newItems) {
      // O HistoryProvider fará a deduplicação e calculará os pesos matemáticos
      GlobalHistoryProvider.syncData(newItems);
    }
  }, { deep: true, immediate: true });

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

  const fetchAllActive = async (force: boolean = false) => {
    // Se já carregou uma vez na sessão e não estamos forçando, preserva a memória local!
    if (isLoaded.value && !force) return; 

    isLoading.value = true;
    globalError.value = null;
    try {
      const data = await CompassApi.getActiveCommitments();
      items.value = data;
      isLoaded.value = true; // Marca como hidratado
    } catch (err: any) {
      globalError.value = 'Falha ao sincronizar compromissos locais com o servidor.';
      console.error('Erro em fetchAllActive:', err);
    } finally {
      isLoading.value = false;
    }
  };

  const fetchDatabase = async (page = 1, limit = 50, filters?: DatabaseFilters) => {
    isDatabaseLoading.value = true;
    try {
      const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';
      
      // Monta os parâmetros da URL
      const params = new URLSearchParams({
        page: page.toString(),
        limit: limit.toString(),
      });
      if (filters?.search) params.append('search', filters.search);
      if (filters?.type) params.append('type', filters.type);
      if (filters?.status) params.append('status', filters.status);
      if (filters?.projectId) params.append('projectId', filters.projectId);

      // Assumindo que o Backend criará esta rota no futuro
      const res = await axios.get(`${baseUrl}/commitments/all?${params.toString()}`, {
        headers: { 'X-User-Id': '11111111-1111-1111-1111-111111111111' }
      });
      
      // Suporta tanto o retorno paginado { items: [], total: x } quanto o array direto []
      databaseItems.value = res.data.items || res.data || [];
      databaseTotal.value = res.data.total || databaseItems.value.length;

    } catch (err: any) {
      console.warn('[CommitmentsStore] Endpoint de Database não encontrado ou falhou. Usando Mock de RAM.', err);
      
      // Mock Temporário: Simula o banco filtrando os dados que já temos na RAM
      let mockList = [...items.value];
      if (filters?.search) {
        const q = filters.search.toLowerCase();
        mockList = mockList.filter(i => i.title.toLowerCase().includes(q));
      }
      if (filters?.type) mockList = mockList.filter(i => i.type === filters.type);
      if (filters?.status) mockList = mockList.filter(i => i.status === filters.status);
      if (filters?.projectId) mockList = mockList.filter(i => i.projectId === filters.projectId);
      
      databaseItems.value = mockList;
      databaseTotal.value = mockList.length;
    } finally {
      isDatabaseLoading.value = false;
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

  const updateCommitment = async (id: string, payload: UpdateCommitmentDto, isSilent: boolean = false) => {
    
    const dbIndex = databaseItems.value.findIndex(i => i.id === id);
    if (dbIndex !== -1) {
      Object.assign(databaseItems.value[dbIndex], payload, { _isSyncing: true });
    }
    
    const index = items.value.findIndex(i => i.id === id);
    if (index === -1) return;

    const originalItem = { ...items.value[index] };
    
    // Mutação Otimista
    Object.assign(items.value[index], payload, { _isSyncing: true });

    try {
      const updated = await CompassApi.updateCommitment(id, payload);
      
      
      Object.assign(items.value[index], updated, { _isSyncing: false });
      
      if (!isSilent) toastStore.showToast('Compromisso atualizado.', 'neutral');
    } catch (err: any) {
      // Reverte mantendo a mesma referência
      Object.assign(items.value[index], originalItem, { _isSyncing: false });
      if (!isSilent) toastStore.showToast('Falha na edição. Alterações revertidas.', 'error');
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

    const dbIndex = databaseItems.value.findIndex(i => i.id === id);
    if (dbIndex !== -1) {
      databaseItems.value.splice(dbIndex, 1);
      databaseTotal.value = Math.max(0, databaseTotal.value - 1);
    }
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
    deleteCommitment,
    databaseItems,
    databaseTotal,
    isDatabaseLoading,
    fetchDatabase
  };
});