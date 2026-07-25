import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import axios from 'axios';
import { useToastStore } from './toastStore';

// Contrato DTO espelhando o C#
export interface ProjectCatalogItemDto {
  id: string;
  name: string;
  description: string | null;
  lastUsedAtUtc: string | null;
}

const STORAGE_KEY = 'compass_projects_catalog_cache';

export const useProjectsStore = defineStore('projects', () => {
  const toastStore = useToastStore();

  // --- Estado Reativo ---
  const catalog = ref<ProjectCatalogItemDto[]>([]);
  const isLoading = ref<boolean>(false);
  const isServingFromCache = ref<boolean>(false);
  const lastSyncedAt = ref<Date | null>(null);

  // --- Ações de Cache Local ---
  function saveToDisk() {
    try {
      const payload = {
        timestamp: new Date().toISOString(),
        items: catalog.value
      };
      localStorage.setItem(STORAGE_KEY, JSON.stringify(payload));
    } catch (e) {
      console.warn('[ProjectsStore] Falha ao persistir catálogo no disco local.', e);
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
        return true;
      }
    } catch (e) {
      console.warn('[ProjectsStore] Cache de projetos corrompido ou ausente.', e);
    }
    return false;
  }

  // --- Sincronização Principal (Stale-While-Revalidate) ---
  const fetchCatalog = async (forceRefresh = false) => {
    // 1. Se não estamos forçando e a RAM está vazia, tenta hidratar do disco primeiro
    if (catalog.value.length === 0 && !forceRefresh) {
      loadFromDisk();
    }

    isLoading.value = true;
    const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';
    const headers = {
      'X-User-Id': '11111111-1111-1111-1111-111111111111'
    };

    try {
      const res = await axios.get<ProjectCatalogItemDto[]>(`${baseUrl}/projects/catalog`, { 
        headers, 
        timeout: 5000 
      });

      if (res.status === 200) {
        catalog.value = res.data;
        isServingFromCache.value = false;
        lastSyncedAt.value = new Date();
        saveToDisk();
      }
    } catch (err: any) {
      console.warn('[ProjectsStore] Falha na sincronização do catálogo. Ativando modo offline...', err);
      
      const hasOfflineData = catalog.value.length > 0 || loadFromDisk();
      if (hasOfflineData) {
        toastStore.showToast('[OFFLINE] Catálogo de projetos servido da memória local.', 'neutral');
      } else {
        toastStore.showToast('Sem conexão para carregar o catálogo de projetos.', 'error');
      }
    } finally {
      isLoading.value = false;
    }
  };

  // --- Mutações Otimistas in-RAM (Para uso no Quick Capture) ---
  
  /**
   * Promove um projeto para o topo da lista de recentes quando o usuário 
   * o referencia em uma nova tarefa via comando #projeto.
   */
  const promoteUsage = (projectId: string) => {
    const idx = catalog.value.findIndex(p => p.id === projectId);
    if (idx !== -1) {
      const item = catalog.value[idx];
      item.lastUsedAtUtc = new Date().toISOString();
      // Remove da posição atual e insere no topo (LRU)
      catalog.value.splice(idx, 1);
      catalog.value.unshift(item);
      saveToDisk();
    }
  };

  /**
   * Adiciona otimisticamente um projeto recém-criado ao catálogo sem esperar roundtrip do DB.
   */
  const addOptimisticProject = (newProject: ProjectCatalogItemDto) => {
    catalog.value.unshift(newProject);
    saveToDisk();
  };

  // --- Getters Computados ---
  
  // Lista ordenada por último uso para o menu suspenso de sugestões
  const lruProjects = computed(() => {
    return [...catalog.value].sort((a, b) => {
      const timeA = a.lastUsedAtUtc ? new Date(a.lastUsedAtUtc).getTime() : 0;
      const timeB = b.lastUsedAtUtc ? new Date(b.lastUsedAtUtc).getTime() : 0;
      return timeB - timeA;
    });
  });

  return {
    catalog,
    isLoading,
    isServingFromCache,
    lastSyncedAt,
    lruProjects,
    fetchCatalog,
    promoteUsage,
    addOptimisticProject
  };
});