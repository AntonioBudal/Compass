<template>
  <div class="step-availability">
    <div class="header-section">
      <h2 class="title">Disponibilidade Semanal Padrão</h2>
      <p class="subtitle">
        Defina os dias da semana e os blocos de horário em que você costuma estar disponível.
      </p>
    </div>

    <div v-if="validationError" class="validation-alert" role="alert">
      {{ validationError }}
    </div>

    <div class="days-container">
      <div
        v-for="day in days"
        :key="day.dayOfWeek"
        class="day-card"
        :class="{ 'day-card--enabled': day.enabled }"
      >
        <div class="day-header">
          <label class="day-toggle-label">
            <input
              type="checkbox"
              :checked="day.enabled"
              class="day-checkbox"
              @change="toggleDay(day)"
            />
            <span class="day-name">{{ day.name }}</span>
          </label>
          <span v-if="day.enabled" class="window-count">
            {{ day.windows.length }} {{ day.windows.length === 1 ? 'intervalo' : 'intervalos' }}
          </span>
          <span v-else class="day-disabled-text">Sem disponibilidade</span>
        </div>

        <div v-if="day.enabled" class="day-body">
          <div class="windows-list">
            <div
              v-for="(window, idx) in day.windows"
              :key="idx"
              class="window-row"
            >
              <TimeRangePicker
                v-model:start-time="window.startTime"
                v-model:end-time="window.endTime"
                :can-remove="day.windows.length > 1"
                :error="getWindowError(window)"
                @remove="removeWindow(day, idx)"
              />
            </div>
          </div>

          <button
            type="button"
            class="btn-add-window"
            @click="addWindow(day)"
          >
            Adicionar Intervalo
          </button>
        </div>
      </div>
    </div>

    <div class="actions">
      <AppButton variant="secondary" @click="$emit('back')">
        Voltar
      </AppButton>
      <AppButton variant="primary" @click="handleNext">
        Avançar para Confirmação
      </AppButton>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import AppButton from '@/shared/ui/AppButton.vue'
import TimeRangePicker from '@/shared/ui/TimeRangePicker.vue'
import type { DayConfig } from '../model/onboardingState'
import { validateAvailability } from '../model/onboardingState'

const props = defineProps<{
  days: DayConfig[]
}>()

const emit = defineEmits<{
  (e: 'next'): void
  (e: 'back'): void
}>()

const validationError = ref<string | null>(null)

function toggleDay(day: DayConfig) {
  day.enabled = !day.enabled
  if (day.enabled && day.windows.length === 0) {
    day.windows.push({ startTime: '09:00', endTime: '18:00' })
  }
}

function addWindow(day: DayConfig) {
  const lastWindow = day.windows[day.windows.length - 1]
  const startTime = lastWindow ? lastWindow.endTime : '09:00'
  const endTime = '18:00'
  day.windows.push({ startTime, endTime })
}

function removeWindow(day: DayConfig, index: number) {
  day.windows.splice(index, 1)
}

function getWindowError(window: { startTime: string; endTime: string }): string | undefined {
  if (window.startTime && window.endTime && window.startTime >= window.endTime) {
    return 'O horário de início deve ser anterior ao término.'
  }
  return undefined
}

function handleNext() {
  const error = validateAvailability(props.days)
  if (error) {
    validationError.value = error
    return
  }
  validationError.value = null
  emit('next')
}
</script>

<style scoped>
.step-availability {
  display: flex;
  flex-direction: column;
  gap: var(--space-5);
  max-width: 600px;
  margin: 0 auto;
}

.header-section {
  text-align: center;
  display: flex;
  flex-direction: column;
  gap: var(--space-1);
}

.title {
  font-size: var(--font-size-xl);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-primary);
  letter-spacing: -0.01em;
}

.subtitle {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  line-height: 1.5;
}

.validation-alert {
  padding: var(--space-3) var(--space-4);
  background-color: var(--color-danger-subtle);
  border: 1px solid var(--color-danger);
  border-radius: var(--radius-md);
  color: var(--color-danger-text);
  font-size: var(--font-size-sm);
}

.days-container {
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
}

.day-card {
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: var(--space-3) var(--space-4);
  transition: border-color var(--transition-fast), background-color var(--transition-fast);
}

.day-card--enabled {
  border-color: var(--color-border);
  background-color: var(--color-surface);
}

.day-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.day-toggle-label {
  display: flex;
  align-items: center;
  gap: var(--space-3);
  cursor: pointer;
}

.day-checkbox {
  width: 1rem;
  height: 1rem;
  accent-color: var(--color-accent);
  cursor: pointer;
}

.day-name {
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-medium);
  color: var(--color-text-primary);
}

.window-count {
  font-size: var(--font-size-xs);
  color: var(--color-text-secondary);
  font-weight: var(--font-weight-medium);
}

.day-disabled-text {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
}

.day-body {
  margin-top: var(--space-3);
  padding-top: var(--space-3);
  border-top: 1px solid var(--color-border-subtle);
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
}

.windows-list {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
}

.btn-add-window {
  align-self: flex-start;
  background: var(--color-surface-subtle);
  border: 1px dashed var(--color-border);
  color: var(--color-text-secondary);
  padding: 4px var(--space-3);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-medium);
  cursor: pointer;
  transition: background-color var(--transition-fast), border-color var(--transition-fast), color var(--transition-fast);
}

.btn-add-window:hover {
  background-color: var(--color-surface-hover);
  border-color: var(--color-text-secondary);
  color: var(--color-text-primary);
}

.actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: var(--space-2);
}
</style>
