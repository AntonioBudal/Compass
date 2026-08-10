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
      return 'bg-status-warning-bg text-status-warning border-status-warning-border';
    case 'strategic':
      return 'bg-surface text-content-accent border-borderfocus';
    case 'execution':
      return 'bg-status-success-bg text-status-success-text border-status-success-border';
    case 'blocked':
      return 'bg-status-danger-bg text-status-danger-text border-status-danger-border';
    case 'neutral':
    default:
      return 'bg-surface text-content-muted border-borderbase';
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