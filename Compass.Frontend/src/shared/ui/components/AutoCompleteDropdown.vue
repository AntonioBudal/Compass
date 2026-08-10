<script setup lang="ts">
import { ref, watch, nextTick } from 'vue';
import { Hash, Clock, Calendar, Terminal, CornerDownLeft } from 'lucide-vue-next';

export interface DropdownItem {
  label: string;
  value: string;
  id?: string;
}

const props = defineProps<{
  items: DropdownItem[];
  selectedIndex: number;
  triggerType: 'PROJECT' | 'TYPE' | 'TIME' | 'DATE' | null;
}>();

const emit = defineEmits<{
  (e: 'select', item: DropdownItem): void;
}>();

const listRef = ref<HTMLElement | null>(null);

// Rastreamento automático de foco: Rola o container suavemente se o item focado sair da área visível
watch(() => props.selectedIndex, async (newIdx) => {
  await nextTick();
  if (!listRef.value) return;
  
  const activeEl = listRef.value.children[newIdx] as HTMLElement;
  if (activeEl) {
    activeEl.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
  }
});
</script>

<template>
  <div 
    v-if="items.length > 0 && triggerType"
    class="absolute left-4 right-4 top-14 z-30 bg-surface border border-borderfocus rounded-lg shadow-2xl overflow-hidden py-1.5 max-h-56 overflow-y-auto font-mono select-none gpu-accelerated animate-dropdown-in"
  >
    <!-- Cabeçalho de Categoria (Raycast Style) -->
    <div class="px-3 py-1 text-[10px] uppercase tracking-wider text-content-muted border-b border-borderbase/60 flex items-center justify-between">
      <span>
        {{ triggerType === 'PROJECT' ? 'Projetos LRU (Catálogo RAM)' : 
           triggerType === 'TYPE' ? 'Arquétipos do Sistema' : 
           triggerType === 'TIME' ? 'Sprints & Duração' : 'Limites Temporais' }}
      </span>
      <span>{{ items.length }} sugestões</span>
    </div>

    <!-- Lista Reativa -->
    <div ref="listRef" class="divide-y divide-borderbase/20 pt-0.5">
      <div
        v-for="(item, idx) in items"
        :key="item.value"
        @click="emit('select', item)"
        class="px-3 py-2 text-xs flex items-center justify-between cursor-pointer transition-colors"
        :class="idx === selectedIndex ? 'bg-surface-active text-content font-bold border-l-2 border-content pl-2.5' : 'text-content-muted hover:bg-surface-hover hover:text-content'"
      >
        <div class="flex items-center gap-2.5 truncate">
          <Hash v-if="triggerType === 'PROJECT'" class="w-3.5 h-3.5 flex-shrink-0 text-content-muted" />
          <Clock v-else-if="triggerType === 'TIME'" class="w-3.5 h-3.5 flex-shrink-0 text-content-muted" />
          <Calendar v-else-if="triggerType === 'DATE'" class="w-3.5 h-3.5 flex-shrink-0 text-content-muted" />
          <Terminal v-else-if="triggerType === 'TYPE'" class="w-3.5 h-3.5 flex-shrink-0 text-content-muted" />
          <span class="truncate">{{ item.label }}</span>
        </div>

        <span class="text-[10px] opacity-50 font-mono flex items-center gap-1 flex-shrink-0">
          <span>Tab / Enter</span>
          <CornerDownLeft class="w-3 h-3" />
        </span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.gpu-accelerated {
  will-change: transform, opacity;
  transform: translateZ(0);
}
.animate-dropdown-in {
  animation: dropdownIn 120ms cubic-bezier(0.16, 1, 0.3, 1) forwards;
}
@keyframes dropdownIn {
  from { opacity: 0; transform: translateY(-4px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>