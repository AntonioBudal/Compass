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

  //  ARQUITETURA NORMALIZADA
  // O Dicionário (Single Source of Truth) e a Lista de Ponteiros
  const entities = ref<Record<string, ProjectCatalogItemDto>>({});
  const catalogIds = ref<string[]>([]);

  const isLoading = ref<boolean>(false);
  const isServingFromCache = ref<boolean>(false);
  const lastSyncedAt = ref<Date | null>(null);

  //  COMPUTED BRIDGE: A View continua acessando `.catalog` como se fosse um array!
  const catalog = computed(() => catalogIds.value.map(id => entities.value[id]).filter(Boolean));

  watch(() => catalog.value, (newCatalog) => {
    if (newCatalog && newCatalog.length > 0) {
      ProjectProvider.syncData(
        newCatalog.map(p => ({
          id: p.id,
          name: p.name,
          lastUsedAtUtc: p.lastUsedAtUtc
        }))
      );
    }
  }, { deep: true, immediate: true });

  // --- MOTOR DE CACHE LOCAL (PURIFICADO) ---
  
  function saveToDisk() {
    try {
      // Salvamos a projeção em array para manter compatibilidade reversa caso exista cache velho
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
        const cachedItems: ProjectCatalogItemDto[] = parsed.items || [];
        
        // Normaliza os dados lidos do disco
        const newEntities: Record<string, ProjectCatalogItemDto> = {};
        const newIds: string[] = [];
        
        cachedItems.forEach(p => {
          newEntities[p.id] = p;
          newIds.push(p.id);
        });

        entities.value = newEntities;
        catalogIds.value = newIds;
        
        lastSyncedAt.value = parsed.timestamp ? new Date(parsed.timestamp) : null;
        isServingFromCache.value = true;
        return catalogIds.value.length > 0;
      }
    } catch (e) {
      console.warn('[ProjectsStore] Cache corrompido. Ignorando...', e);
    }
    return false;
  }

  // --- SINCRONIZAÇÃO COM O BACKEND ---

  const fetchCatalog = async (forceRefresh = false) => {
    if (catalogIds.value.length === 0 && !forceRefresh) loadFromDisk();

    isLoading.value = true;
    const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';

    try {
      const res = await axios.get<ProjectCatalogItemDto[]>(`${baseUrl}/projects/catalog`, { 
        headers: { 'X-User-Id': '11111111-1111-1111-1111-111111111111' }, 
        timeout: 5000 
      });

      if (res.status === 200) {
        //  ANIQUILAÇÃO DE ZUMBIS: Substituímos o estado local 100% pelo que o servidor mandou.
        // Projetos apagados no server deixarão de existir aqui automaticamente.
        const newEntities: Record<string, ProjectCatalogItemDto> = {};
        const newIds: string[] = [];
        
        res.data.forEach(p => {
          newEntities[p.id] = p;
          newIds.push(p.id);
        });

        entities.value = newEntities;
        catalogIds.value = newIds;
        
        isServingFromCache.value = false;
        lastSyncedAt.value = new Date();
        saveToDisk(); // Atualiza o disco com a verdade absoluta
      }
    } catch (err: any) {
      console.warn('[ProjectsStore] Backend indisponível. Ativando modo local...');
      const hasOfflineData = loadFromDisk();
      
      // Mock de Sobrevivência (Se for a primeira vez rodando sem backend)
      if (!hasOfflineData && catalogIds.value.length === 0) {
        const demoId = 'proj-demo-1';
        entities.value[demoId] = { id: demoId, name: 'Refatoração da Arquitetura', description: 'Migrar UI e Stores', lastUsedAtUtc: new Date().toISOString() };
        catalogIds.value.push(demoId);
        saveToDisk();
      }
      toastStore.showToast('[OFFLINE] Projetos servidos localmente.', 'neutral');
    } finally {
      isLoading.value = false;
    }
  };

  // --- MUTAÇÕES OTIMISTAS O(1) ---

  const updateProject = async (id: string, payload: any, isSilent: boolean = false) => {
    if (!entities.value[id]) return;

    const originalItem = { ...entities.value[id] };
    
    //  Mutação direta no Dicionário O(1)
    Object.assign(entities.value[id], payload); 

    try {
      // Futuro: await CompassApi.updateProject(id, payload);
      saveToDisk(); 
      if (!isSilent) toastStore.showToast('Projeto atualizado.', 'neutral');
    } catch (err: any) {
      Object.assign(entities.value[id], originalItem);
      if (!isSilent) toastStore.showToast('Falha na edição. Revertido.', 'error');
      throw err;
    }
  };

  const promoteUsage = (projectId: string) => {
    if (!entities.value[projectId]) return;
    
    entities.value[projectId].lastUsedAtUtc = new Date().toISOString();
    
    // Move o ponteiro para o topo (LRU) sem mexer no objeto em si
    catalogIds.value = catalogIds.value.filter(id => id !== projectId);
    catalogIds.value.unshift(projectId);
    
    saveToDisk();
  };

  const addOptimisticProject = (newProject: ProjectCatalogItemDto) => {
    entities.value[newProject.id] = newProject;
    catalogIds.value.unshift(newProject.id);
    saveToDisk();
  };

  const createProject = async (projectName: string): Promise<ProjectCatalogItemDto> => {
    const tempId = `temp-${Date.now()}`;
    const optimisticProject: ProjectCatalogItemDto = {
      id: tempId, name: projectName, description: null, lastUsedAtUtc: new Date().toISOString()
    };
    
    addOptimisticProject(optimisticProject);
    return optimisticProject; // Mock temporário até termos o endpoint C#
  };

  // Getter de ordenação para o dropdown (mantém a lógica original intacta)
  const lruProjects = computed(() => {
    return [...catalog.value].sort((a, b) => {
      const timeA = a.lastUsedAtUtc ? new Date(a.lastUsedAtUtc).getTime() : 0;
      const timeB = b.lastUsedAtUtc ? new Date(b.lastUsedAtUtc).getTime() : 0;
      return timeB - timeA;
    });
  });

  return {
    entities,
    catalog, 
    isLoading, 
    isServingFromCache, 
    lastSyncedAt, 
    lruProjects,
    fetchCatalog, 
    promoteUsage, 
    addOptimisticProject, 
    createProject,
    updateProject 
  };
});