<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useCommitmentsStore } from '@/stores/commitmentsStore';
import { isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';
import { Folder, PlusCircle, Clock, CheckCircle2, Zap, ArrowUpRight } from 'lucide-vue-next';
import DefensiveEmptyState from '@/components/core/DefensiveEmptyState.vue';

const store = useCommitmentsStore();
const currentTab = ref<'ACTIVE' | 'COMPLETED'>('ACTIVE');

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
      if (item.status === 'COMPLETED') {
        existing.completedMinutes += duration;
      }

      existing.progressPercentage = existing.totalMinutes > 0 
        ? Math.round((existing.completedMinutes / existing.totalMinutes) * 100) 
        : 0;

      if (existing.progressPercentage === 100) {
        existing.status = 'COMPLETED';
      } else if (existing.completedMinutes === 0) {
        existing.status = 'PENDING';
      } else {
        existing.status = 'IN_PROGRESS';
      }

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

// Injeta uma tarefa com o hashtag do projeto no turno atual de foco
const injectInShift = (projectName: string) => {
  // Dispara evento para o QuickCapture preencher com a hashtag do projeto
  window.dispatchEvent(new CustomEvent('compass:inject-project', { detail: projectName }));
  isQuickCaptureOpen.value = true;
};
</script>

<template>
  <div class="max-w-5xl mx-auto space-y-6 select-none">
    <!-- Cabeçalho de Projetos -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-4 border-b border-borderbase">
      <div>
        <h1 class="text-2xl font-semibold text-content tracking-tight flex items-center gap-2.5">
          <span>Projetos & Módulos</span>
          <span class="text-xs font-mono bg-surface text-content-muted px-2 py-0.5 rounded border border-borderbase">
            {{ allProjects.length }} Total
          </span>
        </h1>
        <p class="text-sm text-content-muted mt-1">
          Agrupadores táticos orientados a entregas com auditoria de estimativas de tempo (EAI).
        </p>
      </div>

      <button 
        @click="openNewProjectModal"
        class="inline-flex items-center gap-2 px-3 py-1.5 rounded-tactic bg-content text-content-invert hover:opacity-90 text-xs font-semibold font-mono transition-all shadow-sm cursor-pointer self-start sm:self-auto"
      >
        <PlusCircle class="w-3.5 h-3.5" />
        <span>+ Nova Tarefa com Projeto</span>
      </button>
    </div>

    <!-- SELETOR TÁTICO DE ABAS -->
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

    <!-- EMPTY STATE DEFENSIVO (Se não houver projetos na aba) -->
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

    <!-- GRADE DE PROJETOS DE ALTA DENSIDADE -->
    <div v-else class="border border-borderbase rounded-xl overflow-hidden bg-app">
      <div class="grid grid-cols-12 gap-4 px-4 py-2.5 bg-surface border-b border-borderbase text-[11px] font-mono font-semibold text-content-muted uppercase tracking-wider">
        <div class="col-span-5 sm:col-span-4">Nome do Projeto</div>
        <div class="col-span-3 hidden sm:block">Meta Vinculada</div>
        <div class="col-span-2 hidden md:block">Prazo Alvo</div>
        <div class="col-span-7 sm:col-span-3 text-right">Esforço & Ações</div>
      </div>

      <div class="divide-y divide-borderbase">
        <div 
          v-for="proj in displayedProjects" 
          :key="proj.id"
          class="grid grid-cols-12 gap-4 py-3.5 px-4 items-center hover:bg-surface-hover transition-colors group"
        >
          <!-- Coluna 1: Nome -->
          <div class="col-span-5 sm:col-span-4 flex items-center gap-2.5 min-w-0">
            <Folder class="w-4 h-4 text-content-muted flex-shrink-0 group-hover:text-content transition-colors" />
            <span class="text-sm font-medium text-content truncate">{{ proj.name }}</span>
          </div>

          <!-- Coluna 2: Meta -->
          <div class="col-span-3 hidden sm:flex items-center text-xs text-content-muted truncate font-sans">
            <span>{{ proj.linkedGoal }}</span>
          </div>

          <!-- Coluna 3: Prazo -->
          <div class="col-span-2 hidden md:flex items-center text-xs font-mono text-content-muted">
            <span>{{ proj.deadline }}</span>
          </div>

          <!-- Coluna 4: Progresso Horário + Botão Injetar -->
          <div class="col-span-7 sm:col-span-3 flex items-center justify-end gap-3">
            <div class="flex flex-col items-end gap-1">
              <span class="text-xs font-mono text-content-muted">
                <strong class="text-content">{{ proj.completedMinutes }}m</strong> / {{ proj.totalMinutes }}m
              </span>
              <div class="h-1 w-20 bg-surface-active rounded overflow-hidden">
                <div 
                  class="h-full bg-content transition-all duration-300"
                  :style="{ width: `${proj.progressPercentage}%` }"
                />
              </div>
            </div>

            <!-- Botão Tático: [+ Injetar no Turno] -->
            <button
              @click.stop="injectInShift(proj.name)"
              class="px-2 py-1 rounded bg-surface hover:bg-surface-active border border-borderbase hover:border-borderfocus text-xs font-mono text-content transition-all flex items-center gap-1 cursor-pointer shadow-sm"
              title="Criar tarefa rápida associada a este projeto"
            >
              <Zap class="w-3 h-3 text-content" />
              <span class="hidden sm:inline">+ Turno</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>