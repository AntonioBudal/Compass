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
    class="group relative flex items-start justify-between gap-3 p-3 rounded-tactic border border-borderbase bg-surface hover:bg-surface-hover hover:border-borderfocus transition-all duration-tactic select-none gpu-accelerated cursor-pointer"
    :class="{ 'opacity-50 bg-app translate-y-0.5': isCompleted }"
    @click="handleOpenEdit"
  >
    <span v-if="item._isSyncing" class="absolute top-2 right-2 w-1.5 h-1.5 rounded-full bg-content-accent animate-pulse" title="Sincronizando..." />

    <!-- Lado Esquerdo: Checkbox Tático -->
    <div class="flex items-start gap-3 min-w-0 flex-1">
      <button 
        type="button"
        @click.stop="handleComplete"
        class="mt-0.5 w-4.5 h-4.5 rounded border border-borderbase bg-app flex items-center justify-center transition-colors duration-fast focus-visible:ring-2 focus-visible:ring-borderfocus cursor-pointer"
        :class="isCompleted ? 'bg-status-success-bg border-status-success-border text-status-success-text shadow-sm' : 'hover:border-borderfocus group-hover:border-borderfocus'"
        :disabled="isCompleted"
      >
        <Check v-if="isCompleted" class="w-3.5 h-3.5 stroke-[3] animate-check" style="stroke-dasharray: 24;" />
      </button>

      <div class="min-w-0 flex-1">
        <p 
          class="text-sm font-medium text-content group-hover:text-content-accent transition-colors duration-fast truncate"
          :class="{ 'line-through text-content-muted': isCompleted }"
        >
          {{ item.title }}
        </p>

        <!-- Metadados -->
        <div class="mt-1 flex flex-wrap items-center gap-2 text-xs text-content-muted font-mono">
          <span v-if="item.projectName" class="hidden lg:inline text-content-muted">
            [{ item.projectName }]
          </span>

          <TacticBadge v-if="item.deadline" variant="urgent">
            Vence em breve
          </TacticBadge>

          <TacticBadge v-if="isHabit && item.currentStreak > 0" variant="execution">
            <Flame class="w-3 h-3 mr-0.5 inline text-status-success-text" /> {{ item.currentStreak }}d
          </TacticBadge>

          <span class="flex items-center gap-1 bg-surface px-1.5 py-0.5 rounded border border-borderbase">
            <Clock class="w-3 h-3 text-content-muted" /> {{ item.estimatedDurationMinutes }}m
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
        class="opacity-0 group-hover:opacity-100 focus:opacity-100 p-1 rounded text-content-muted hover:text-content hover:bg-surface-hover transition-opacity duration-fast cursor-pointer"
        title="Opções de Edição"
      >
        <MoreHorizontal class="w-4 h-4" />
      </button>
    </div>
  </div>
</template>