import { defineStore } from 'pinia';
import axios from 'axios';
import { ref, computed } from 'vue';
import { CompassApi } from '@/services/api';
import { useToastStore } from '@/stores/toastStore';
import { GlobalHistoryProvider } from '@/utils/autocomplete/providers/HistoryProvider';
import { useOfflineStore } from '@/stores/offlineStore';
import type { 
  CommitmentDto, 
  CreateCommitmentDto, 
  UpdateCommitmentDto,
  CommitmentStatus 
} from '@/types/index';

export type CommitmentItem = CommitmentDto & {
  _isSyncing?: boolean;
  _syncError?: string | null;
};

export interface DatabaseFilters {
  search?: string;
  type?: 'TASK' | 'EVENT' | 'HABIT' | 'NOTE' | '';
  status?: 'PENDING' | 'IN_PROGRESS' | 'COMPLETED' | 'ARCHIVED' | '';
  projectId?: string;
}

export const useCommitmentsStore = defineStore('commitments', () => {
  const toastStore = useToastStore();
  const offlineStore = useOfflineStore();

  // 🔥 ARQ: O Vue é apenas um Espelho do EF Core / SQLite
  const entities = ref<Record<string, CommitmentItem>>({});
  
  const activeIds = ref<string[]>([]);
  const databaseIds = ref<string[]>([]);

  const isLoading = ref<boolean>(false);
  const globalError = ref<string | null>(null);
  const isLoaded = ref<boolean>(false);
  const databaseTotal = ref<number>(0);
  const isDatabaseLoading = ref<boolean>(false);

  const allKnownEntities = computed(() => Object.values(entities.value));

  const items = computed(() => activeIds.value.map(id => entities.value[id]).filter(Boolean));
  const databaseItems = computed(() => databaseIds.value.map(id => entities.value[id]).filter(Boolean));

  const activeCandidates = computed(() => 
    allKnownEntities.value.filter(i => 
      (i.status === 'PENDING' || i.status === 'IN_PROGRESS') && 
      (i.type === 'TASK' || i.type === 'HABIT')
    )
  );

  const habitsToday = computed(() => 
    allKnownEntities.value.filter(i => i.type === 'HABIT' && i.status !== 'ARCHIVED')
  );

  const eventsToday = computed(() => 
    allKnownEntities.value.filter(i => i.type === 'EVENT' && i.status !== 'ARCHIVED')
  );

  const projectsSummary = computed(() => {
    const map = new Map<string, { id: string; name: string; count: number }>();
    allKnownEntities.value.forEach(i => {
      if (i.projectId && i.projectName) {
        const existing = map.get(i.projectId) || { id: i.projectId, name: i.projectName, count: 0 };
        existing.count++;
        map.set(i.projectId, existing);
      }
    });
    return Array.from(map.values());
  });

  const syncHistory = () => {
    GlobalHistoryProvider.syncData(allKnownEntities.value);
  };

  const fetchAllActive = async (force: boolean = false) => {
    if (isLoaded.value && !force) return; 

    isLoading.value = true;
    globalError.value = null;
    try {
      const data = await CompassApi.getActiveCommitments();
      
      const newEntities: Record<string, CommitmentItem> = {};
      const ids: string[] = [];
      data.forEach((item: CommitmentItem) => {
        newEntities[item.id] = item;
        ids.push(item.id);
      });
      
      entities.value = newEntities;
      activeIds.value = ids;
      isLoaded.value = true;
      syncHistory();
    } catch (err: any) {
      globalError.value = 'Falha ao sincronizar compromissos com o banco de dados.';
    } finally {
      isLoading.value = false;
    }
  };

  const fetchDatabase = async (page = 1, limit = 50, filters?: DatabaseFilters) => {
    isDatabaseLoading.value = true;
    try {
      const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';
      
      const params = new URLSearchParams({
        page: page.toString(),
        limit: limit.toString(),
      });
      if (filters?.search) params.append('search', filters.search);
      if (filters?.type) params.append('type', filters.type);
      if (filters?.status) params.append('status', filters.status);
      if (filters?.projectId) params.append('projectId', filters.projectId);

      const res = await axios.get(`${baseUrl}/commitments/all?${params.toString()}`, {
        headers: { 'X-User-Id': '11111111-1111-1111-1111-111111111111' }
      });
      
      const rawData = res.data.items || res.data || [];
      
      const newEntities = { ...entities.value };
      const ids: string[] = [];
      rawData.forEach((item: CommitmentItem) => {
        newEntities[item.id] = item;
        ids.push(item.id);
      });
      
      entities.value = newEntities;
      databaseIds.value = ids;
      databaseTotal.value = res.data.total || databaseIds.value.length;
      syncHistory();
    } catch (err: any) {
      console.error('[CommitmentsStore] Falha ao buscar database', err);
    } finally {
      isDatabaseLoading.value = false;
    }
  };

  // --- MUTAÇÕES ---
  const createCommitment = async (payload: CreateCommitmentDto) => {
    if (payload.type === 'HABIT' && !payload.cronExpression) payload.cronExpression = '0 8 * * *';
    if (payload.type === 'NOTE') payload.estimatedDurationMinutes = 0; 

    // 🔥 ARQ: O Backend Cria, Nós Espelhamos
    const created = await CompassApi.createCommitment(payload);
    
    entities.value = { ...entities.value, [created.id]: created };
    activeIds.value.unshift(created.id);
    if (!databaseIds.value.includes(created.id)) {
        databaseIds.value.unshift(created.id);
    }
    
    syncHistory();
    return created;
  };

  const updateCommitment = async (id: string, payload: UpdateCommitmentDto, isSilent: boolean = false) => {
    if (!entities.value[id]) return;
    const originalItem = { ...entities.value[id] };
    
    // Otimismo Visual
    Object.assign(entities.value[id], payload, { _isSyncing: true });

    try {
      const updated = await CompassApi.updateCommitment(id, payload);
      Object.assign(entities.value[id], updated, { _isSyncing: false });
      syncHistory();
      if (!isSilent) toastStore.showToast('Compromisso atualizado.', 'neutral');
    } catch (err: any) {
      Object.assign(entities.value[id], originalItem, { _isSyncing: false });
      if (!isSilent) toastStore.showToast('Falha na edição. Revertido.', 'error');
      throw err;
    }
  };

  const updateStatus = async (id: string, newStatus: CommitmentStatus) => {
    if (!entities.value[id]) return;
    const targetItem = entities.value[id];
    const previousStatus = targetItem.status;
    
    if (previousStatus === newStatus) return;

    targetItem.status = newStatus;
    targetItem._isSyncing = true;
    targetItem._syncError = null;

    try {
      // 🔥 ARQ: O Backend C# sabe recalcular Streaks e Progresso.
      // Retornamos todo o DTO atualizado do C# e sobrescrevemos a RAM.
      const response = await CompassApi.updateStatus(id, { newStatus });
      Object.assign(entities.value[id], response, { _isSyncing: false });

      toastStore.showToast(`Status alterado para ${newStatus}.`, newStatus === 'COMPLETED' ? 'success' : 'neutral',
        async () => { await updateStatus(id, previousStatus); }
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
    if (!entities.value[id]) return;
    const removedItem = { ...entities.value[id] };

    // 1. OTIMISMO: Remove da Fonte de Verdade e dos Ponteiros imediatamente (UI Latência Zero)
    const newEntities = { ...entities.value };
    delete newEntities[id];
    entities.value = newEntities;
    
    activeIds.value = activeIds.value.filter(i => i !== id);
    databaseIds.value = databaseIds.value.filter(i => i !== id);
    databaseTotal.value = Math.max(0, databaseTotal.value - 1);
    syncHistory();

    // 2. FILA PERSISTENTE (O Desfazer)
    const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';
    
    const operationId = offlineStore.addToQueue({
      url: `${baseUrl}/commitments/${id}`,
      method: 'DELETE',
      payload: null,
      executeAfter: Date.now() + 8000 // Fica retido na fila local (disco) por 8 segundos
    });

    // 3. UX: O Toast com botão "Desfazer"
    toastStore.showToast('Compromisso removido.', 'neutral', () => {
        offlineStore.cancelRequest(operationId);
        
        // Devolve para a memória UI
        entities.value = { ...entities.value, [id]: removedItem };
        activeIds.value.unshift(id);
        databaseIds.value.unshift(id);
        databaseTotal.value += 1;
        syncHistory();
      },
      8000
    );
  };

  return {
    entities,
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