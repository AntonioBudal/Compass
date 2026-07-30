<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useCommitmentsStore } from '@/stores/commitmentsStore';
import { useSettingsStore } from '@/stores/settingsStore';
import { isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';
import { Folder, PlusCircle, Zap } from 'lucide-vue-next';
import DefensiveEmptyState from '@/components/core/DefensiveEmptyState.vue';
import PageHeader from '@/components/layout/PageHeader.vue';

const store = useCommitmentsStore();
const settingsStore = useSettingsStore();

const currentTab = ref<'ACTIVE' | 'COMPLETED'>('ACTIVE');

// Reatividade de Densidade (Compacto vs Detalhado)
const viewDensity = computed(() => settingsStore.getViewDensity('projects'));

onMounted(() => {
  store.fetchAllActive();
});

interface ProjectSummary {
  id: string;
  name: string;
  linkedGoal: string;
  deadline: string;
  completedMinutes: number;
  totalMinutes: number;
  progressPercentage: number;
  status: 'IN_PROGRESS' | 'PENDING' | 'COMPLETED';
}

const allProjects = computed<ProjectSummary[]>(() => {
  const map = new Map<string, ProjectSummary>();

  store.items.forEach(item => {
    if (item.projectName) {
      const key = item.projectName;
      const existing = map.get(key) || {
        id: `proj-${key}`,
        name: key,
        linkedGoal: '🎯 Q3 Launch MVP',
        deadline: item.deadline ? new Date(item.deadline).toLocaleDateString() : '30/08/2026',
        completedMinutes: 0,
        totalMinutes: 0,
        progressPercentage: 0,
        status: 'IN_PROGRESS'
      };

      const duration = item.estimatedDurationMinutes || 30;
      existing.totalMinutes += duration;
      if (item.status === 'COMPLETED') existing.completedMinutes += duration;

      existing.progressPercentage = existing.totalMinutes > 0 
        ? Math.round((existing.completedMinutes / existing.totalMinutes) * 100) 
        : 0;

      if (existing.progressPercentage === 100) existing.status = 'COMPLETED';
      else if (existing.completedMinutes === 0) existing.status = 'PENDING';
      else existing.status = 'IN_PROGRESS';

      map.set(key, existing);
    }
  });

  return Array.from(map.values());
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
</script>

<template>
  <div class="max-w-5xl mx-auto space-y-6 select-none">
    
    <!-- 1. CABEÇALHO UNIVERSAL COM DENSITY TOGGLE -->
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

    <!-- 2. SELETOR TÁTICO DE ABAS -->
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

    <!-- 3. EMPTY STATE DEFENSIVO -->
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

    <!-- 4. GRADE DE PROJETOS REATIVA (Densidade) -->
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

      <div class="divide-y divide-borderbase">
        <div 
          v-for="proj in displayedProjects" 
          :key="proj.id"
          class="grid gap-4 items-center hover:bg-surface-hover transition-colors group"
          :class="viewDensity === 'compact' ? 'grid-cols-8 py-1.5 px-3' : 'grid-cols-12 py-3.5 px-4'"
        >
          <!-- Nome -->
          <div class="flex items-center gap-2.5 min-w-0" :class="viewDensity === 'compact' ? 'col-span-5' : 'col-span-5 sm:col-span-4'">
            <Folder class="text-content-muted flex-shrink-0 group-hover:text-content transition-colors" :class="viewDensity === 'compact' ? 'w-3 h-3' : 'w-4 h-4'" />
            <span class="font-medium text-content truncate" :class="viewDensity === 'compact' ? 'text-xs' : 'text-sm'">{{ proj.name }}</span>
          </div>

          <!-- Meta (Somente Detalhado) -->
          <div v-if="viewDensity === 'detailed'" class="col-span-3 hidden sm:flex items-center text-xs text-content-muted truncate font-sans">
            <span>{{ proj.linkedGoal }}</span>
          </div>

          <!-- Prazo (Somente Detalhado) -->
          <div v-if="viewDensity === 'detailed'" class="col-span-2 hidden md:flex items-center text-xs font-mono text-content-muted">
            <span>{{ proj.deadline }}</span>
          </div>

          <!-- Progresso + Botão -->
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

            <!-- Botão Tático -->
            <button
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
      </div>
    </div>
  </div>
</template>