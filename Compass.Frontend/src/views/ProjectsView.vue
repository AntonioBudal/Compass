<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useCommitmentsStore } from '@/stores/commitmentsStore';
import { useProjectsStore } from '@/stores/projectsStore';
import { useSettingsStore } from '@/stores/settingsStore';
import { useInspectorStore } from '@/stores/inspectorStore'; // 🔥 ARQ-00
import { isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';
import { Folder, PlusCircle, Zap, CheckCircle2 } from 'lucide-vue-next';
import DefensiveEmptyState from '@/components/core/DefensiveEmptyState.vue';
import PageHeader from '@/components/layout/PageHeader.vue';

const commitmentsStore = useCommitmentsStore();
const projectsStore = useProjectsStore();
const settingsStore = useSettingsStore();
const inspectorStore = useInspectorStore(); // 🔥 ARQ-00

const currentTab = ref<'ACTIVE' | 'COMPLETED'>('ACTIVE');
const viewDensity = computed(() => settingsStore.getViewDensity('projects'));

onMounted(async () => {
  await Promise.all([
    commitmentsStore.fetchAllActive(),
    projectsStore.fetchCatalog()
  ]);
});

interface ProjectSummary {
  id: string;
  name: string;
  linkedGoal: string | null;
  deadline: string | null;
  completedMinutes: number;
  totalMinutes: number;
  progressPercentage: number;
  status: 'IN_PROGRESS' | 'PENDING' | 'COMPLETED';
}

const allProjects = computed<ProjectSummary[]>(() => {
  // Blindagem defensiva contra falhas de rede/inicialização nula
  if (!Array.isArray(projectsStore.catalog)) return [];

  return projectsStore.catalog.map(project => {
    const relatedTasks = commitmentsStore.items.filter(item => item.projectId === project.id);
    
    let totalMinutes = 0;
    let completedMinutes = 0;

    relatedTasks.forEach(task => {
      const duration = task.estimatedDurationMinutes || 30;
      totalMinutes += duration;
      if (task.status === 'COMPLETED') {
        completedMinutes += duration;
      }
    });

    const progressPercentage = totalMinutes > 0 
      ? Math.round((completedMinutes / totalMinutes) * 100) 
      : 0;

    let status: 'PENDING' | 'IN_PROGRESS' | 'COMPLETED' = 'PENDING';
    if (totalMinutes > 0 && progressPercentage === 100) {
      status = 'COMPLETED';
    } else if (completedMinutes > 0) {
      status = 'IN_PROGRESS';
    }

    return {
      id: project.id,
      name: project.name,
      linkedGoal: project.description || null, // Hack temporário de UX
      deadline: null, 
      completedMinutes,
      totalMinutes,
      progressPercentage,
      status
    };
  });
});

const displayedProjects = computed(() => {
  return allProjects.value.filter(p => 
    currentTab.value === 'COMPLETED' ? p.status === 'COMPLETED' : p.status !== 'COMPLETED'
  );
});

const openNewProjectModal = () => {
  isQuickCaptureOpen.value = true;
};

const injectInShift = (projectName: string) => {
  window.dispatchEvent(new CustomEvent('compass:inject-project', { detail: projectName }));
  isQuickCaptureOpen.value = true;
};

// 🔥 ARQ-00: Função para buscar a Entidade Real no catálogo e enviá-la ao Inspetor
const openProjectInspector = (projectId: string) => {
  const realProject = projectsStore.catalog.find(p => p.id === projectId);
  if (realProject) {
    inspectorStore.openInspector(realProject, 'PROJECT');
  }
};
</script>

<template>
  <div class="max-w-5xl mx-auto space-y-6 select-none">
    
    <PageHeader 
      title="Projetos & Módulos"
      :badgeCount="allProjects.length"
      badgeLabel="Total"
      description="Agrupadores táticos orientados a entregas com auditoria de estimativas de tempo (EAI)."
      actionLabel="Nova Tarefa"
      :actionIcon="PlusCircle"
      @action="openNewProjectModal"
      viewName="projects"
      :showDensityToggle="true"
    />

    <div class="flex items-center gap-2 font-mono text-xs border-b border-borderbase pb-2">
      <button
        @click="currentTab = 'ACTIVE'"
        class="px-3 py-1.5 rounded-lg font-semibold transition-colors cursor-pointer"
        :class="currentTab === 'ACTIVE' ? 'bg-surface-active text-content border border-borderfocus' : 'text-content-muted hover:text-content'"
      >
        Em Andamento ({{ allProjects.filter(p => p.status !== 'COMPLETED').length }})
      </button>
      <button
        @click="currentTab = 'COMPLETED'"
        class="px-3 py-1.5 rounded-lg font-semibold transition-colors cursor-pointer"
        :class="currentTab === 'COMPLETED' ? 'bg-surface-active text-content border border-borderfocus' : 'text-content-muted hover:text-content'"
      >
        Concluídos ({{ allProjects.filter(p => p.status === 'COMPLETED').length }})
      </button>
    </div>

    <DefensiveEmptyState
      v-if="displayedProjects.length === 0"
      :icon="Folder"
      :title="currentTab === 'ACTIVE' ? 'Nenhum projeto em andamento.' : 'Nenhum projeto concluído ainda.'"
      :explanation="currentTab === 'ACTIVE' 
        ? 'Ao criar uma tarefa utilizando a hashtag #nome-do-projeto no Quick Capture, ela agrupará o tempo estimado nesta tela.' 
        : 'Projetos que atingirem 100% de conclusão de tempo útil serão arquivados nesta coluna.'"
      action-label="Criar Tarefa com Projeto (Cmd+K)"
      @action="openNewProjectModal"
    />

    <div v-else class="border border-borderbase rounded-xl overflow-hidden bg-app transition-all">
      <div 
        class="grid gap-4 bg-surface border-b border-borderbase font-mono font-semibold text-content-muted uppercase tracking-wider"
        :class="viewDensity === 'compact' ? 'grid-cols-8 px-3 py-2 text-[10px]' : 'grid-cols-12 px-4 py-2.5 text-[11px]'"
      >
        <div :class="viewDensity === 'compact' ? 'col-span-5' : 'col-span-5 sm:col-span-4'">Nome do Projeto</div>
        <div v-if="viewDensity === 'detailed'" class="col-span-3 hidden sm:block">Meta Vinculada</div>
        <div v-if="viewDensity === 'detailed'" class="col-span-2 hidden md:block">Prazo Alvo</div>
        <div :class="viewDensity === 'compact' ? 'col-span-3 text-right' : 'col-span-7 sm:col-span-3 text-right'">Progresso</div>
      </div>

      <transition-group name="list" tag="div" class="divide-y divide-borderbase relative">
        <div 
          v-for="proj in displayedProjects" 
          :key="proj.id"
          @click="openProjectInspector(proj.id)"
          class="grid gap-4 items-center hover:bg-surface-hover transition-colors group relative cursor-pointer"
          :class="[
            viewDensity === 'compact' ? 'grid-cols-8 py-1.5 px-3' : 'grid-cols-12 py-3.5 px-4',
            proj.progressPercentage === 100 ? 'bg-status-success-bg/10' : ''
          ]"
        >
          <div class="flex items-center gap-2.5 min-w-0" :class="viewDensity === 'compact' ? 'col-span-5' : 'col-span-5 sm:col-span-4'">
            <CheckCircle2 v-if="proj.progressPercentage === 100" class="text-status-success-text flex-shrink-0" :class="viewDensity === 'compact' ? 'w-3 h-3' : 'w-4 h-4'" />
            <Folder v-else class="text-content-muted flex-shrink-0 group-hover:text-content transition-colors" :class="viewDensity === 'compact' ? 'w-3 h-3' : 'w-4 h-4'" />
            <span class="font-medium text-content truncate" :class="viewDensity === 'compact' ? 'text-xs' : 'text-sm'">{{ proj.name }}</span>
          </div>

          <div v-if="viewDensity === 'detailed'" class="col-span-3 hidden sm:flex items-center text-xs text-content-muted truncate font-sans">
            <span :class="{'italic opacity-50': !proj.linkedGoal}">{{ proj.linkedGoal || 'Sem meta' }}</span>
          </div>

          <div v-if="viewDensity === 'detailed'" class="col-span-2 hidden md:flex items-center text-xs font-mono text-content-muted">
            <span :class="{'italic opacity-50': !proj.deadline}">{{ proj.deadline || 'S/ Prazo' }}</span>
          </div>

          <div class="flex items-center justify-end gap-3" :class="viewDensity === 'compact' ? 'col-span-3' : 'col-span-7 sm:col-span-3'">
            
            <div v-if="viewDensity === 'detailed'" class="flex flex-col items-end gap-1">
              <span class="text-xs font-mono text-content-muted">
                <strong class="text-content">{{ proj.completedMinutes }}m</strong> / {{ proj.totalMinutes }}m
              </span>
              <div class="h-1 w-20 bg-surface-active rounded overflow-hidden">
                <div class="h-full bg-content transition-all duration-300" :style="{ width: `${proj.progressPercentage}%` }" />
              </div>
            </div>
            <div v-else class="text-[10px] font-mono font-bold text-content-muted">
              {{ proj.progressPercentage }}%
            </div>

            <button
              v-if="proj.progressPercentage < 100"
              @click.stop="injectInShift(proj.name)"
              class="rounded bg-surface hover:bg-surface-active border border-borderbase hover:border-borderfocus font-mono text-content transition-all flex items-center justify-center cursor-pointer shadow-sm"
              :class="viewDensity === 'compact' ? 'w-6 h-6' : 'px-2 py-1 gap-1 text-xs'"
              title="Criar tarefa rápida associada a este projeto"
            >
              <Zap class="text-content" :class="viewDensity === 'compact' ? 'w-3 h-3' : 'w-3 h-3'" />
              <span v-if="viewDensity === 'detailed'" class="hidden sm:inline">+ Turno</span>
            </button>
          </div>
        </div>
      </transition-group>
    </div>
  </div>
</template>

<style scoped>
.list-enter-active,
.list-leave-active {
  transition: all 0.4s cubic-bezier(0.25, 1, 0.5, 1);
}
.list-enter-from {
  opacity: 0;
  transform: translateY(15px);
}
.list-leave-to {
  opacity: 0;
  transform: translateX(-30px);
}
</style>