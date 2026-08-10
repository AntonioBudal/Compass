<script setup lang="ts">
import { Lock, CircleDashed } from 'lucide-vue-next';
import type { CommitmentItem } from '@/modules/tactical/stores/commitmentsStore';

defineProps<{
  items: CommitmentItem[];
}>();

const emit = defineEmits<{
  (e: 'dragstart', event: DragEvent, item: CommitmentItem): void;
  (e: 'dragend', event: DragEvent): void;
  (e: 'drop-to-backlog', itemId: string): void;
}>();

const handleDrop = (event: DragEvent) => {
  const itemId = event.dataTransfer?.getData('text/plain');
  if (itemId) {
    emit('drop-to-backlog', itemId);
  }
};
</script>

<template>
  <!-- FIX: A div não some mais quando vazia, tornando-se uma Drop Zone permanente -->
  <div 
    class="shrink-0 p-4 rounded-lg bg-surface border space-y-3 transition-colors"
    :class="items.length > 0 ? 'border-borderbase' : 'border-dashed border-borderfocus opacity-70'"
    @dragover.prevent
    @dragenter.prevent
    @drop="handleDrop"
  >
    <div v-if="items.length > 0" class="flex flex-wrap gap-2">
      <div 
        v-for="orphan in items" :key="orphan.id" 
        draggable="true"
        @dragstart="emit('dragstart', $event, orphan)"
        @dragend="emit('dragend', $event)"
        class="flex items-center gap-2 text-xs bg-app px-3 py-2 rounded-lg border border-borderbase cursor-grab active:cursor-grabbing shadow-sm hover:border-borderfocus transition-colors"
      >
        <Lock v-if="orphan.type === 'EVENT'" class="w-3.5 h-3.5 text-status-danger-text pointer-events-none" />
        <CircleDashed v-else class="w-3.5 h-3.5 text-content-accent pointer-events-none" />
        <span class="font-medium text-content pointer-events-none">{{ orphan.title }}</span>
        <span class="text-content-muted pointer-events-none">({{ orphan.estimatedDurationMinutes }}m)</span>
      </div>
    </div>
    
    <div v-else class="w-full text-center py-2">
      <span class="text-xs font-mono uppercase tracking-widest text-content-muted">Zona de Drag & Drop Livre</span>
    </div>
  </div>
</template>