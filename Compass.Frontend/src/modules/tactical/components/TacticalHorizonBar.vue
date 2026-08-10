<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue';
import { Calendar, Clock, Sun, Sunrise } from 'lucide-vue-next';

export type HorizonOption = 'today' | 'tomorrow' | '3days' | 'week';

const props = defineProps<{
  modelValue: HorizonOption;
  counts?: Record<HorizonOption, number>;
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', value: HorizonOption): void;
}>();

const options: { id: HorizonOption; label: string; icon: any; hint: string }[] = [
  { id: 'today', label: 'Hoje (Turno Atual)', icon: Sun, hint: '1' },
  { id: 'tomorrow', label: 'Amanhã', icon: Sunrise, hint: '2' },
  { id: '3days', label: 'Próximos 3 Dias', icon: Clock, hint: '3' },
  { id: 'week', label: 'Próxima Semana', icon: Calendar, hint: '4' }
];

function setHorizon(opt: HorizonOption) {
  emit('update:modelValue', opt);
}

// Atalhos de teclado: Shift + 1..4 para alternar o horizonte em latência zero (< 16ms)
function handleKeyDown(e: KeyboardEvent) {
  if (e.shiftKey && ['1', '2', '3', '4'].includes(e.key)) {
    const idx = parseInt(e.key) - 1;
    if (options[idx]) {
      e.preventDefault();
      setHorizon(options[idx].id);
    }
  }
}

// Intercepta eventos de UX Defensiva (ex: quando o VisibilityTracker sugere "Ver Amanhã")
function handleHorizonEvent(e: CustomEvent<HorizonOption>) {
  if (e.detail) setHorizon(e.detail);
}

onMounted(() => {
  window.addEventListener('keydown', handleKeyDown);
  window.addEventListener('compass:set-horizon' as any, handleHorizonEvent as EventListener);
});

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeyDown);
  window.removeEventListener('compass:set-horizon' as any, handleHorizonEvent as EventListener);
});
</script>

<template>
  <div class="flex items-center justify-between p-1.5 rounded-xl bg-surface border border-borderbase font-mono select-none overflow-x-auto">
    <div class="flex items-center gap-1 min-w-0">
      <button
        v-for="opt in options"
        :key="opt.id"
        type="button"
        @click="setHorizon(opt.id)"
        class="flex items-center gap-2 px-3 py-1.5 rounded-lg text-xs font-semibold transition-all cursor-pointer whitespace-nowrap"
        :class="modelValue === opt.id
          ? 'bg-content text-content-invert shadow-sm border border-content'
          : 'bg-transparent text-content-muted hover:text-content hover:bg-surface-hover border border-transparent'"
      >
        <component :is="opt.icon" class="w-3.5 h-3.5 flex-shrink-0" />
        <span>{{ opt.label }}</span>
        
        <!-- Contador de Itens no Horizonte -->
        <span
          v-if="counts && counts[opt.id] !== undefined"
          class="ml-1 px-1.5 py-0.5 rounded text-[10px] border font-bold"
          :class="modelValue === opt.id
            ? 'bg-app text-content border-borderbase'
            : 'bg-app/60 text-content-muted border-borderbase'"
        >
          {{ counts[opt.id] }}
        </span>
      </button>
    </div>

    <!-- Referência Tática do Atalho -->
    <div class="hidden md:flex items-center gap-1.5 px-3 text-[11px] text-content-muted">
      <span>Atalhos:</span>
      <kbd class="px-1.5 py-0.5 rounded bg-app border border-borderbase text-content font-bold">Shift + 1..4</kbd>
    </div>
  </div>
</template>