import { defineStore } from 'pinia';
import { ref, computed, watch } from 'vue';
import axios from 'axios';
import { useToastStore } from './toastStore';
import { ProjectProvider } from '@/utils/autocomplete/AutocompleteEngine';

export interface ProjectCatalogItemDto {
  id: string;
  name: string;
  description: string | null;
  lastUsedAtUtc: string | null;
}

const STORAGE_KEY = 'compass_projects_catalog_cache';

export const useProjectsStore = defineStore('projects', () => {
  const toastStore = useToastStore();

  const catalog = ref<ProjectCatalogItemDto[]>([]);
  const isLoading = ref<boolean>(false);
  const isServingFromCache = ref<boolean>(false);
  const lastSyncedAt = ref<Date | null>(null);

  watch(() => catalog.value, (newCatalog) => {
    if (newCatalog && Array.isArray(newCatalog)) {
      ProjectProvider.syncData(
        newCatalog.map(p => ({
          id: p.id,
          name: p.name,
          lastUsedAtUtc: p.lastUsedAtUtc
        }))
      );
    }
  }, { deep: true, immediate: true });

  function saveToDisk() {
    try {
      const payload = { timestamp: new Date().toISOString(), items: catalog.value };
      localStorage.setItem(STORAGE_KEY, JSON.stringify(payload));
    } catch (e) {
      console.warn('[ProjectsStore] Falha ao persistir no disco local.', e);
    }
  }

  function loadFromDisk(): boolean {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (raw) {
        const parsed = JSON.parse(raw);
        catalog.value = parsed.items || [];
        lastSyncedAt.value = parsed.timestamp ? new Date(parsed.timestamp) : null;
        isServingFromCache.value = true;
        return catalog.value.length > 0;
      }
    } catch (e) {}
    return false;
  }

  const fetchCatalog = async (forceRefresh = false) => {
    if (catalog.value.length === 0 && !forceRefresh) loadFromDisk();

    isLoading.value = true;
    const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';

    try {
      const res = await axios.get<ProjectCatalogItemDto[]>(`${baseUrl}/projects/catalog`, { 
        headers: { 'X-User-Id': '11111111-1111-1111-1111-111111111111' }, 
        timeout: 5000 
      });

      if (res.status === 200) {
        catalog.value = res.data;
        isServingFromCache.value = false;
        lastSyncedAt.value = new Date();
        saveToDisk();
      }
    } catch (err: any) {
      console.warn('[ProjectsStore] Backend indisponível. Ativando modo local...');
      const hasOfflineData = loadFromDisk();
      
      // 🔥 CORREÇÃO: Mock de Sobrevivência para a UI não ficar em branco
      if (!hasOfflineData && catalog.value.length === 0) {
        catalog.value = [
          { id: 'proj-demo-1', name: 'Refatoração da Arquitetura', description: 'Migrar UI e Stores', lastUsedAtUtc: new Date().toISOString() }
        ];
        saveToDisk();
      }
      toastStore.showToast('[OFFLINE] Projetos servidos localmente.', 'neutral');
    } finally {
      isLoading.value = false;
    }
  };

  // 🔥 NOVO MÉTODO (O Conector Universal para o Inspetor)
  const updateProject = async (id: string, payload: any, isSilent: boolean = false) => {
    const index = catalog.value.findIndex(p => p.id === id);
    if (index === -1) return;

    const originalItem = { ...catalog.value[index] };
    Object.assign(catalog.value[index], payload); // Mutação na memória

    try {
      // Futuro: Chamada Axios PUT aqui
      saveToDisk(); // Salva local por enquanto
      if (!isSilent) toastStore.showToast('Projeto atualizado.', 'neutral');
    } catch (err: any) {
      Object.assign(catalog.value[index], originalItem);
      if (!isSilent) toastStore.showToast('Falha na edição. Revertido.', 'error');
      throw err;
    }
  };

  const promoteUsage = (projectId: string) => {
    const idx = catalog.value.findIndex(p => p.id === projectId);
    if (idx !== -1) {
      const item = catalog.value[idx];
      item.lastUsedAtUtc = new Date().toISOString();
      catalog.value.splice(idx, 1);
      catalog.value.unshift(item);
      saveToDisk();
    }
  };

  const addOptimisticProject = (newProject: ProjectCatalogItemDto) => {
    catalog.value.unshift(newProject);
    saveToDisk();
  };

  const lruProjects = computed(() => {
    return [...catalog.value].sort((a, b) => {
      const timeA = a.lastUsedAtUtc ? new Date(a.lastUsedAtUtc).getTime() : 0;
      const timeB = b.lastUsedAtUtc ? new Date(b.lastUsedAtUtc).getTime() : 0;
      return timeB - timeA;
    });
  });

  const createProject = async (projectName: string): Promise<ProjectCatalogItemDto> => {
    const tempId = `temp-${Date.now()}`;
    const optimisticProject: ProjectCatalogItemDto = {
      id: tempId, name: projectName, description: null, lastUsedAtUtc: new Date().toISOString()
    };
    addOptimisticProject(optimisticProject);
    return optimisticProject; // Retorna mock direto para não quebrar a UI
  };

  return {
    catalog, isLoading, isServingFromCache, lastSyncedAt, lruProjects,
    fetchCatalog, promoteUsage, addOptimisticProject, createProject,
    updateProject 
  };
});