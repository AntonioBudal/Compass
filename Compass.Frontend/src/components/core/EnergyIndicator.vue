<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  level: number; // 1 = Baixa | 2 = Média | 3 = Alta
}>();

const visualBlocks = computed(() => {
  switch (props.level) {
    case 3: return '■■■ DEEP';
    case 1: return '■□□ MAINT';
    case 2:
    default: return '■■□ OPER';
  }
});
</script>

<template>
  <span 
    class="inline-flex items-center gap-1.5 px-1.5 py-0.5 rounded text-[11px] font-mono border border-zinc-700/80 bg-zinc-900 text-zinc-300 leading-none select-none tracking-tight whitespace-nowrap"
    :title="`Nível de Energia Requerido: ${level}`"
  >
    <span class="font-bold tracking-widest text-zinc-100">{{ visualBlocks.split(' ')[0] }}</span>
    <span class="text-[9px] uppercase tracking-normal text-zinc-500">{{ visualBlocks.split(' ')[1] }}</span>
  </span>
</template>