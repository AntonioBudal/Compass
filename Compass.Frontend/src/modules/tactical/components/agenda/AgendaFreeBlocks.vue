<!-- src/components/agenda/AgendaFreeBlocks.vue -->
<script setup lang="ts">
import { PlusCircle } from 'lucide-vue-next';

export interface FreeBlock {
  startMin: number;
  endMin: number;
  duration: number;
}

defineProps<{
  blocks: FreeBlock[];
  pixelsPerMinute: number;
}>();

defineEmits<{
  (e: 'allocate', block: FreeBlock): void;
}>();
</script>

<template>
  <!-- ZONAS VAZIAS (Timeboxing Livre) -->
  <div 
    v-for="(block, index) in blocks" :key="'free-'+index"
    class="absolute left-16 right-4 bg-surface-active/10 border border-dashed border-content-muted/30 hover:border-content-accent hover:bg-surface-active/40 z-10 flex items-center justify-center cursor-pointer transition-all rounded-lg"
    :style="{ top: `${block.startMin * pixelsPerMinute}px`, height: `${block.duration * pixelsPerMinute}px` }"
    @click="$emit('allocate', block)"
  >
    <span class="flex items-center gap-2 text-[10px] font-bold text-content-muted uppercase tracking-widest opacity-0 hover:opacity-100 transition-opacity">
      <PlusCircle class="w-3.5 h-3.5" /> Alocar {{ block.duration }}m Livres
    </span>
  </div>
</template>