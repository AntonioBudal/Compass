import { defineStore } from 'pinia';
import { ref, computed, watch } from 'vue';
import { useToastStore } from '../../../shared/stores/toastStore';
import { useCommitmentsStore } from '@/modules/tactical/stores/commitmentsStore'; //  ARQ: Integração Bottom-Up
import { ProjectProvider } from '@/shared/utils/autocomplete/AutocompleteEngine';
import { ProjectsApi } from '@/modules/strategy/api/projects.api'
import { apiClient } from '@/shared/api/client';
//  NOVO DTO: Agora possui GoalId
export interface ProjectCatalogItemDto {
  id: string;
  name: string;
  description: string | null;
  goalId: string | null; 
  lastUsedAtUtc: string | null;
}

const STORAGE_KEY = 'compass_projects_catalog_cache';

export const useProjectsStore = defineStore('projects', () => {
  const toastStore = useToastStore();
  const commitmentsStore = useCommitmentsStore();

  //  ARQUITETURA NORMALIZADA O(1)
  const entities = ref<Record<string, ProjectCatalogItemDto>>({});
  const catalogIds = ref<string[]>([]);

  const isLoading = ref<boolean>(false);
  const isServingFromCache = ref<boolean>(false);
  const lastSyncedAt = ref<Date | null>(null);

  // Getter bruto
  const catalog = computed(() => catalogIds.value.map(id => entities.value[id]).filter(Boolean));

  //  MÁGICA BOTTOM-UP: Este computed funde Projetos + Tarefas em tempo real
  const enrichedProjects = computed(() => {
    const allTasks = Object.values(commitmentsStore.entities).filter(t => t.type === 'TASK');
    
    return catalog.value.map(project => {
      const projTasks = allTasks.filter(t => t.projectId === project.id);
      
      let totalMinutes = 0;
      let completedMinutes = 0;

      projTasks.forEach(task => {
        const duration = task.estimatedDurationMinutes || 30;
        totalMinutes += duration;
        if (task.status === 'COMPLETED') {
          completedMinutes += duration;
        }
      });

      const progressPercentage = totalMinutes > 0 ? Math.round((completedMinutes / totalMinutes) * 100) : 0;
      let status: 'PENDING' | 'IN_PROGRESS' | 'COMPLETED' = 'PENDING';
      
      if (totalMinutes > 0 && progressPercentage === 100) status = 'COMPLETED';
      else if (completedMinutes > 0) status = 'IN_PROGRESS';

      return {
        ...project,
        totalMinutes,
        completedMinutes,
        progressPercentage,
        status,
        taskCount: projTasks.length
      };
    });
  });

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

  // --- MOTOR DE CACHE ---
  

  
  const fetchCatalog = async (forceRefresh = false) => {
    

    isLoading.value = true;
    const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';

    try {
      const res = await apiClient.get<ProjectCatalogItemDto[]>(`${baseUrl}/projects/catalog`, { 
        headers: { 'X-User-Id': '11111111-1111-1111-1111-111111111111' }, 
        timeout: 5000 
      });

      if (res.status === 200) {
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
   
      }
    } catch (err: any) {
      const hasOfflineData = 1
      if (!hasOfflineData && catalogIds.value.length === 0) {
        const demoId = 'proj-demo-1';
        // Mock adaptado para o novo DTO
       entities.value[demoId] = { 
        id: demoId, 
        name: 'Refatoração da Arquitetura', 
        description: null, 
        goalId: null, 
        lastUsedAtUtc: new Date().toISOString() 
      };
       
      }
    } finally {
      isLoading.value = false;
    }
  };

  const updateProject = async (id: string, payload: Partial<ProjectCatalogItemDto>, isSilent: boolean = false) => {
    if (!entities.value[id]) return;
    const originalItem = { ...entities.value[id] };
    Object.assign(entities.value[id], payload); 

    try {
      
      await ProjectsApi.updateProject(id, { name: entities.value[id].name, goalId: entities.value[id].goalId });
      
      if (!isSilent) toastStore.showToast('Projeto atualizado e sincronizado.', 'neutral');
    } catch (err: any) {
      Object.assign(entities.value[id], originalItem);
      if (!isSilent) toastStore.showToast('Falha na edição. Revertido.', 'error');
      throw err;
    }
  };

  const deleteProject = async (id: string) => {
    if (!entities.value[id]) return;
    
    // Otimismo Visual
    const projectToDelete = { ...entities.value[id] };
    delete entities.value[id];
    catalogIds.value = catalogIds.value.filter(pId => pId !== id);

    try {
      
      await ProjectsApi.deleteProject(id);
    
      toastStore.showToast('Projeto excluído com sucesso.', 'neutral');
    } catch (err) {
      // Rollback se falhar
      entities.value[id] = projectToDelete;
      catalogIds.value.unshift(id);
      toastStore.showToast('Falha ao excluir o projeto.', 'error');
      throw err;
    }
  };

  const promoteUsage = (projectId: string) => {
    if (!entities.value[projectId]) return;
    entities.value[projectId].lastUsedAtUtc = new Date().toISOString();
    
    catalogIds.value = catalogIds.value.filter(id => id !== projectId);
    catalogIds.value.unshift(projectId);

  };

  const lruProjects = computed(() => {
    return [...catalog.value].sort((a, b) => {
      const timeA = a.lastUsedAtUtc ? new Date(a.lastUsedAtUtc).getTime() : 0;
      const timeB = b.lastUsedAtUtc ? new Date(b.lastUsedAtUtc).getTime() : 0;
      return timeB - timeA;
    });
  });

  const createProject = async (projectName: string): Promise<ProjectCatalogItemDto> => {
    // 🚀 Bate na API que gera o Guid real
    const created = await ProjectsApi.createProject({ name: projectName, goalId: null });
    
    entities.value[created.id] = created;
    catalogIds.value.unshift(created.id);

    
    return created;
  };

  return {
    entities,
    catalog,
    enrichedProjects, 
    isLoading, 
    lruProjects,
    fetchCatalog, 
    promoteUsage,
    deleteProject,
    createProject,
    updateProject 
  };
});