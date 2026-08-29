<template>
  <div class="app-select-group">
    <label v-if="label" :for="id" class="app-select-label">
      {{ label }}
      <span v-if="required" class="required-indicator" aria-hidden="true">*</span>
    </label>
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
}

.app-select-label {
  font-size: var(--font-size-sm);
  font-weight: 500;
  color: var(--color-text-secondary);
}

.required-indicator {
  color: var(--color-danger-border);
}

.app-select {
  width: 100%;
  padding: var(--space-2) var(--space-3);
  font-size: var(--font-size-sm);
  color: var(--color-text-primary);
  background-color: var(--color-bg-surface);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-md);
  font-family: inherit;
  cursor: pointer;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
}

.app-select:hover:not(:disabled) {
  border-color: var(--color-text-muted);
}

.app-select:focus {
  border-color: var(--color-accent-primary);
  outline: none;
}

.app-select:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.app-select--error {
  border-color: var(--color-danger-border);
}

.app-select-hint {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
}

.app-select-error {
  font-size: var(--font-size-xs);
  color: var(--color-danger-text);
  font-weight: 500;
}
</style>
