<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useGoalsStore } from '@/stores/goalsStore';
import { Target, PlusCircle, ChevronRight, CheckCircle2, Clock, Plus } from 'lucide-vue-next';
import { isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';
import DefensiveEmptyState from '@/components/core/DefensiveEmptyState.vue';

const goalsStore = useGoalsStore();
const expandedGoalIds = ref<string[]>(['goal-1']);

// Controle de edição inline de título
const editingGoalId = ref<string | null>(null);
const editingTitleText = ref('');

// Controle para adicionar novo módulo inline
const newModuleName = ref('');
const addingModuleGoalId = ref<string | null>(null);

onMounted(() => {
  goalsStore.loadFromDisk();
});

const toggleGoal = (id: string) => {
  const index = expandedGoalIds.value.indexOf(id);
  if (index === -1) expandedGoalIds.value.push(id);
  else expandedGoalIds.value.splice(index, 1);
};

// Edição Inline de Atrito Zero
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
    <!-- Cabeçalho de Metas -->
    <div class="flex items-center justify-between gap-4 pb-4 border-b border-borderbase">
      <div>
        <h1 class="text-2xl font-semibold text-content tracking-tight flex items-center gap-2.5">
          <span>Metas Estratégicas</span>
          <span class="text-xs font-mono bg-surface text-content-muted px-2 py-0.5 rounded border border-borderbase">
            {{ goalsStore.activeGoals.length }} Ativas
          </span>
        </h1>
        <p class="text-sm text-content-muted mt-1">
          Guarda-chuvas estratégicos com medição de progresso e edição inline sem cliques extras.
        </p>
      </div>

      <button 
        @click="isQuickCaptureOpen = true"
        class="inline-flex items-center gap-2 px-3 py-1.5 rounded-tactic bg-content text-content-invert hover:opacity-90 text-xs font-semibold font-mono transition-all shadow-sm cursor-pointer"
      >
        <PlusCircle class="w-3.5 h-3.5" />
        <span>Nova Meta</span>
      </button>
    </div>

    <!-- EMPTY STATE DEFENSIVO (Se não houver metas) -->
    <DefensiveEmptyState
      v-if="goalsStore.goals.length === 0"
      :icon="Target"
      title="Nenhuma meta estratégica definida."
      explanation="Metas conectam tarefas táticas diárias a objetivos de longo prazo. Sem metas, o Now Engine pondera apenas prazos urgentes."
      action-label="Criar Primeira Meta (Cmd+K)"
      @action="isQuickCaptureOpen = true"
    />

    <!-- STRATEGIC TREE ACCORDION -->
    <div v-else class="space-y-4">
      <div 
        v-for="goal in goalsStore.goals" 
        :key="goal.id"
        class="rounded-xl border border-borderbase bg-surface overflow-hidden transition-all duration-tactic"
        :class="{ 'border-borderfocus bg-surface-active shadow-lg': expandedGoalIds.includes(goal.id) }"
      >
        <!-- Título Expansível com Edição Inline -->
        <div 
          @click="toggleGoal(goal.id)"
          class="w-full p-5 flex items-center justify-between gap-4 text-left hover:bg-surface-hover transition-colors cursor-pointer"
        >
          <div class="flex items-center gap-3 min-w-0 flex-1">
            <ChevronRight 
              class="w-5 h-5 text-content-muted transition-transform duration-200 flex-shrink-0"
              :class="{ 'rotate-90 text-content': expandedGoalIds.includes(goal.id) }"
            />
            
            <div class="truncate space-y-1 flex-1 min-w-0">
              <!-- MODO EDIÇÃO INLINE DO TÍTULO -->
              <input
                v-if="editingGoalId === goal.id"
                v-model="editingTitleText"
                type="text"
                @click.stop
                @keydown.enter="saveEditedTitle(goal.id)"
                @blur="saveEditedTitle(goal.id)"
                class="w-full bg-surface px-2 py-0.5 rounded border border-borderfocus text-base font-semibold text-content focus:outline-none"
                autofocus
              />
              <!-- MODO LEITURA NORMAL -->
              <h2 
                v-else 
                @dblclick="startEditingTitle(goal.id, goal.title, $event)"
                class="text-base font-semibold text-content truncate flex items-center gap-2 group"
                title="Duplo clique para editar o título sem modais"
              >
                <span>{{ goal.title }}</span>
                <span class="text-[10px] font-mono opacity-0 group-hover:opacity-100 text-content-muted">[Duplo clique para editar]</span>
              </h2>

              <p class="text-xs text-content-muted truncate font-mono">
                Prazo: {{ goal.targetDate }} | Módulos: {{ goal.children?.length || 0 }}
              </p>
            </div>
          </div>

          <!-- Medidor de Progresso da Meta -->
          <div class="flex items-center gap-4 flex-shrink-0">
            <div class="flex flex-col items-end gap-1.5">
              <span class="text-xs font-mono text-content-muted">Progresso: <strong class="text-content">{{ goal.progressPercentage }}%</strong></span>
              <div class="h-1.5 w-24 bg-app rounded-full overflow-hidden">
                <div class="h-full bg-content transition-all duration-500" :style="{ width: `${goal.progressPercentage}%` }" />
              </div>
            </div>

            <!-- Seletor Rápido de Estado da Meta -->
            <select
              :value="goal.status"
              @change="(e) => goalsStore.updateGoalStatus(goal.id, (e.target as HTMLSelectElement).value as any)"
              @click.stop
              class="text-[11px] font-mono uppercase bg-surface border border-borderbase px-2 py-1 rounded text-content cursor-pointer focus:outline-none"
            >
              <option value="ACTIVE">ACTIVE</option>
              <option value="COMPLETED">COMPLETED</option>
              <option value="ARCHIVED">ARCHIVED</option>
            </select>
          </div>
        </div>

        <!-- ÁRVORE DE PROJETOS E MÓDULOS FILHOS -->
        <div v-if="expandedGoalIds.includes(goal.id)" class="px-5 pb-5 pt-3 border-t border-borderbase bg-app/60 space-y-4">
          <div class="p-3 rounded bg-surface/80 border border-borderbase text-xs text-content-muted font-sans">
            <strong class="text-content font-mono uppercase">Propósito Estratégico:</strong> {{ goal.why }}
          </div>

          <div class="space-y-2">
            <div class="flex items-center justify-between">
              <p class="text-xs font-mono uppercase tracking-wider text-content-muted">Módulos de Execução:</p>
              
              <!-- Botão para Adicionar Módulo -->
              <button
                @click="addingModuleGoalId = goal.id"
                class="text-xs font-mono text-content-muted hover:text-content flex items-center gap-1 cursor-pointer"
              >
                <Plus class="w-3.5 h-3.5" /> Adicionar Módulo
              </button>
            </div>

            <!-- Lista de Filhos -->
            <div class="space-y-1.5 font-mono text-xs pl-2">
              <div 
                v-for="(child, idx) in goal.children" 
                :key="child.id"
                class="flex items-center justify-between gap-4 text-content-muted hover:text-content transition-colors py-1.5 px-3 rounded bg-surface border border-borderbase/50"
              >
                <div class="flex items-center gap-2 truncate">
                  <span class="text-content-muted select-none">{{ idx === goal.children.length - 1 ? '└──' : '├──' }}</span>
                  <span class="truncate font-medium text-content">{{ child.name }}</span>
                </div>

                <!-- Slider / Check de Progresso do Módulo -->
                <div class="flex items-center gap-3 flex-shrink-0">
                  <input
                    type="range"
                    min="0"
                    max="100"
                    step="10"
                    :value="child.progress"
                    @input="(e) => goalsStore.updateChildProgress(goal.id, child.id, Number((e.target as HTMLInputElement).value))"
                    class="w-20 accent-content cursor-pointer"
                    title="Ajustar porcentagem do módulo"
                  />
                  <span class="w-10 text-right font-mono font-bold text-content">{{ child.progress }}%</span>
                </div>
              </div>

              <!-- Input Rápido para Novo Módulo -->
              <div v-if="addingModuleGoalId === goal.id" class="flex items-center gap-2 pt-1">
                <input
                  v-model="newModuleName"
                  type="text"
                  placeholder="Nome do novo módulo ou entrega..."
                  @keydown.enter="handleAddModule(goal.id)"
                  @keydown.esc="addingModuleGoalId = null"
                  class="flex-1 bg-surface px-3 py-1.5 rounded border border-borderfocus text-xs font-mono text-content focus:outline-none"
                  autofocus
                />
                <button
                  @click="handleAddModule(goal.id)"
                  class="px-3 py-1.5 rounded bg-content text-content-invert text-xs font-semibold"
                >
                  Salvar (Enter)
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>