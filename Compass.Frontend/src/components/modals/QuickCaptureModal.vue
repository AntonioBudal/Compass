<script setup lang="ts">
import { ref, watch, nextTick } from 'vue';
import { useCommitmentsStore } from '@/stores/commitmentsStore';
import { useDecisionStore } from '@/stores/decisionStore';
import { isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';
import type { CommitmentType } from '@/types/index';
import { PlusCircle, Clock, Zap, Check, X, Calendar, Folder, RefreshCw, FileText } from 'lucide-vue-next';

const commitmentsStore = useCommitmentsStore();
const decisionStore = useDecisionStore();

const title = ref('');
const selectedType = ref<CommitmentType>('TASK');
const duration = ref(30);
const energy = ref(2);
const titleInputRef = ref<HTMLInputElement | null>(null);
const isSubmitting = ref(false);

const archetypes = [
  { id: 'TASK' as CommitmentType, label: 'Tarefa', icon: Check, shortcut: '1' },
  { id: 'HABIT' as CommitmentType, label: 'Hábito', icon: RefreshCw, shortcut: '2' },
  { id: 'EVENT' as CommitmentType, label: 'Evento', icon: Calendar, shortcut: '3' },
  { id: 'NOTE' as CommitmentType, label: 'Nota', icon: FileText, shortcut: '4' },
];

watch(isQuickCaptureOpen, async (isOpen) => {
  if (isOpen) {
    title.value = '';
    selectedType.value = 'TASK';
    duration.value = 30;
    energy.value = 2;
    isSubmitting.value = false;
    await nextTick();
    titleInputRef.value?.focus();
  }
});

const handleSubmit = async () => {
  if (!title.value.trim() || isSubmitting.value) return;

  isSubmitting.value = true;
  try {
    await commitmentsStore.createCommitment({
      title: title.value.trim(),
      type: selectedType.value,
      estimatedDurationMinutes: Number(duration.value) || 30,
      energyRequired: Number(energy.value) || 2,
    });

    isQuickCaptureOpen.value = false;
    // Força atualização da tela do motor se uma nova tarefa foi injetada no sistema
    decisionStore.fetchNow();
  } catch (err) {
    console.error('Erro na captura rápida:', err);
  } finally {
    isSubmitting.value = false;
  }
};

// Alternância rápida de arquétipo via teclado (Cmd+1 a Cmd+4)
const handleKeyDown = (e: KeyboardEvent) => {
  if (e.key === 'Enter' && !e.shiftKey) {
    e.preventDefault();
    handleSubmit();
    return;
  }
  if ((e.metaKey || e.ctrlKey) && ['1', '2', '3', '4'].includes(e.key)) {
    e.preventDefault();
    const idx = parseInt(e.key) - 1;
    if (archetypes[idx]) {
      selectedType.value = archetypes[idx].id;
    }
  }
};
</script>

<template>
  <transition name="modal-snap">
    <div 
      v-if="isQuickCaptureOpen" 
      class="fixed inset-0 z-50 flex items-start justify-center pt-[20vh] px-4 bg-black/60 backdrop-blur-sm select-none"
      @click.self="isQuickCaptureOpen = false"
    >
      <div 
        class="w-full max-w-lg bg-zinc-900/95 backdrop-blur-xl border border-zinc-700/80 shadow-2xl rounded-xl overflow-hidden p-6 space-y-5"
        @keydown="handleKeyDown"
      >
        <!-- Cabeçalho de Captura -->
        <div class="flex items-center justify-between pb-3 border-b border-zinc-800">
          <div class="flex items-center gap-2 text-sm font-semibold text-zinc-100">
            <PlusCircle class="w-4 h-4 text-emerald-400" />
            <span>Captura Rápida de Ação</span>
          </div>
          <button 
            @click="isQuickCaptureOpen = false" 
            class="p-1 rounded text-zinc-400 hover:text-white hover:bg-zinc-800 transition-colors"
          >
            <X class="w-4 h-4" />
          </button>
        </div>

        <!-- Campo Principal do Título -->
        <div>
          <label class="block text-xs font-medium text-zinc-400 uppercase tracking-wider mb-1.5">
            O que precisa ser feito ou registrado?
          </label>
          <input 
            ref="titleInputRef"
            v-model="title"
            type="text" 
            placeholder="Ex: Revisar PR de autenticação no GitHub..." 
            class="w-full px-3 py-2.5 bg-zinc-950/80 border border-zinc-800 focus:border-zinc-600 focus:ring-1 focus:ring-zinc-600 rounded-tactic text-sm text-zinc-100 placeholder:text-zinc-500 transition-all font-sans"
          />
        </div>

        <!-- Seletor de Arquétipos (Buttons Toggle) -->
        <div class="space-y-1.5">
          <label class="block text-xs font-medium text-zinc-400 uppercase tracking-wider">
            Arquétipo (Tipo de Compromisso)
          </label>
          <div class="grid grid-cols-4 gap-2">
            <button
              v-for="arc in archetypes"
              :key="arc.id"
              type="button"
              @click="selectedType = arc.id"
              class="flex items-center justify-center gap-1.5 py-1.5 px-2 rounded-tactic border text-xs font-medium transition-all"
              :class="selectedType === arc.id 
                ? 'bg-zinc-800 border-indigo-500 text-white shadow-sm' 
                : 'bg-zinc-950/60 border-zinc-800/80 text-zinc-400 hover:border-zinc-700 hover:text-zinc-200'"
            >
              <component :is="arc.icon" class="w-3.5 h-3.5" :class="selectedType === arc.id ? 'text-indigo-400' : 'text-zinc-500'" />
              <span>{{ arc.label }}</span>
            </button>
          </div>
        </div>

        <!-- Metadados Táticos (Duração & Energia) -->
        <div class="grid grid-cols-2 gap-4 pt-1">
          <div>
            <label class="block text-xs font-medium text-zinc-400 uppercase tracking-wider mb-1.5">
              Duração Estimada
            </label>
            <div class="relative flex items-center">
              <Clock class="w-4 h-4 text-zinc-500 absolute left-3" />
              <input 
                v-model.number="duration"
                type="number" 
                step="15"
                min="5"
                max="480"
                class="w-full pl-9 pr-8 py-1.5 bg-zinc-950/80 border border-zinc-800 focus:border-zinc-600 rounded-tactic text-sm font-mono text-zinc-100"
              />
              <span class="absolute right-3 text-xs font-mono text-zinc-500">min</span>
            </div>
          </div>

          <div>
            <label class="block text-xs font-medium text-zinc-400 uppercase tracking-wider mb-1.5">
              Energia Requerida
            </label>
            <select 
              v-model.number="energy"
              class="w-full px-3 py-1.5 bg-zinc-950/80 border border-zinc-800 focus:border-zinc-600 rounded-tactic text-sm font-mono text-zinc-100"
            >
              <option :value="1">⚡ Baixa (1)</option>
              <option :value="2">⚡⚡ Média (2)</option>
              <option :value="3">⚡⚡⚡ Alta (3)</option>
            </select>
          </div>
        </div>

        <!-- Rodapé de Confirmação -->
        <div class="flex items-center justify-end gap-3 pt-3 border-t border-zinc-800">
          <button 
            type="button"
            @click="isQuickCaptureOpen = false"
            class="px-3 py-1.5 rounded-tactic bg-transparent hover:bg-zinc-800/60 text-xs font-medium text-zinc-400 hover:text-white transition-colors"
          >
            Cancelar <kbd class="ml-1 text-[10px] font-mono bg-zinc-900 px-1 rounded border border-zinc-800">ESC</kbd>
          </button>

          <button 
            type="button"
            @click="handleSubmit"
            :disabled="!title.trim() || isSubmitting"
            class="inline-flex items-center gap-2 px-4 py-1.5 rounded-tactic bg-emerald-600 hover:bg-emerald-500 active:scale-[0.99] text-xs font-medium text-white shadow-sm transition-all disabled:opacity-50"
          >
            <Check class="w-3.5 h-3.5 stroke-[2.5]" />
            <span>Salvar Compromisso</span>
            <kbd class="ml-1 text-[10px] font-mono bg-emerald-700/60 px-1 rounded border border-emerald-500/30">Enter</kbd>
          </button>
        </div>
      </div>
    </div>
  </transition>
</template>

<style scoped>
.modal-snap-enter-active,
.modal-snap-leave-active {
  transition: opacity 150ms cubic-bezier(0.16, 1, 0.3, 1), transform 150ms cubic-bezier(0.16, 1, 0.3, 1);
}
.modal-snap-enter-from,
.modal-snap-leave-to {
  opacity: 0;
  transform: scale(0.96);
}
</style>