<template>
  <div class="app-select-group">
    <label v-if="label" :for="id" class="app-select-label">
      {{ label }}
      <span v-if="required" class="required-indicator" aria-hidden="true">*</span>
    </label>
    <div class="select-wrapper">
      <select
        :id="id"
        :value="modelValue"
        :disabled="disabled"
        :aria-invalid="!!error"
        :aria-describedby="error ? `${id}-error` : hint ? `${id}-hint` : undefined"
        class="app-select"
        :class="{ 'app-select--error': !!error }"
        @change="handleChange"
      >
        <option v-if="placeholder" value="" disabled selected>
          {{ placeholder }}
        </option>
        <option
          v-for="option in options"
          :key="option.value"
          :value="option.value"
        >
          {{ option.label }}
        </option>
      </select>
      <div class="select-chevron" aria-hidden="true">
        <svg
          xmlns="http://www.w3.org/2000/svg"
          width="16"
          height="16"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2"
          stroke-linecap="round"
          stroke-linejoin="round"
        >
          <polyline points="6 9 12 15 18 9" />
        </svg>
      </div>
    </div>
    <p v-if="hint && !error" :id="`${id}-hint`" class="app-select-hint">
      {{ hint }}
    </p>
    <p v-if="error" :id="`${id}-error`" class="app-select-error" role="alert">
      {{ error }}
    </p>
  </div>
</template>

<script setup lang="ts">
export interface SelectOption {
  value: string | number
  label: string
}

withDefaults(
  defineProps<{
    modelValue: string | number
    options: SelectOption[]
    id?: string
    label?: string
    placeholder?: string
    disabled?: boolean
    required?: boolean
    error?: string
    hint?: string
  }>(),
  {
    id: () => `select-${Math.random().toString(36).substring(2, 9)}`,
    placeholder: '',
    disabled: false,
    required: false,
    error: '',
    hint: ''
  }
)

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void
}>()

function handleChange(event: Event) {
  const target = event.target as HTMLSelectElement
  emit('update:modelValue', target.value)
}
</script>

<style scoped>
.app-select-group {
  display: flex;
  flex-direction: column;
  gap: var(--space-1);
  width: 100%;
}

.app-select-label {
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-medium);
  color: var(--color-text-secondary);
  line-height: 1.4;
}

.required-indicator {
  color: var(--color-danger);
}

.select-wrapper {
  position: relative;
  width: 100%;
  display: flex;
  align-items: center;
}

.app-select {
  width: 100%;
  padding: var(--space-2) var(--space-8) var(--space-2) var(--space-3);
  font-size: var(--font-size-sm);
  color: var(--color-text-primary);
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  font-family: inherit;
  cursor: pointer;
  appearance: none;
  line-height: 1.4;
  transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
}

.select-chevron {
  position: absolute;
  right: var(--space-3);
  pointer-events: none;
  color: var(--color-text-muted);
  display: flex;
  align-items: center;
  justify-content: center;
}

.app-select:hover:not(:disabled) {
  border-color: var(--color-text-secondary);
}

.app-select:focus {
  border-color: var(--color-accent);
  outline: none;
  box-shadow: var(--focus-ring);
}

.app-select:disabled {
  opacity: 0.6;
  background-color: var(--color-surface-subtle);
  cursor: not-allowed;
}

.app-select--error {
  border-color: var(--color-danger);
}

.app-select--error:focus {
  box-shadow: 0 0 0 2px var(--color-bg-app), 0 0 0 4px var(--color-danger);
}

.app-select-hint {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
}

.app-select-error {
  font-size: var(--font-size-xs);
  color: var(--color-danger-text);
  font-weight: var(--font-weight-medium);
}
</style>
