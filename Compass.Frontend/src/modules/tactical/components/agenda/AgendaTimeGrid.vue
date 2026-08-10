<!-- src/components/agenda/AgendaTimeGrid.vue -->
<script setup lang="ts">
defineProps<{
  pixelsPerMinute: number;
  gridIntervalMinutes: number;
}>();
</script>

<template>
  <!-- GRID VISUAL DINÂMICO -->
  <div 
    v-for="step in (1440 / gridIntervalMinutes)" :key="step"
    class="absolute w-full flex items-start group pointer-events-none z-0" 
    :style="{ top: `${(step - 1) * gridIntervalMinutes * pixelsPerMinute}px` }"
  >
    <span 
      class="w-16 text-right pr-4 font-mono relative"
      :class="[(step - 1) % 2 === 0 ? 'text-[11px] font-bold text-content-muted -top-2' : 'text-[9px] text-content-muted/50 -top-1.5']"
    >
      {{ String(Math.floor(((step - 1) * gridIntervalMinutes) / 60)).padStart(2, '0') }}:{{ String(((step - 1) * gridIntervalMinutes) % 60).padStart(2, '0') }}
    </span>
    <div 
      class="flex-1 border-t"
      :class="[(step - 1) % 2 === 0 ? 'border-borderbase opacity-60' : 'border-borderbase border-dashed opacity-30']"
    />
  </div>
</template>