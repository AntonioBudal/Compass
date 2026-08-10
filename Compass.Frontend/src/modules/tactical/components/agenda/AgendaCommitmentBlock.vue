<!-- src/components/agenda/AgendaCommitmentBlock.vue -->
<script setup lang="ts">
import { GripHorizontal, Clock } from 'lucide-vue-next';
import type { CommitmentItem } from '@/modules/tactical/stores/commitmentsStore';

const props = defineProps<{
  item: CommitmentItem & { startMin: number; endMin: number };
  pixelsPerMinute: number;
  dragEnabledId: string | null;
  resizingId: string | null;
  previewDuration: number | null;
}>();

const emit = defineEmits<{
  (e: 'update:dragEnabledId', id: string | null): void;
  (e: 'dragstart', event: DragEvent, item: CommitmentItem): void;
  (e: 'dragend', event: DragEvent): void;
  (e: 'resize-start', event: MouseEvent, id: string, currentDuration: number): void;
  (e: 'open-inspector', item: CommitmentItem): void;
}>();

const handleDragStart = (event: DragEvent) => {
  emit('dragstart', event, props.item);
};

const handleDragEnd = (event: DragEvent) => {
  emit('dragend', event);
};

const handleResizeStart = (event: MouseEvent) => {
  emit('resize-start', event, props.item.id, props.item.estimatedDurationMinutes);
};
</script>

<template>
  <div 
    :draggable="dragEnabledId === item.id"
    @dragstart="handleDragStart"
    @dragend="handleDragEnd"
    class="absolute left-16 right-4 rounded-lg flex flex-col justify-start overflow-hidden shadow-sm hover:shadow-md transition-all z-30 group bg-surface"
    :class="[
      dragEnabledId === item.id ? 'cursor-grab active:cursor-grabbing scale-[1.01] z-50 shadow-lg' : '',
      item.type === 'EVENT' ? 'border border-status-danger-border/50 border-l-4 border-l-status-danger-text' : 'border border-borderfocus border-l-4 border-l-content',
      resizingId === item.id ? 'ring-2 ring-content-accent z-40' : ''
    ]"
    :style="{ 
      top: `${item.startMin * pixelsPerMinute}px`, 
      height: `${(resizingId === item.id && previewDuration !== null ? previewDuration : (item.endMin - item.startMin)) * pixelsPerMinute}px`,
      minHeight: '24px' 
    }"
  >
    <!-- ALÇA SUPERIOR -->
    <div 
      class="h-4 w-full flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity bg-content-muted/10 hover:bg-content-muted/20" 
      @mouseenter="emit('update:dragEnabledId', item.id)" 
      @mouseleave="emit('update:dragEnabledId', null)"
    >
      <GripHorizontal class="w-3 h-3 text-content-muted" />
    </div>

    <!-- CORPO -->
    <div 
      class="px-3 pb-3 flex-1 flex flex-col cursor-pointer bg-transparent" 
      @click="emit('open-inspector', item)"
    >
      <div class="flex items-center justify-between gap-2 pointer-events-none">
        <h4 class="text-sm font-bold text-content truncate">{{ item.title }}</h4>
        <span class="text-[10px] font-mono opacity-80 whitespace-nowrap text-content">
          {{ new Date(item.startTime!).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}
          <span v-if="resizingId === item.id" class="text-content-accent">({{ previewDuration }}m)</span>
        </span>
      </div>
      <div v-if="(item.endMin - item.startMin) >= 30" class="flex items-center gap-3 text-[11px] font-mono text-content-muted mt-1 opacity-90 pointer-events-none">
        <span class="flex items-center gap-1"><Clock class="w-3 h-3" /> {{ item.estimatedDurationMinutes }}m</span>
      </div>
    </div>

    <!-- ALÇA INFERIOR -->
    <div 
      v-if="item.type !== 'EVENT'" 
      class="absolute bottom-0 left-0 right-0 h-3 cursor-ns-resize hover:bg-content-accent/20 transition-colors flex justify-center items-center opacity-0 group-hover:opacity-100 z-50" 
      @mousedown.prevent="handleResizeStart"
    >
      <div class="w-10 h-1 rounded-full bg-content-muted pointer-events-none" />
    </div>
  </div>
</template>