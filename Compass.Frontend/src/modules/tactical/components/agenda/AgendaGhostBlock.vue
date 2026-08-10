<!-- src/components/agenda/AgendaGhostBlock.vue -->
<script setup lang="ts">
defineProps<{
  isActive: boolean;
  startMin: number;
  durationMin: number;
  title: string;
  type: string;
  pixelsPerMinute: number;
}>();
</script>

<template>
  <div 
    v-if="isActive"
    class="absolute left-16 right-4 rounded-lg border-2 border-dashed flex flex-col justify-start overflow-hidden z-40 pointer-events-none transition-all duration-75 shadow-xl"
    :class="[
      type === 'EVENT' ? 'bg-status-danger-bg/80 border-status-danger-text' : 'bg-surface-active border-content-accent'
    ]"
    :style="{ 
      top: `${startMin * pixelsPerMinute}px`, 
      height: `${durationMin * pixelsPerMinute}px`,
    }"
  >
    <div class="p-3 opacity-100">
      <h4 class="text-sm font-bold text-content truncate">{{ title }}</h4>
      <span class="text-[10px] font-mono font-bold text-content-accent uppercase tracking-widest bg-app px-1 rounded border border-borderbase">
        Soltar em: {{ String(Math.floor(startMin / 60)).padStart(2, '0') }}:{{ String(startMin % 60).padStart(2, '0') }}
      </span>
    </div>
  </div>
</template>