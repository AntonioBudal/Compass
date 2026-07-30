<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useGoalsStore } from '@/stores/goalsStore';
import { useSettingsStore } from '@/stores/settingsStore';
import { Target, PlusCircle, ChevronRight, Plus } from 'lucide-vue-next';
import { isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';
import DefensiveEmptyState from '@/components/core/DefensiveEmptyState.vue';
import PageHeader from '@/components/layout/PageHeader.vue';
import InspectableCard from '@/components/core/InspectableCard.vue';

const goalsStore = useGoalsStore();
const settingsStore = useSettingsStore();

const expandedGoalIds = ref<string[]>(['goal-1']);
const editingGoalId = ref<string | null>(null);
const editingTitleText = ref('');
const newModuleName = ref('');
const addingModuleGoalId = ref<string | null>(null);

// Reatividade de Densidade (Compacto vs Detalhado)
const viewDensity = computed(() => settingsStore.getViewDensity('goals'));

onMounted(() => {
  goalsStore.loadFromDisk();
});

const toggleGoal = (id: string) => {
  const index = expandedGoalIds.value.indexOf(id);
  if (index === -1) expandedGoalIds.value.push(id);
  else expandedGoalIds.value.splice(index, 1);
};

const startEditingTitle = (id: string, currentTitle: string, e: Event) => {
  e.stopPropagation();
  editingGoalId.value = id;
  editingTitleText.value = currentTitle;
};

const saveEditedTitle = (id: string) => {
  if (editingGoalId.value === id) {
    goalsStore.updateGoalTitle(id, editingTitleText.value);
    editingGoalId.value = null;
  }
};

const handleAddModule = (goalId: string) => {
  if (newModuleName.value.trim()) {
    goalsStore.addChildModule(goalId, newModuleName.value);
    newModuleName.value = '';
    addingModuleGoalId.value = null;
  }
};
</script>

<template>
  <div class="max-w-4xl mx-auto space-y-6 select-none">
    
    <!-- CABEÇALHO UNIVERSAL COM DENSITY TOGGLE -->
    <PageHeader 
      title="Metas Estratégicas"
      :badgeCount="goalsStore.activeGoals.length"
      badgeLabel="Ativas"
      description="Guarda-chuvas estratégicos com medição de progresso. Acione o Inspetor com duplo clique."
      actionLabel="Nova Meta"
      :actionIcon="PlusCircle"
      @action="isQuickCaptureOpen = true"
      viewName="goals"
      :showDensityToggle="true"
    />

    <!-- EMPTY STATE -->
    <DefensiveEmptyState
      v-if="goalsStore.goals.length === 0"
      :icon="Target"
      title="Nenhuma meta estratégica definida."
      explanation="Metas conectam tarefas diárias a objetivos de longo prazo. Sem metas, o Now Engine pondera apenas urgências."
      action-label="Criar Primeira Meta (Cmd+K)"
      @action="isQuickCaptureOpen = true"
    />

    <!-- STRATEGIC TREE ACCORDION -->
    <div v-else class="space-y-3">
      <!-- 🚀 Injetando o InspectableCard ao redor da Meta para permitir o Lápis/Duplo clique -->
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
          <!-- Título Expansível -->
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
                <input
                  v-if="editingGoalId === goal.id"
                  v-model="editingTitleText"
                  type="text"
                  @click.stop
                  @keydown.enter="saveEditedTitle(goal.id)"
                  @blur="saveEditedTitle(goal.id)"
                  class="w-full bg-surface px-2 py-0.5 rounded border border-borderfocus font-semibold text-content focus:outline-none"
                  :class="viewDensity === 'compact' ? 'text-sm' : 'text-base'"
                  autofocus
                />
                <h2 
                  v-else 
                  @dblclick="startEditingTitle(goal.id, goal.title, $event)"
                  class="font-semibold text-content truncate flex items-center gap-2 group"
                  :class="viewDensity === 'compact' ? 'text-sm' : 'text-base'"
                >
                  <span>{{ goal.title }}</span>
                  <span class="text-[9px] font-mono opacity-0 group-hover:opacity-100 text-content-muted" v-if="viewDensity === 'detailed'">[Inline Edit]</span>
                </h2>

                <p v-if="viewDensity === 'detailed'" class="text-[11px] text-content-muted truncate font-mono">
                  Prazo: {{ goal.targetDate }} | Módulos: {{ goal.children?.length || 0 }}
                </p>
              </div>
            </div>

            <!-- Medidor de Progresso da Meta -->
            <div class="flex items-center gap-4 flex-shrink-0 pr-6"> <!-- pr-6 para não colar no botão de Lápis do wrapper -->
              <div class="flex flex-col items-end gap-1">
                <span class="font-mono text-content-muted" :class="viewDensity === 'compact' ? 'text-[10px]' : 'text-xs'">Progresso: <strong class="text-content">{{ goal.progressPercentage }}%</strong></span>
                <div v-if="viewDensity === 'detailed'" class="h-1.5 w-24 bg-app rounded-full overflow-hidden">
                  <div class="h-full bg-content transition-all duration-500" :style="{ width: `${goal.progressPercentage}%` }" />
                </div>
              </div>

              <!-- Seletor Rápido de Estado da Meta -->
              <select
                :value="goal.status"
                @change="(e) => goalsStore.updateGoalStatus(goal.id, (e.target as HTMLSelectElement).value as any)"
                @click.stop
                class="font-mono uppercase bg-surface border border-borderbase rounded text-content cursor-pointer focus:outline-none"
                :class="viewDensity === 'compact' ? 'text-[9px] px-1 py-0.5' : 'text-[11px] px-2 py-1'"
              >
                <option value="ACTIVE">ACTIVE</option>
                <option value="COMPLETED">COMPLETED</option>
                <option value="ARCHIVED">ARCHIVED</option>
              </select>
            </div>
          </div>

          <!-- ÁRVORE DE MÓDULOS FILHOS -->
          <div v-if="expandedGoalIds.includes(goal.id)" class="border-t border-borderbase bg-app/60" :class="viewDensity === 'compact' ? 'px-3 pb-3 pt-2 space-y-2' : 'px-5 pb-5 pt-3 space-y-4'">
            
            <!-- Propósito (Oculto no compacto para economizar tela) -->
            <div v-if="viewDensity === 'detailed'" class="p-3 rounded bg-surface/80 border border-borderbase text-xs text-content-muted font-sans">
              <strong class="text-content font-mono uppercase">Propósito Estratégico:</strong> {{ goal.why }}
            </div>

            <div class="space-y-2">
              <div class="flex items-center justify-between">
                <p class="text-[10px] font-mono uppercase tracking-wider text-content-muted">Módulos de Execução:</p>
                
                <button
                  @click="addingModuleGoalId = goal.id"
                  class="text-[10px] font-mono text-content-muted hover:text-content flex items-center gap-1 cursor-pointer"
                >
                  <Plus class="w-3 h-3" /> Adicionar
                </button>
              </div>

              <div class="space-y-1 font-mono text-xs pl-2">
                <div 
                  v-for="(child, idx) in goal.children" 
                  :key="child.id"
                  class="flex items-center justify-between gap-4 text-content-muted hover:text-content transition-colors rounded bg-surface border border-borderbase/50"
                  :class="viewDensity === 'compact' ? 'py-1 px-2' : 'py-1.5 px-3'"
                >
                  <div class="flex items-center gap-2 truncate">
                    <span class="text-content-muted select-none">{{ idx === goal.children.length - 1 ? '└──' : '├──' }}</span>
                    <span class="truncate font-medium text-content" :class="viewDensity === 'compact' ? 'text-[11px]' : 'text-xs'">{{ child.name }}</span>
                  </div>

                  <div class="flex items-center gap-2 flex-shrink-0">
                    <input
                      v-if="viewDensity === 'detailed'"
                      type="range" min="0" max="100" step="10"
                      :value="child.progress"
                      @input="(e) => goalsStore.updateChildProgress(goal.id, child.id, Number((e.target as HTMLInputElement).value))"
                      class="w-16 accent-content cursor-pointer"
                    />
                    <!-- Controle numérico no modo compacto -->
                    <button v-else @click="goalsStore.updateChildProgress(goal.id, child.id, child.progress === 100 ? 0 : 100)" class="text-[10px] underline cursor-pointer">
                      Alternar
                    </button>
                    <span class="w-8 text-right font-mono font-bold text-content" :class="viewDensity === 'compact' ? 'text-[10px]' : 'text-xs'">{{ child.progress }}%</span>
                  </div>
                </div>

                <!-- Input Rápido -->
                <div v-if="addingModuleGoalId === goal.id" class="flex items-center gap-2 pt-1">
                  <input
                    v-model="newModuleName" type="text" placeholder="Novo módulo..."
                    @keydown.enter="handleAddModule(goal.id)" @keydown.esc="addingModuleGoalId = null"
                    class="flex-1 bg-surface px-2 py-1 rounded border border-borderfocus text-[11px] font-mono text-content focus:outline-none" autofocus
                  />
                  <button @click="handleAddModule(goal.id)" class="px-2 py-1 rounded bg-content text-content-invert text-[11px] font-semibold">
                    Enter
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </InspectableCard>
    </div>
  </div>
</template>