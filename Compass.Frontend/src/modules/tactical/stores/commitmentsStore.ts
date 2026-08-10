import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { CommitmentsApi } from '@/modules/tactical/api/commitments.api'; //  ARQ: Nova API Modular
import { useToastStore } from '@/shared/stores/toastStore';
import { GlobalHistoryProvider } from '@/shared/utils/autocomplete/providers/HistoryProvider';
import { useOfflineStore } from '@/shared/stores/offlineStore';
import type { 
  CommitmentDto, 
  CreateCommitmentDto, 
  UpdateCommitmentDto,
  CommitmentStatus 
} from '@/shared/types/global';

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
      const data = await CommitmentsApi.getActiveCommitments();
      
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
      const params = new URLSearchParams({
        page: page.toString(),
        limit: limit.toString(),
      });
      if (filters?.search) params.append('search', filters.search);
      if (filters?.type) params.append('type', filters.type);
      if (filters?.status) params.append('status', filters.status);
      if (filters?.projectId) params.append('projectId', filters.projectId);

      const rawDataResponse = await CommitmentsApi.fetchDatabase(params.toString());
      const rawData = rawDataResponse.items || rawDataResponse || [];
      
      const newEntities = { ...entities.value };
      const ids: string[] = [];
      rawData.forEach((item: CommitmentItem) => {
        newEntities[item.id] = item;
        ids.push(item.id);
      });
      
      entities.value = newEntities;
      databaseIds.value = ids;
      databaseTotal.value = rawDataResponse.total || databaseIds.value.length;
      syncHistory();
    } catch (err: any) {
      console.error('[CommitmentsStore] Falha ao buscar database', err);
    } finally {
      isDatabaseLoading.value = false;
    }
  };

  const createCommitment = async (payload: CreateCommitmentDto) => {
    if (payload.type === 'HABIT' && !payload.cronExpression) payload.cronExpression = '0 8 * * *';
    if (payload.type === 'NOTE') payload.estimatedDurationMinutes = 0; 

    const created = await CommitmentsApi.createCommitment(payload);
    
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
    
    entities.value = { ...entities.value, [id]: { ...originalItem, ...payload, _isSyncing: true } };

    try {
      const updated = await CommitmentsApi.updateCommitment(id, payload);
      const safeUpdated = { ...updated };
      if (payload.startTime && !safeUpdated.startTime) safeUpdated.startTime = payload.startTime;
      
      entities.value = { ...entities.value, [id]: { ...entities.value[id], ...safeUpdated, _isSyncing: false } };
      syncHistory();
      if (!isSilent) toastStore.showToast('Compromisso atualizado.', 'neutral');
    } catch (err: any) {
      entities.value = { ...entities.value, [id]: { ...originalItem, _isSyncing: false } };
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
      const response = await CommitmentsApi.updateStatus(id, { newStatus });
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

    const newEntities = { ...entities.value };
    delete newEntities[id];
    entities.value = newEntities;
    
    activeIds.value = activeIds.value.filter(i => i !== id);
    databaseIds.value = databaseIds.value.filter(i => i !== id);
    databaseTotal.value = Math.max(0, databaseTotal.value - 1);
    syncHistory();

    const operationId = offlineStore.addToQueue({
      url: `/commitments/${id}`, // BaseUrl é gerenciado pelo Axios agora
      method: 'DELETE',
      payload: null,
      executeAfter: Date.now() + 8000
    });

    toastStore.showToast('Compromisso removido.', 'neutral', () => {
        offlineStore.cancelRequest(operationId);
        entities.value = { ...entities.value, [id]: removedItem };
        activeIds.value.unshift(id);
        databaseIds.value.unshift(id);
        databaseTotal.value += 1;
        syncHistory();
      }, 8000
    );
  };

  return {
    entities, items, isLoading, globalError, activeCandidates, habitsToday, eventsToday, projectsSummary,
    fetchAllActive, createCommitment, updateCommitment, updateStatus, deleteCommitment, 
    databaseItems, databaseTotal, isDatabaseLoading, fetchDatabase
  };
});