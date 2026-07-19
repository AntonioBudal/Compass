<script setup lang="ts">
import { computed } from 'vue';

const props = withDefaults(defineProps<{
  variant?: 'urgent' | 'strategic' | 'execution' | 'neutral' | 'blocked';
  icon?: string;
}>(), {
  variant: 'neutral'
});

const variantClasses = computed(() => {
  switch (props.variant) {
    case 'urgent':
      return 'bg-amber-500/10 text-amber-400 border-amber-500/20';
    case 'strategic':
      return 'bg-indigo-500/10 text-indigo-400 border-indigo-500/20';
    case 'execution':
      return 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20';
    case 'blocked':
      return 'bg-rose-500/10 text-rose-400 border-rose-500/20';
    case 'neutral':
    default:
      return 'bg-zinc-800/80 text-zinc-300 border-zinc-700/50';
  }
});
</script>

<template>
  <span 
    class="inline-flex items-center gap-1.5 px-1.5 py-0.5 rounded text-[11px] font-mono border h-5 leading-none select-none tracking-tight whitespace-nowrap"
    :class="variantClasses"
  >
    <span v-if="icon">{{ icon }}</span>
    <slot />
  </span>
</template>