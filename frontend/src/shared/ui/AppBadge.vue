<template>
  <span
    class="app-badge"
    :class="[
      `app-badge--${variant}`,
      `app-badge--${size}`
    ]"
  >
    <slot />
  </span>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    variant?: 'default' | 'accent' | 'success' | 'warning' | 'danger'
    size?: 'sm' | 'md'
  }>(),
  {
    variant: 'default',
    size: 'sm',
  }
)
</script>

<style scoped>
.app-badge {
  display: inline-flex;
  align-items: center;
  gap: var(--space-1);
  font-weight: var(--font-weight-medium);
  border-radius: var(--radius-full);
  line-height: 1;
  white-space: nowrap;
}

/* Sizes */
.app-badge--sm {
  font-size: var(--font-size-xs);
  padding: 2px var(--space-2);
}

.app-badge--md {
  font-size: var(--font-size-sm);
  padding: 4px var(--space-3);
}

/* Variants */
.app-badge--default {
  background-color: var(--color-surface-subtle);
  color: var(--color-text-secondary);
  border: 1px solid var(--color-border);
}

.app-badge--accent {
  background-color: var(--color-accent-subtle);
  color: var(--color-accent);
  border: 1px solid var(--color-accent-subtle);
}

.app-badge--success {
  background-color: var(--color-success-subtle);
  color: var(--color-success-text);
  border: 1px solid var(--color-success-subtle);
}

.app-badge--warning {
  background-color: var(--color-warning-subtle);
  color: var(--color-warning-text);
  border: 1px solid var(--color-warning-subtle);
}

.app-badge--danger {
  background-color: var(--color-danger-subtle);
  color: var(--color-danger-text);
  border: 1px solid var(--color-danger-subtle);
}
</style>
