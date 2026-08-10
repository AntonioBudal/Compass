<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import { useGoalsStore } from '@/modules/strategy/stores/goalsStore';
import { useProjectsStore } from '@/modules/strategy/stores/projectsStore';
import { useSettingsStore } from '@/modules/settings/stores/settingsStore';
import { useInspectorStore } from '@/modules/tactical/stores/inspectorStore';
import { Target, PlusCircle, ChevronRight, Folder, CheckCircle2 } from 'lucide-vue-next';
import { isQuickCaptureOpen } from '@/shared/composables/useKeyboardShortcuts';
import DefensiveEmptyState from '@/components/core/DefensiveEmptyState.vue';
import PageHeader from '@/components/layout/PageHeader.vue';
import InspectableCard from '@/components/core/InspectableCard.vue';

const goalsStore = useGoalsStore();
const projectsStore = useProjectsStore();
const settingsStore = useSettingsStore();
const inspectorStore = useInspectorStore();

// Mantém o estado de expansão na sessão para o usuário não perder contexto no F5
const expandedGoalIds = ref<string[]>(JSON.parse(sessionStorage.getItem('compass_expanded_goals') || '[]'));

watch(expandedGoalIds, (newVal) => {
  sessionStorage.setItem('compass_expanded_goals', JSON.stringify(newVal));
}, { deep: true });

const viewDensity = computed(() => settingsStore.getViewDensity('goals'));

onMounted(async () => {
  //  ARQ: Hidratamos Metas e o Catálogo de Projetos para garantir que o Bubble-Up seja infalível, 
  goalsStore.fetchGoals()

  await projectsStore.fetchCatalog();
});

const toggleGoal = (id: string) => {
  const index = expandedGoalIds.value.indexOf(id);
  if (index === -1) expandedGoalIds.value.push(id);
  else expandedGoalIds.value.splice(index, 1);
};

//  UX MESTRA: Navegação Cruzada. Clicar no projeto dentro da meta abre o inspetor do projeto.
const openProjectInspector = (projectId: string) => {
  const realProject = projectsStore.entities[projectId];
  if (realProject) {
    inspectorStore.openInspector(realProject, 'PROJECT');
  }
};
</script>

<template>
  <div class="max-w-4xl mx-auto space-y-6 select-none pb-12">
    
    <PageHeader 
      title="Metas Estratégicas"
      :badgeCount="goalsStore.activeGoals.length"
      badgeLabel="Ativas"
      description="O topo da hierarquia. O progresso estratégico flui automaticamente da base (Projetos e Tarefas) para o topo. Acione o Inspetor com duplo clique."
      actionLabel="Nova Meta"
      :actionIcon="PlusCircle"
      @action="isQuickCaptureOpen = true"
      viewName="goals"
      :showDensityToggle="true"
    />

    <DefensiveEmptyState
      v-if="goalsStore.goals.length === 0"
      :icon="Target"
      :title="!goalsStore.isLoaded ? 'Consultando panorama estratégico...' : 'Nenhuma meta estratégica definida.'"
      :explanation="!goalsStore.isLoaded 
        ? 'Aguarde um momento.' 
        : 'Metas são a bússola do sistema. Sem elas, seus projetos são apenas esteiras de trabalho sem destino final.'"
      action-label="Definir Primeira Meta"
      @action="isQuickCaptureOpen = true"
    />

    <div v-else class="space-y-3">
      <!--  ARQ: O v-for itera sobre `goals` que já vêm mastigados do Computed Enriched da Store -->
      <InspectableCard
        v-for="goal in goalsStore.goals" 
        :key="goal.id"
        :entity="goal"
        type="GOAL"
      >
        <div 
          class="rounded-xl border border-borderbase bg-surface overflow-hidden transition-all duration-200 w-full"
          :class="{ 'border-borderfocus bg-surface-active shadow-md': expandedGoalIds.includes(goal.id) }"
        >
          <!-- HEADER DA META (Sempre Visível) -->
          <div 
            @click="toggleGoal(goal.id)"
            class="w-full flex items-center justify-between gap-4 text-left hover:bg-surface-hover transition-colors cursor-pointer"
            :class="viewDensity === 'compact' ? 'p-3' : 'p-5'"
          >
            <div class="flex items-center gap-3 min-w-0 flex-1">
              <ChevronRight 
                class="text-content-muted transition-transform duration-200 flex-shrink-0"
                :class="[{ 'rotate-90 text-content': expandedGoalIds.includes(goal.id) }, viewDensity === 'compact' ? 'w-4 h-4' : 'w-5 h-5']"
              />
              
              <div class="truncate space-y-0.5 flex-1 min-w-0">
                <h2 
                  @dblclick.stop="inspectorStore.openInspector(goal, 'GOAL')"
                  class="font-semibold text-content truncate flex items-center gap-2 group"
                  :class="viewDensity === 'compact' ? 'text-sm' : 'text-base'"
                  title="Duplo clique para abrir configurações"
                >
                  <span>{{ goal.title }}</span>
                  <span class="text-[9px] font-mono opacity-0 group-hover:opacity-100 text-content-muted transition-opacity" v-if="viewDensity === 'detailed'">[Inspecionar]</span>
                </h2>

                <p v-if="viewDensity === 'detailed'" class="text-[11px] text-content-muted truncate font-mono">
                  Prazo Alvo: {{ goal.targetDate || 'Indefinido' }} | Veículos Atrelados: {{ goal.projectCount || 0 }}
                </p>
              </div>
            </div>

            <!-- PAINEL DIREITO: Progresso e Controle -->
            <div class="flex items-center gap-4 flex-shrink-0 pr-2 sm:pr-6">
              
              <div class="flex flex-col items-end gap-1.5 w-full max-w-[120px]">
                <span class="font-mono text-content-muted tracking-wide" :class="viewDensity === 'compact' ? 'text-[10px]' : 'text-[11px]'">
                  Geral: <strong class="text-content">{{ goal.progressPercentage }}%</strong>
                </span>
                <div v-if="viewDensity === 'detailed'" class="h-1.5 w-24 bg-app rounded-full overflow-hidden hidden sm:block">
                  <div class="h-full bg-content transition-all duration-500 ease-out" :style="{ width: `${goal.progressPercentage}%` }" />
                </div>
              </div>

              <!--  UX: O usuário ainda controla a macro-decisão de fechar/arquivar a meta. -->
              <select
                :value="goal.status"
                @change="(e) => goalsStore.updateGoalStatus(goal.id, (e.target as HTMLSelectElement).value as any)"
                @click.stop
                class="font-mono uppercase bg-surface border border-borderbase rounded text-content cursor-pointer focus:outline-none focus:border-borderfocus"
                :class="viewDensity === 'compact' ? 'text-[9px] px-1.5 py-1' : 'text-[10px] px-2 py-1.5'"
              >
                <option value="ACTIVE">Em Progresso</option>
                <option value="COMPLETED">Batida</option>
                <option value="ARCHIVED">Descartada</option>
              </select>
            </div>
          </div>

          <!-- CORPO EXPANDIDO: O Desdobramento da Meta -->
          <div v-if="expandedGoalIds.includes(goal.id)" class="border-t border-borderbase bg-app/60" :class="viewDensity === 'compact' ? 'px-3 pb-3 pt-2 space-y-3' : 'px-5 pb-5 pt-4 space-y-5'">
            
            <div v-if="viewDensity === 'detailed'" class="p-3.5 rounded bg-surface/80 border border-borderbase text-xs text-content-muted font-sans leading-relaxed">
              <strong class="text-content font-mono uppercase tracking-wider block mb-1">Propósito Estratégico (O Porquê)</strong>
              {{ goal.why || 'Nenhum propósito detalhado. Abra o inspetor para definir a motivação desta meta.' }}
            </div>

            <div class="space-y-2">
              <p class="text-[10px] font-mono uppercase tracking-wider text-content-muted flex items-center gap-1.5">
                <Folder class="w-3.5 h-3.5" />
                <span>Veículos de Execução (Projetos Vinculados)</span>
              </p>

              <!-- Empty State dos Projetos -->
              <div v-if="!goal.projects || goal.projects.length === 0" class="p-4 rounded-lg border border-dashed border-borderbase bg-surface text-center space-y-2">
                <p class="text-xs text-content-muted font-sans">
                  Nenhum projeto associado a esta meta estratégica.
                </p>
                <p class="text-[10px] text-content-muted opacity-70 font-mono uppercase tracking-wider">
                  Vá à tela de Projetos ou use o Inspetor para criar o vínculo.
                </p>
              </div>

              <!-- Lista Relacional de Projetos -->
              <div v-else class="space-y-1.5 pl-2">
                <div 
                  v-for="project in goal.projects" 
                  :key="project.id"
                  @click.stop="openProjectInspector(project.id)"
                  class="flex items-center justify-between gap-4 text-content-muted hover:text-content transition-colors rounded-lg bg-surface border border-borderbase/50 cursor-pointer group"
                  :class="viewDensity === 'compact' ? 'py-1.5 px-2.5' : 'py-2.5 px-3.5'"
                  title="Clique para inspecionar este projeto"
                >
                  <div class="flex items-center gap-2.5 min-w-0">
                    <span class="text-content-muted opacity-50 select-none">├──</span>
                    <CheckCircle2 v-if="project.progressPercentage === 100" class="text-status-success-text flex-shrink-0" :class="viewDensity === 'compact' ? 'w-3 h-3' : 'w-3.5 h-3.5'" />
                    <Folder v-else class="opacity-60 group-hover:opacity-100 transition-opacity flex-shrink-0" :class="viewDensity === 'compact' ? 'w-3 h-3' : 'w-3.5 h-3.5'" />
                    <span class="truncate font-medium text-content" :class="viewDensity === 'compact' ? 'text-[11px]' : 'text-xs'">{{ project.name }}</span>
                  </div>

                  <div class="flex items-center gap-3 flex-shrink-0">
                    <span class="font-mono font-bold text-content" :class="viewDensity === 'compact' ? 'text-[10px]' : 'text-[11px]'">{{ project.progressPercentage }}%</span>
                    <div v-if="viewDensity === 'detailed'" class="h-1.5 w-16 bg-surface-active rounded-full overflow-hidden hidden sm:block">
                      <div class="h-full bg-content transition-all duration-300 ease-out" :style="{ width: `${project.progressPercentage}%` }" />
                    </div>
                  </div>
                </div>
              </div>

            </div>
          </div>
        </div>
      </InspectableCard>
    </div>
  </div>
</template>

<style scoped>
</style>