<script setup lang="ts">
import { computed } from 'vue';
import { useCommitmentsStore, type CommitmentItem } from '@/stores/commitmentsStore';
import { editingCommitment } from '@/composables/useKeyboardShortcuts';
import TacticBadge from './TacticBadge.vue';
import EnergyIndicator from './EnergyIndicator.vue';
import { Check, Clock, Flame, MoreHorizontal } from 'lucide-vue-next';

const props = defineProps<{
  item: CommitmentItem;
}>();

const store = useCommitmentsStore();

const isCompleted = computed(() => props.item.status === 'COMPLETED');
const isHabit = computed(() => props.item.type === 'HABIT');

const handleComplete = () => {
  if (isCompleted.value) return;
  store.updateStatus(props.item.id, 'COMPLETED');
};

const handleOpenEdit = () => {
  editingCommitment.value = props.item;
};
</script>

<template>
  <div 
    class="group relative flex items-start justify-between gap-3 p-3 rounded-tactic border border-zinc-800/80 bg-zinc-900/40 hover:bg-zinc-900/90 hover:border-zinc-700/80 transition-all duration-tactic select-none gpu-accelerated cursor-pointer"
    :class="{ 'opacity-50 bg-zinc-950/20 translate-y-0.5': isCompleted }"
    @click="handleOpenEdit"
  >
    <span v-if="item._isSyncing" class="absolute top-2 right-2 w-1.5 h-1.5 rounded-full bg-zinc-400 animate-pulse" title="Sincronizando..." />

    <!-- Lado Esquerdo: Checkbox Tático -->
    <div class="flex items-start gap-3 min-w-0 flex-1">
      <button 
        type="button"
        @click.stop="handleComplete"
        class="mt-0.5 w-4.5 h-4.5 rounded border border-zinc-700 bg-zinc-950 flex items-center justify-center transition-colors duration-fast focus-visible:ring-2 focus-visible:ring-indigo-500 cursor-pointer"
        :class="isCompleted ? 'bg-emerald-600 border-emerald-500 text-white shadow-sm' : 'hover:border-zinc-500 group-hover:border-zinc-600'"
        :disabled="isCompleted"
      >
        <Check v-if="isCompleted" class="w-3.5 h-3.5 stroke-[3] animate-check" style="stroke-dasharray: 24;" />
      </button>

      <div class="min-w-0 flex-1">
        <p 
          class="text-sm font-medium text-zinc-100 group-hover:text-white transition-colors duration-fast truncate"
          :class="{ 'line-through text-zinc-500': isCompleted }"
        >
          {{ item.title }}
        </p>

        <!-- Metadados -->
        <div class="mt-1 flex flex-wrap items-center gap-2 text-xs text-zinc-500 font-mono">
          <span v-if="item.projectName" class="hidden lg:inline text-zinc-400">
            [{ item.projectName }]
          </span>

          <TacticBadge v-if="item.deadline" variant="urgent">
            Vence em breve
          </TacticBadge>

          <TacticBadge v-if="isHabit && item.currentStreak > 0" variant="execution">
            <Flame class="w-3 h-3 mr-0.5 inline text-emerald-400" /> {{ item.currentStreak }}d
          </TacticBadge>

          <span class="flex items-center gap-1 bg-zinc-900 px-1.5 py-0.5 rounded border border-zinc-800">
            <Clock class="w-3 h-3 text-zinc-400" /> {{ item.estimatedDurationMinutes }}m
          </span>
        </div>
      </div>
    </div>

    <!-- Lado Direito: Geometria de Energia e Opções -->
    <div class="flex items-center gap-2 flex-shrink-0" @click.stop>
      <EnergyIndicator class="hidden sm:inline-flex" :level="item.energyRequired || 2" />

      <button 
        type="button"
        @click="handleOpenEdit"
        class="opacity-0 group-hover:opacity-100 focus:opacity-100 p-1 rounded text-zinc-500 hover:text-zinc-200 hover:bg-zinc-800 transition-opacity duration-fast cursor-pointer"
        title="Opções de Edição"
      >
        <MoreHorizontal class="w-4 h-4" />
      </button>
    </div>
  </div>
</template>