<!-- src/components/agenda/AgendaRestBlocks.vue -->
<script setup lang="ts">
import { Moon } from 'lucide-vue-next';

defineProps<{
  blocks: { start: number; end: number; label: string }[];
  pixelsPerMinute: number;
}>();
</script>

<template>
  <!-- BLOCOS DE DESCANSO -->
  <div 
    v-for="(block, index) in blocks" :key="'rest-' + index"
    class="absolute left-16 right-0 bg-pattern-rest pointer-events-none z-0 border-y border-borderbase flex items-start p-4"
    :style="{ top: `${block.start * pixelsPerMinute}px`, height: `${(block.end - block.start) * pixelsPerMinute}px` }"
  >
    <div class="px-2 py-1 bg-surface backdrop-blur border border-borderbase rounded-md text-[10px] font-mono text-content-muted uppercase font-bold tracking-widest shadow-sm flex items-center gap-1.5">
      <Moon class="w-3 h-3" /> {{ block.label }}
    </div>
  </div>
</template>

<style scoped>
/* O CSS isolado para o hachurado de descanso */
.bg-pattern-rest {
  background-image: repeating-linear-gradient(
    -45deg,
    transparent,
    transparent 6px,
    var(--color-border-base) 6px,
    var(--color-border-base) 7px
  );
  opacity: 0.3;
}
</style>