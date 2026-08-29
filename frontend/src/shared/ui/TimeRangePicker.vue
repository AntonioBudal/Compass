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
        ✕
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
  padding: var(--space-1) var(--space-2);
  font-size: var(--font-size-sm);
  color: var(--color-text-primary);
  background-color: var(--color-bg-surface);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-sm);
  font-family: inherit;
}

.time-input:focus {
  border-color: var(--color-accent-primary);
  outline: none;
}

.time-input--error {
  border-color: var(--color-danger-border);
}

.range-separator {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
}

.btn-remove {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 1.75rem;
  height: 1.75rem;
  border-radius: var(--radius-sm);
  border: 1px solid transparent;
  background: transparent;
  color: var(--color-text-muted);
  cursor: pointer;
  font-size: var(--font-size-xs);
  transition: color 0.15s ease, background-color 0.15s ease;
}

.btn-remove:hover {
  background-color: var(--color-danger-bg);
  color: var(--color-danger-text);
}

.time-range-error {
  font-size: var(--font-size-xs);
  color: var(--color-danger-text);
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
