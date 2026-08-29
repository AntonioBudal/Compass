<template>
  <button
    :type="type"
    :disabled="disabled || loading"
    :class="['app-btn', `app-btn--${variant}`, { 'app-btn--loading': loading }]"
    @click="$emit('click', $event)"
  >
    <span v-if="loading" class="spinner" aria-hidden="true"></span>
    <slot />
  </button>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    type?: 'button' | 'submit' | 'reset'
    variant?: 'primary' | 'secondary' | 'outline' | 'ghost' | 'danger'
    disabled?: boolean
    loading?: boolean
  }>(),
  {
    type: 'button',
    variant: 'primary',
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
  padding: var(--space-2) var(--space-4);
  font-size: var(--font-size-sm);
  font-weight: 600;
  border-radius: var(--radius-md);
  border: 1px solid transparent;
  cursor: pointer;
  transition: background-color 0.15s ease, border-color 0.15s ease, opacity 0.15s ease;
  font-family: inherit;
}

.app-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.app-btn--primary {
  background-color: var(--color-accent-primary);
  color: var(--color-accent-text);
}

.app-btn--primary:hover:not(:disabled) {
  background-color: var(--color-accent-hover);
}

.app-btn--secondary {
  background-color: var(--color-bg-elevated);
  color: var(--color-text-primary);
}

.app-btn--secondary:hover:not(:disabled) {
  background-color: #475569;
}

.app-btn--outline {
  background-color: transparent;
  border-color: var(--color-border-subtle);
  color: var(--color-text-primary);
}

.app-btn--outline:hover:not(:disabled) {
  background-color: var(--color-bg-surface);
  border-color: var(--color-border-focus);
}

.app-btn--ghost {
  background-color: transparent;
  color: var(--color-text-secondary);
}

.app-btn--ghost:hover:not(:disabled) {
  background-color: var(--color-bg-surface);
  color: var(--color-text-primary);
}

.app-btn--danger {
  background-color: var(--color-danger-bg);
  border-color: var(--color-danger-border);
  color: var(--color-danger-text);
}

.app-btn--danger:hover:not(:disabled) {
  background-color: rgba(239, 68, 68, 0.2);
}

.spinner {
  width: 1rem;
  height: 1rem;
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
