<script setup lang="ts">
import { ref, watch, nextTick } from 'vue';
import { useCommitmentsStore, type CommitmentItem } from '@/stores/commitmentsStore';
import { editingCommitment } from '@/composables/useKeyboardShortcuts';
import { X, Trash2, Check, Clock, Folder } from 'lucide-vue-next';

const store = useCommitmentsStore();

const title = ref('');
const duration = ref(30);
const energy = ref(2);
const project = ref('');
const inputRef = ref<HTMLInputElement | null>(null);
const isSubmitting = ref(false);

watch(editingCommitment, async (item) => {
  if (item) {
    title.value = item.title;
    duration.value = item.estimatedDurationMinutes || 30;
    energy.value = item.energyRequired || 2;
    project.value = item.projectName || '';
    isSubmitting.value = false;
    await nextTick();
    inputRef.value?.focus();
  }
});

const handleSave = async () => {
  if (!editingCommitment.value || !title.value.trim() || isSubmitting.value) return;

  isSubmitting.value = true;
  try {
    await store.updateCommitment(editingCommitment.value.id, {
      title: title.value.trim(),
      estimatedDurationMinutes: Number(duration.value) || 30,
      energyRequired: Number(energy.value) || 2,
      projectId: project.value.trim() || null
    });
    editingCommitment.value = null;
  } catch (e) {
    console.error('Falha ao salvar edição', e);
  } finally {
    isSubmitting.value = false;
  }
};

const handleDelete = async () => {
  if (!editingCommitment.value) return;
  const targetId = editingCommitment.value.id;
  editingCommitment.value = null;
  await store.deleteCommitment(targetId);
};

const handleKeyDown = (e: KeyboardEvent) => {
  if (e.key === 'Enter' && !e.shiftKey) {
    e.preventDefault();
    handleSave();
  }
};
</script>

<template>
  <transition name="slide-over">
    <div 
      v-if="editingCommitment" 
      class="fixed inset-0 z-50 flex justify-end bg-black/60 backdrop-blur-sm select-none"
      @click.self="editingCommitment = null"
    >
      <div 
        class="w-full max-w-md bg-zinc-900 border-l border-zinc-800 h-full p-6 flex flex-col justify-between shadow-2xl"
        @keydown="handleKeyDown"
      >
        <!-- Cabeçalho -->
        <div class="space-y-6">
          <div class="flex items-center justify-between pb-4 border-b border-zinc-800">
            <span class="text-xs font-mono uppercase text-zinc-400">Editar Compromisso</span>
            <button 
              @click="editingCommitment = null"
              class="p-1 rounded text-zinc-500 hover:text-zinc-300 hover:bg-zinc-800 transition-colors"
            >
              <X class="w-4 h-4" />
            </button>
          </div>

          <!-- Campos de Edição -->
          <div class="space-y-4">
            <div>
              <label class="block text-xs font-mono uppercase text-zinc-400 mb-1.5">Título da Ação</label>
              <input 
                ref="inputRef"
                v-model="title"
                type="text"
                class="w-full px-3 py-2 bg-zinc-950 border border-zinc-800 rounded-tactic text-sm text-zinc-100 focus:border-zinc-600 focus:outline-none font-sans"
              />
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-xs font-mono uppercase text-zinc-400 mb-1.5">Duração (m)</label>
                <div class="relative flex items-center">
                  <Clock class="w-3.5 h-3.5 text-zinc-500 absolute left-3" />
                  <input 
                    v-model.number="duration"
                    type="number"
                    step="15"
                    class="w-full pl-8 pr-3 py-1.5 bg-zinc-950 border border-zinc-800 rounded-tactic text-sm font-mono text-zinc-100 focus:border-zinc-600 focus:outline-none"
                  />
                </div>
              </div>

              <div>
                <label class="block text-xs font-mono uppercase text-zinc-400 mb-1.5">Energia</label>
                <select 
                  v-model.number="energy"
                  class="w-full px-3 py-1.5 bg-zinc-950 border border-zinc-800 rounded-tactic text-sm font-mono text-zinc-100 focus:border-zinc-600 focus:outline-none"
                >
                  <option :value="1">■□□ MAINT (1)</option>
                  <option :value="2">■■□ OPER (2)</option>
                  <option :value="3">■■■ DEEP (3)</option>
                </select>
              </div>
            </div>

            <div>
              <label class="block text-xs font-mono uppercase text-zinc-400 mb-1.5">Projeto Vinculado</label>
              <div class="relative flex items-center">
                <Folder class="w-3.5 h-3.5 text-zinc-500 absolute left-3" />
                <input 
                  v-model="project"
                  type="text"
                  placeholder="Ex: Backend .NET"
                  class="w-full pl-8 pr-3 py-1.5 bg-zinc-950 border border-zinc-800 rounded-tactic text-sm text-zinc-100 focus:border-zinc-600 focus:outline-none"
                />
              </div>
            </div>
          </div>
        </div>

        <!-- Rodapé com Ações de Mutações -->
        <div class="pt-4 border-t border-zinc-800 flex items-center justify-between gap-3">
          <button 
            type="button"
            @click="handleDelete"
            class="inline-flex items-center gap-1.5 px-3 py-2 rounded-tactic bg-rose-500/10 hover:bg-rose-500/20 text-rose-400 border border-rose-500/20 text-xs font-medium transition-colors cursor-pointer"
            title="Arquivar/Excluir"
          >
            <Trash2 class="w-3.5 h-3.5" />
            <span>Excluir</span>
          </button>

          <div class="flex items-center gap-2">
            <button 
              type="button"
              @click="editingCommitment = null"
              class="px-3 py-2 rounded-tactic hover:bg-zinc-800 text-zinc-400 text-xs font-medium transition-colors"
            >
              Cancelar
            </button>

            <button 
              type="button"
              @click="handleSave"
              :disabled="!title.trim() || isSubmitting"
              class="inline-flex items-center gap-1.5 px-4 py-2 rounded-tactic bg-zinc-100 hover:bg-white text-zinc-950 text-xs font-semibold shadow-sm transition-all disabled:opacity-50 cursor-pointer"
            >
              <Check class="w-3.5 h-3.5 stroke-[2.5]" />
              <span>Salvar</span>
              <kbd class="ml-1 text-[9px] font-mono bg-zinc-300 text-zinc-800 px-1 rounded">Enter</kbd>
            </button>
          </div>
        </div>
      </div>
    </div>
  </transition>
</template>

<style scoped>
.slide-over-enter-active,
.slide-over-leave-active {
  transition: transform 150ms cubic-bezier(0.16, 1, 0.3, 1), opacity 150ms ease;
}
.slide-over-enter-from,
.slide-over-leave-to {
  transform: translateX(100%);
  opacity: 0;
}
</style>