<template>
  <div class="app-input-group">
    <label v-if="label" :for="id" class="app-input-label">
      {{ label }}
      <span v-if="required" class="required-indicator" aria-hidden="true">*</span>
    </label>
    <input
      :id="id"
      :type="type"
      :value="modelValue"
      :placeholder="placeholder"
      :disabled="disabled"
      :aria-invalid="!!error"
      :aria-describedby="error ? `${id}-error` : hint ? `${id}-hint` : undefined"
      class="app-input"
      :class="{ 'app-input--error': !!error }"
      @input="handleInput"
    />
    <p v-if="hint && !error" :id="`${id}-hint`" class="app-input-hint">
      {{ hint }}
    </p>
    <p v-if="error" :id="`${id}-error`" class="app-input-error" role="alert">
      {{ error }}
    </p>
  </div>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    modelValue: string | number
    id?: string
    label?: string
    type?: string
    placeholder?: string
    disabled?: boolean
    required?: boolean
    error?: string
    hint?: string
  }>(),
  {
    id: () => `input-${Math.random().toString(36).substring(2, 9)}`,
    type: 'text',
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

function handleInput(event: Event) {
  const target = event.target as HTMLInputElement
  emit('update:modelValue', target.value)
}
</script>

<style scoped>
.app-input-group {
  display: flex;
  flex-direction: column;
  gap: var(--space-1);
  width: 100%;
}

.app-input-label {
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-medium);
  color: var(--color-text-secondary);
  line-height: 1.4;
}

.required-indicator {
  color: var(--color-danger);
}

.app-input {
  width: 100%;
  padding: var(--space-2) var(--space-3);
  font-size: var(--font-size-sm);
  color: var(--color-text-primary);
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  font-family: inherit;
  line-height: 1.4;
  transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
}

.app-input::placeholder {
  color: var(--color-text-muted);
}

.app-input:hover:not(:disabled) {
  border-color: var(--color-text-secondary);
}

.app-input:focus {
  border-color: var(--color-accent);
  outline: none;
  box-shadow: var(--focus-ring);
}

.app-input:disabled {
  opacity: 0.6;
  background-color: var(--color-surface-subtle);
  cursor: not-allowed;
}

.app-input--error {
  border-color: var(--color-danger);
}

.app-input--error:focus {
  box-shadow: 0 0 0 2px var(--color-bg-app), 0 0 0 4px var(--color-danger);
}

.app-input-hint {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
}

.app-input-error {
  font-size: var(--font-size-xs);
  color: var(--color-danger-text);
  font-weight: var(--font-weight-medium);
}
</style>
