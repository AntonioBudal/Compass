<!-- src/components/agenda/AgendaBacklog.vue -->
<script setup lang="ts">
import { Lock, CircleDashed } from 'lucide-vue-next';
import type { CommitmentItem } from '@/stores/commitmentsStore';

defineProps<{
  items: CommitmentItem[];
}>();

defineEmits<{
  (e: 'dragstart', event: DragEvent, item: CommitmentItem): void;
  (e: 'dragend', event: DragEvent): void;
}>();
</script>

<template>
  <div v-if="items.length > 0" class="shrink-0 p-4 rounded-lg bg-surface border border-borderbase space-y-3">
    <div class="flex flex-wrap gap-2">
      <div 
        v-for="orphan in items" :key="orphan.id" 
        draggable="true"
        @dragstart="$emit('dragstart', $event, orphan)"
        @dragend="$emit('dragend', $event)"
        class="flex items-center gap-2 text-xs bg-app px-3 py-2 rounded-lg border border-borderbase cursor-grab active:cursor-grabbing shadow-sm hover:border-borderfocus transition-colors"
      >
        <Lock v-if="orphan.type === 'EVENT'" class="w-3.5 h-3.5 text-status-danger-text pointer-events-none" />
        <CircleDashed v-else class="w-3.5 h-3.5 text-content-accent pointer-events-none" />
        <span class="font-medium text-content pointer-events-none">{{ orphan.title }}</span>
        <span class="text-content-muted pointer-events-none">({{ orphan.estimatedDurationMinutes }}m)</span>
      </div>
    </div>
  </div>
</template>