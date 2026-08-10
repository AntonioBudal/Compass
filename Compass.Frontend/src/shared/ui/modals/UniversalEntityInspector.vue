<script setup lang="ts">
import { computed, defineAsyncComponent } from 'vue';
import { useInspectorStore } from '@/modules/tactical/stores/inspectorStore';
import { X, Trash2, CheckCircle2, Edit2, Loader2, AlertCircle } from 'lucide-vue-next';

// Lazy loading da Fábrica de Sub-Formulários
const TaskEditorForm = defineAsyncComponent(() => import('@/components/inspectors/TaskEditorForm.vue'));
const EventEditorForm = defineAsyncComponent(() => import('@/components/inspectors/EventEditorForm.vue'));
const HabitEditorForm = defineAsyncComponent(() => import('@/components/inspectors/HabitEditorForm.vue'));
const GoalEditorForm = defineAsyncComponent(() => import('@/components/inspectors/GoalEditorForm.vue'));
const ProjectEditorForm = defineAsyncComponent(() => import('@/components/inspectors/ProjectEditorForm.vue'));

const inspectorStore = useInspectorStore();

// --- MAPEAMENTO DE TÍTULOS DINÂMICOS ---
const headerTitle = computed(() => {
  const type = inspectorStore.draft?.entityType;
  if (!type) return 'Inspetor';

  if (type === 'COMMITMENT') {
    const cType = inspectorStore.draft?.mutatedPayload?.type;
    const map: Record<string, string> = { TASK: 'Tarefa', HABIT: 'Hábito', EVENT: 'Evento', NOTE: 'Nota' };
    return `Editar ${map[cType] || 'Compromisso'}`;
  }
  if (type === 'PROJECT') return 'Editar Projeto';
  if (type === 'GOAL') return 'Editar Meta';
  return 'Inspetor Universal';
});

// --- INDICADOR DE STATUS DO AUTO-SAVE ---
const syncIndicator = computed(() => {
  switch (inspectorStore.syncStatus) {
    case 'EDITING': return { text: 'Rascunho...', icon: Edit2, color: 'text-content-muted', spin: false };
    case 'SYNCING': return { text: 'Salvando...', icon: Loader2, color: 'text-status-warning', spin: true };
    case 'SAVED': return { text: 'Salvo', icon: CheckCircle2, color: 'text-status-success-text', spin: false };
    case 'ERROR': return { text: 'Erro ao salvar', icon: AlertCircle, color: 'text-status-danger-text', spin: false };
    default: return null;
  }
});

//  CORREÇÃO (ARQ-013): Delegação total de responsabilidade. 
// O Componente não lida mais com debounces locais. A Store assume o controle.
const handleFormUpdate = () => {
  inspectorStore.markAsEditing();
};

// --- CONTROLE DE FOCO E TECLADO ---
const handleKeyDown = (e: KeyboardEvent) => {
  if (e.key === 'Escape') {
    e.preventDefault();
    e.stopPropagation();
    inspectorStore.flushAndClose();
  }
  
  // Ctrl+S / Cmd+S: Salva instantaneamente sem fechar (Força o Flush)
  if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 's') {
    e.preventDefault();
    e.stopPropagation();
    inspectorStore.flushAndClose(); 
  }
};

// --- FÁBRICA DINÂMICA ---
const resolveSubForm = computed(() => {
  const type = inspectorStore.draft?.entityType;
  const subType = inspectorStore.draft?.mutatedPayload?.type;
  
  if (type === 'COMMITMENT') {
    if (subType === 'TASK') return TaskEditorForm;
    if (subType === 'EVENT') return EventEditorForm;
    if (subType === 'HABIT') return HabitEditorForm;
  }
  if (type === 'GOAL') return GoalEditorForm;
  
  // ADICIONE ESTA LINHA:
  if (type === 'PROJECT') return ProjectEditorForm;
  
  return null;
});
</script>

<template>
  <transition name="slide-over">
    <!-- BACKDROP -->
    <div 
      v-if="inspectorStore.isOpen" 
      class="fixed inset-0 z-[100] flex justify-end bg-app/60 backdrop-blur-sm select-none"
      @click.self="inspectorStore.flushAndClose"
    >
      <!-- SHELL DO INSPETOR -->
      <div 
        class="w-full max-w-md bg-surface border-l border-borderbase h-full flex flex-col shadow-2xl focus:outline-none"
        tabindex="0"
        @keydown="handleKeyDown"
      >
        <!-- 1. CABEÇALHO -->
        <div class="px-6 py-4 flex items-center justify-between border-b border-borderbase">
          <div class="flex items-center gap-4">
            <span class="text-xs font-mono font-bold uppercase tracking-wider text-content">
              {{ headerTitle }}
            </span>
            
            <transition name="fade">
              <div v-if="syncIndicator" class="flex items-center gap-1.5 text-[10px] font-mono font-bold uppercase px-2 py-0.5 rounded bg-app border border-borderbase" :class="syncIndicator.color">
                <component :is="syncIndicator.icon" class="w-3 h-3" :class="syncIndicator.spin ? 'animate-spin' : ''" />
                <span>{{ syncIndicator.text }}</span>
              </div>
            </transition>
          </div>

          <button 
            @click="inspectorStore.flushAndClose"
            class="p-1.5 rounded-md text-content-muted hover:text-content hover:bg-surface-hover transition-colors cursor-pointer"
            title="Fechar (ESC)"
          >
            <X class="w-4 h-4" />
          </button>
        </div>

        <!-- 2. CORPO (FÁBRICA DINÂMICA) -->
        <div class="flex-1 overflow-y-auto p-6">
          <div v-if="inspectorStore.draft" class="h-full">
            
            <!--  CORREÇÃO (HIDRATAÇÃO E MUTAÇÃO): Uso do v-model:draft conectando a via bidirecional -->
            <component 
              v-if="resolveSubForm"
              :is="resolveSubForm" 
              v-model:draft="inspectorStore.draft.mutatedPayload" 
              @update="handleFormUpdate" 
            />
            
            <div v-else class="w-full h-full border-2 border-dashed border-borderfocus rounded-xl flex flex-col items-center justify-center p-8 text-center gap-4 opacity-50 bg-app">
               <AlertCircle class="w-8 h-8 text-content-muted" />
               <p class="text-sm font-mono text-content">Formulário especialista não encontrado para este tipo de entidade.</p>
            </div>
            
          </div>
        </div>

        <!-- 3. RODAPÉ FIXO -->
        <div class="p-4 border-t border-borderbase bg-app flex items-center justify-between">
          <button 
            type="button"
            @click="inspectorStore.requestDeletion()"
            class="inline-flex items-center gap-2 px-3 py-2 rounded-tactic bg-status-danger-bg/50 hover:bg-status-danger-bg text-status-danger-text border border-status-danger-border text-xs font-bold transition-colors cursor-pointer"
          >
            <Trash2 class="w-4 h-4" />
            <span>Excluir</span>
          </button>
          <span class="text-[10px] font-mono text-content-muted text-right">Auto-Save Ativo</span>
        </div>

      </div>
    </div>
  </transition>
</template>

<style scoped>
.slide-over-enter-active,
.slide-over-leave-active { transition: transform 250ms cubic-bezier(0.16, 1, 0.3, 1), opacity 200ms ease; }
.slide-over-enter-from,
.slide-over-leave-to { transform: translateX(100%); opacity: 0; }
.fade-enter-active,
.fade-leave-active { transition: opacity 150ms ease; }
.fade-enter-from,
.fade-leave-to { opacity: 0; }
</style>