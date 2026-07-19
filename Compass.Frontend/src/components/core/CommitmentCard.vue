<script setup lang="ts">
import { computed } from 'vue';
import { useCommitmentsStore, type CommitmentItem } from '@/stores/commitmentsStore';
import TacticBadge from './TacticBadge.vue';
import { Check, Clock, Flame, Zap, MoreHorizontal } from 'lucide-vue-next';

const props = defineProps<{
  item: CommitmentItem;
}>();

const store = useCommitmentsStore();

const isCompleted = computed(() => props.item.status === 'COMPLETED');
const isHabit = computed(() => props.item.type === 'HABIT');

const energyLabel = computed(() => {
  switch (props.item.energyRequired) {
    case 3: return '⚡⚡⚡ Alta';
    case 1: return '⚡ Baixa';
    case 2:
    default: return '⚡⚡ Média';
  }
});

const handleComplete = () => {
  if (isCompleted.value) return;
  // Mutação instantânea < 16ms
  store.updateStatus(props.item.id, 'COMPLETED');
};
</script>

<template>
  <div 
    class="group relative flex items-start justify-between gap-3 p-3 rounded-lg border border-zinc-800/80 bg-zinc-900/40 hover:bg-zinc-900/90 hover:border-zinc-700/80 transition-all duration-tactic select-none"
    :class="{ 'opacity-50 bg-zinc-950/20': isCompleted }"
  >
    <!-- Indicador de transação pendente (Optimistic UI Syncing) -->
    <span v-if="item._isSyncing" class="absolute top-2 right-2 w-1.5 h-1.5 rounded-full bg-indigo-500 animate-pulse" title="Sincronizando com servidor..." />

    <!-- Lado Esquerdo: Checkbox Tático -->
    <div class="flex items-start gap-3 min-w-0 flex-1">
      <button 
        type="button"
        @click.stop="handleComplete"
        class="mt-0.5 w-4.5 h-4.5 rounded border border-zinc-700 bg-zinc-950 flex items-center justify-center transition-colors focus-visible:ring-2 focus-visible:ring-indigo-500"
        :class="isCompleted ? 'bg-emerald-600 border-emerald-500 text-white' : 'hover:border-zinc-500 group-hover:border-zinc-600'"
        :disabled="isCompleted"
      >
        <Check v-if="isCompleted" class="w-3 h-3 stroke-[3]" />
      </button>

      <!-- Centro: Hierarquia de Dados -->
      <div class="min-w-0 flex-1">
        <p 
          class="text-sm font-medium text-zinc-100 group-hover:text-white transition-colors truncate"
          :class="{ 'line-through text-zinc-500': isCompleted }"
        >
          {{ item.title }}
        </p>

        <!-- Linha 2: Metadados em Monospace (< 1024px oculta projeto por compressão responsiva) -->
        <div class="mt-1 flex flex-wrap items-center gap-2 text-xs text-zinc-500 font-mono">
          <span v-if="item.projectName" class="hidden lg:inline text-zinc-400">
            📁 {{ item.projectName }}
          </span>

          <TacticBadge v-if="item.deadline" variant="urgent">
            ⚠️ Vence em breve
          </TacticBadge>

          <TacticBadge v-if="isHabit && item.currentStreak > 0" variant="execution">
            <Flame class="w-3 h-3 mr-0.5 inline" /> {{ item.currentStreak }}d
          </TacticBadge>

          <span class="flex items-center gap-1 bg-zinc-900 px-1.5 py-0.5 rounded border border-zinc-800">
            <Clock class="w-3 h-3 text-zinc-400" /> {{ item.estimatedDurationMinutes }}m
          </span>
        </div>
      </div>
    </div>

    <!-- Lado Direito: Comandos Secundários e Energia -->
    <div class="flex items-center gap-2 flex-shrink-0">
      <span class="hidden sm:inline-block text-xs font-mono text-zinc-400 bg-zinc-800/60 px-1.5 py-0.5 rounded border border-zinc-700/40">
        {{ energyLabel }}
      </span>

      <!-- Revelado apenas em Hover ou Foco (Cap. 3.4) -->
      <button 
        type="button" 
        class="opacity-0 group-hover:opacity-100 focus:opacity-100 p-1 rounded text-zinc-400 hover:text-zinc-100 hover:bg-zinc-800 transition-opacity"
        title="Mais opções..."
      >
        <MoreHorizontal class="w-4 h-4" />
      </button>
    </div>
  </div>
</template>