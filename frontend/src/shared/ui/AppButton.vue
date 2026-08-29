<template>
  <button
    :type="type"
    :disabled="disabled || loading"
    :class="[
      'app-btn',
      `app-btn--${variant}`,
      `app-btn--${size}`,
      { 'app-btn--loading': loading }
    ]"
    @click="$emit('click', $event)"
  >
    <span v-if="loading" class="spinner" aria-hidden="true" />
    <slot />
  </button>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    type?: 'button' | 'submit' | 'reset'
    variant?: 'primary' | 'secondary' | 'outline' | 'ghost' | 'danger'
    size?: 'sm' | 'md'
    disabled?: boolean
    loading?: boolean
  }>(),
  {
    type: 'button',
    variant: 'primary',
    size: 'md',
    disabled: false,
    loading: false
  }
)

defineEmits<{
  (e: 'click', event: MouseEvent): void
}>()
</script>

<style scoped>
.app-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-2);
  font-weight: var(--font-weight-medium);
  border-radius: var(--radius-md);
  border: 1px solid transparent;
  cursor: pointer;
  transition: background-color var(--transition-fast), border-color var(--transition-fast), color var(--transition-fast);
  font-family: inherit;
  line-height: 1.4;
  white-space: nowrap;
}

.app-btn--sm {
  padding: 4px var(--space-3);
  font-size: var(--font-size-xs);
}

.app-btn--md {
  padding: 6px var(--space-4);
  font-size: var(--font-size-sm);
}

.app-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Primary */
.app-btn--primary {
  background-color: var(--color-accent);
  color: var(--color-accent-text);
  border-color: var(--color-accent);
}

.app-btn--primary:hover:not(:disabled) {
  background-color: var(--color-accent-hover);
  border-color: var(--color-accent-hover);
}

/* Secondary & Outline */
.app-btn--secondary,
.app-btn--outline {
  background-color: var(--color-surface-subtle);
  border-color: var(--color-border);
  color: var(--color-text-primary);
}

.app-btn--secondary:hover:not(:disabled),
.app-btn--outline:hover:not(:disabled) {
  background-color: var(--color-surface-hover);
  border-color: var(--color-border);
}

/* Ghost */
.app-btn--ghost {
  background-color: transparent;
  color: var(--color-text-secondary);
}

.app-btn--ghost:hover:not(:disabled) {
  background-color: var(--color-surface-hover);
  color: var(--color-text-primary);
}

/* Danger */
.app-btn--danger {
  background-color: var(--color-danger-subtle);
  border-color: var(--color-danger-subtle);
  color: var(--color-danger-text);
}

.app-btn--danger:hover:not(:disabled) {
  background-color: var(--color-danger);
  border-color: var(--color-danger);
  color: var(--color-accent-text);
}

/* Spinner */
.spinner {
  width: 0.875rem;
  height: 0.875rem;
  border: 2px solid currentColor;
  border-right-color: transparent;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
