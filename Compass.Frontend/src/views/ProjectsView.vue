<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useCommitmentsStore } from '@/stores/commitmentsStore';
import { useProjectsStore } from '@/stores/projectsStore';
import { useGoalsStore } from '@/stores/goalsStore';
import { useSettingsStore } from '@/stores/settingsStore';
import { useInspectorStore } from '@/stores/inspectorStore';
import { isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';
import { Folder, PlusCircle, Zap, CheckCircle2, Target, ListTodo } from 'lucide-vue-next';
import DefensiveEmptyState from '@/components/core/DefensiveEmptyState.vue';
import PageHeader from '@/components/layout/PageHeader.vue';

const commitmentsStore = useCommitmentsStore();
const projectsStore = useProjectsStore();
const goalsStore = useGoalsStore();
const settingsStore = useSettingsStore();
const inspectorStore = useInspectorStore();

const currentTab = ref<'ACTIVE' | 'COMPLETED'>('ACTIVE');
const viewDensity = computed(() => settingsStore.getViewDensity('projects'));

onMounted(async () => {
  //  ARQ: O ecossistema inteiro é hidratado para garantir o cálculo em Cascata
  goalsStore.loadFromDisk();
  await Promise.all([
    goalsStore.fetchGoals(), 
    commitmentsStore.fetchAllActive(),
    projectsStore.fetchCatalog()
  ]);
});

//  ARQ: A View agora é "burra". Toda a matemática pesada (Bubble-Up) já vem mastigada da Store!
const displayedProjects = computed(() => {
  return projectsStore.enrichedProjects.filter(p => 
    currentTab.value === 'COMPLETED' ? p.status === 'COMPLETED' : p.status !== 'COMPLETED'
  );
});

//  UX: Lookup Relacional O(1) para buscar o nome da Bússola (Meta)
const getGoalName = (goalId: string | null) => {
  if (!goalId) return null;
  return goalsStore.entities[goalId]?.title || 'Meta arquivada ou excluída';
};

const openNewProjectModal = () => {
  isQuickCaptureOpen.value = true;
};

const injectInShift = (projectName: string) => {
  window.dispatchEvent(new CustomEvent('compass:inject-project', { detail: projectName }));
  isQuickCaptureOpen.value = true;
};

const openProjectInspector = (projectId: string) => {
  //  ARQ: Acessa a entidade real no Dicionário O(1)
  const realProject = projectsStore.entities[projectId];
  if (realProject) {
    inspectorStore.openInspector(realProject, 'PROJECT');
  }
};
</script>

<template>
  <div class="max-w-5xl mx-auto space-y-6 select-none pb-12">
    
    <PageHeader 
      title="Projetos & Módulos"
      :badgeCount="projectsStore.enrichedProjects.length"
      badgeLabel="Total"
      description="Veículos de execução. O tempo investido nas tarefas empurra o progresso do Projeto, que por sua vez alimenta a sua Meta estratégica."
      actionLabel="Novo Projeto"
      :actionIcon="PlusCircle"
      @action="openNewProjectModal"
      viewName="projects"
      :showDensityToggle="true"
    />

    <!-- Tabs de Navegação -->
    <div class="flex items-center gap-2 font-mono text-xs border-b border-borderbase pb-2">
      <button
        @click="currentTab = 'ACTIVE'"
        class="px-3 py-1.5 rounded-lg font-semibold transition-colors cursor-pointer"
        :class="currentTab === 'ACTIVE' ? 'bg-surface-active text-content border border-borderfocus' : 'text-content-muted hover:text-content'"
      >
        Em Andamento ({{ projectsStore.enrichedProjects.filter(p => p.status !== 'COMPLETED').length }})
      </button>
      <button
        @click="currentTab = 'COMPLETED'"
        class="px-3 py-1.5 rounded-lg font-semibold transition-colors cursor-pointer"
        :class="currentTab === 'COMPLETED' ? 'bg-surface-active text-content border border-borderfocus' : 'text-content-muted hover:text-content'"
      >
        Concluídos ({{ projectsStore.enrichedProjects.filter(p => p.status === 'COMPLETED').length }})
      </button>
    </div>

    <!-- Empty State Contextualizado -->
    <DefensiveEmptyState
      v-if="displayedProjects.length === 0"
      :icon="Folder"
      :title="currentTab === 'ACTIVE' ? 'Nenhum veículo de execução ativo.' : 'Nenhum projeto concluído ainda.'"
      :explanation="currentTab === 'ACTIVE' 
        ? 'Projetos agrupam tarefas em prol de uma Meta. Crie um projeto para começar a mensurar o tempo total investido nas suas iniciativas.' 
        : 'Quando as tarefas de um projeto totalizarem 100% de conclusão do escopo, ele será arquivado aqui.'"
      action-label="Criar Novo Projeto"
      @action="openNewProjectModal"
    />

    <!-- DataGrid de Projetos Enriquecidos -->
    <div v-else class="border border-borderbase rounded-xl overflow-hidden bg-app transition-all shadow-sm">
      <div 
        class="grid gap-4 bg-surface border-b border-borderbase font-mono font-semibold text-content-muted uppercase tracking-wider"
        :class="viewDensity === 'compact' ? 'grid-cols-8 px-3 py-2 text-[10px]' : 'grid-cols-12 px-4 py-2.5 text-[11px]'"
      >
        <div :class="viewDensity === 'compact' ? 'col-span-5' : 'col-span-4'">Nome do Projeto</div>
        <div v-if="viewDensity === 'detailed'" class="col-span-3 hidden sm:block">Meta Vinculada (Destino)</div>
        <div v-if="viewDensity === 'detailed'" class="col-span-2 hidden md:block text-center">Motor (Tarefas)</div>
        <div :class="viewDensity === 'compact' ? 'col-span-3 text-right' : 'col-span-8 sm:col-span-3 text-right'">Progresso Executado</div>
      </div>

      <transition-group name="list" tag="div" class="divide-y divide-borderbase relative">
        <div 
          v-for="proj in displayedProjects" 
          :key="proj.id"
          @click="openProjectInspector(proj.id)"
          class="grid gap-4 items-center hover:bg-surface-hover transition-colors group relative cursor-pointer"
          :class="[
            viewDensity === 'compact' ? 'grid-cols-8 py-2 px-3' : 'grid-cols-12 py-4 px-4',
            proj.progressPercentage === 100 ? 'bg-status-success-bg/10' : ''
          ]"
        >
          <!-- 1. Nome do Projeto -->
          <div class="flex items-center gap-3 min-w-0" :class="viewDensity === 'compact' ? 'col-span-5' : 'col-span-4'">
            <CheckCircle2 v-if="proj.progressPercentage === 100" class="text-status-success-text flex-shrink-0" :class="viewDensity === 'compact' ? 'w-3.5 h-3.5' : 'w-4 h-4'" />
            <Folder v-else class="text-content-muted flex-shrink-0 group-hover:text-content transition-colors" :class="viewDensity === 'compact' ? 'w-3.5 h-3.5' : 'w-4 h-4'" />
            <span class="font-medium text-content truncate" :class="viewDensity === 'compact' ? 'text-xs' : 'text-sm'">{{ proj.name }}</span>
          </div>

          <!-- 2. Meta Vinculada (Breadcrumb Virtual O(1)) -->
          <div v-if="viewDensity === 'detailed'" class="col-span-3 hidden sm:flex items-center gap-1.5 text-xs text-content-muted truncate font-sans">
            <Target v-if="proj.goalId" class="w-3.5 h-3.5 flex-shrink-0 opacity-70" />
            <span v-if="proj.goalId" class="truncate font-medium">{{ getGoalName(proj.goalId) }}</span>
            <span v-else class="italic opacity-50 text-[11px] font-mono">Nenhuma (Projeto Avulso)</span>
          </div>

          <!-- 3. Contagem de Engrenagens (Tarefas) -->
          <div v-if="viewDensity === 'detailed'" class="col-span-2 hidden md:flex items-center justify-center gap-1.5 text-xs font-mono text-content-muted">
            <ListTodo class="w-3.5 h-3.5 opacity-60" />
            <span>{{ proj.taskCount }} itens</span>
          </div>

          <!-- 4. Progresso Bottom-Up -->
          <div class="flex items-center justify-end gap-3" :class="viewDensity === 'compact' ? 'col-span-3' : 'col-span-8 sm:col-span-3'">
            
            <div v-if="viewDensity === 'detailed'" class="flex flex-col items-end gap-1.5 w-full max-w-[120px]">
              <span class="text-[10px] font-mono text-content-muted tracking-wide">
                <strong class="text-content">{{ proj.completedMinutes }}m</strong> / {{ proj.totalMinutes }}m
              </span>
              <div class="h-1.5 w-full bg-surface-active rounded-full overflow-hidden">
                <div class="h-full bg-content transition-all duration-500 ease-out" :style="{ width: `${proj.progressPercentage}%` }" />
              </div>
            </div>
            <div v-else class="text-[10px] font-mono font-bold text-content-muted">
              {{ proj.progressPercentage }}%
            </div>

            <!-- Injeção Rápida no Turno -->
            <button
              v-if="proj.progressPercentage < 100"
              @click.stop="injectInShift(proj.name)"
              class="rounded bg-surface hover:bg-surface-active border border-borderbase hover:border-borderfocus font-mono text-content transition-all flex items-center justify-center cursor-pointer shadow-sm flex-shrink-0 ml-2"
              :class="viewDensity === 'compact' ? 'w-6 h-6' : 'px-2.5 py-1.5 gap-1.5 text-xs'"
              title="Adicionar tarefa tática neste projeto"
            >
              <Zap class="text-content" :class="viewDensity === 'compact' ? 'w-3 h-3' : 'w-3.5 h-3.5'" />
              <span v-if="viewDensity === 'detailed'" class="hidden lg:inline">+ Tarefa</span>
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
  transition: all 0.3s cubic-bezier(0.25, 1, 0.5, 1);
}
.list-enter-from {
  opacity: 0;
  transform: translateY(10px);
}
.list-leave-to {
  opacity: 0;
  transform: translateX(-20px);
}
</style>