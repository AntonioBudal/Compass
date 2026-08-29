<template>
  <div class="time-range-picker">
    <div class="range-inputs">
      <div class="time-field">
        <label :for="`${id}-start`" class="sr-only">Horário Inicial</label>
        <input
          :id="`${id}-start`"
          type="time"
          :value="startTime"
          class="time-input"
          :class="{ 'time-input--error': !!error }"
          aria-label="Horário inicial"
          @input="handleStartChange"
        />
      </div>
      <span class="range-separator" aria-hidden="true">até</span>
      <div class="time-field">
        <label :for="`${id}-end`" class="sr-only">Horário Final</label>
        <input
          :id="`${id}-end`"
          type="time"
          :value="endTime"
          class="time-input"
          :class="{ 'time-input--error': !!error }"
          aria-label="Horário final"
          @input="handleEndChange"
        />
      </div>
      <button
        v-if="canRemove"
        type="button"
        class="btn-remove"
        aria-label="Remover este intervalo de horário"
        title="Remover intervalo"
        @click="$emit('remove')"
      >
        <svg
          xmlns="http://www.w3.org/2000/svg"
          width="14"
          height="14"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2"
          stroke-linecap="round"
          stroke-linejoin="round"
          aria-hidden="true"
        >
          <line x1="18" y1="6" x2="6" y2="18" />
          <line x1="6" y1="6" x2="18" y2="18" />
        </svg>
      </button>
    </div>
    <p v-if="error" class="time-range-error" role="alert">
      {{ error }}
    </p>
  </div>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    startTime: string
    endTime: string
    id?: string
    canRemove?: boolean
    error?: string
  }>(),
  {
    id: () => `timerange-${Math.random().toString(36).substring(2, 9)}`,
    canRemove: true,
    error: ''
  }
)

const emit = defineEmits<{
  (e: 'update:startTime', value: string): void
  (e: 'update:endTime', value: string): void
  (e: 'remove'): void
}>()

function handleStartChange(e: Event) {
  const target = e.target as HTMLInputElement
  emit('update:startTime', target.value)
}

function handleEndChange(e: Event) {
  const target = e.target as HTMLInputElement
  emit('update:endTime', target.value)
}
</script>

<style scoped>
.time-range-picker {
  display: flex;
  flex-direction: column;
  gap: var(--space-1);
}

.range-inputs {
  display: flex;
  align-items: center;
  gap: var(--space-2);
}

.time-input {
  padding: 4px var(--space-2);
  font-size: var(--font-size-sm);
  color: var(--color-text-primary);
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  font-family: inherit;
  line-height: 1.4;
  transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
}

.time-input:hover {
  border-color: var(--color-text-secondary);
}

.time-input:focus {
  border-color: var(--color-accent);
  outline: none;
  box-shadow: var(--focus-ring);
}

.time-input--error {
  border-color: var(--color-danger);
}

.range-separator {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
}

.btn-remove {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border-radius: var(--radius-sm);
  border: 1px solid transparent;
  background: transparent;
  color: var(--color-text-muted);
  cursor: pointer;
  transition: color var(--transition-fast), background-color var(--transition-fast);
}

.btn-remove:hover {
  background-color: var(--color-danger-subtle);
  color: var(--color-danger-text);
}

.time-range-error {
  font-size: var(--font-size-xs);
  color: var(--color-danger-text);
  font-weight: var(--font-weight-medium);
}

.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}
</style>
