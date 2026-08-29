<template>
  <div class="step-confirmation">
    <div class="header-section">
      <h2 class="title">Confirme seu Perfil</h2>
      <p class="subtitle">
        Revise as configurações abaixo antes de finalizar o onboarding.
      </p>
    </div>

    <div v-if="error" class="error-alert" role="alert">
      {{ error }}
    </div>

    <div class="summary-card">
      <div class="summary-section">
        <span class="section-label">Fuso Horário</span>
        <div class="section-value timezone-value">
          <div class="timezone-icon" aria-hidden="true">
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="12" cy="12" r="10" />
              <line x1="2" y1="12" x2="22" y2="12" />
              <path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z" />
            </svg>
          </div>
          <strong>{{ timeZoneId }}</strong>
        </div>
      </div>

      <div class="summary-section">
        <span class="section-label">Disponibilidade Semanal</span>
        <div class="availability-list">
          <div
            v-for="day in enabledDays"
            :key="day.dayOfWeek"
            class="day-summary-row"
          >
            <span class="day-label">{{ day.name }}</span>
            <div class="windows-summary">
              <span
                v-for="(w, idx) in day.windows"
                :key="idx"
                class="window-badge"
              >
                {{ w.startTime }} às {{ w.endTime }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="actions">
      <AppButton variant="secondary" :disabled="loading" @click="$emit('back')">
        Voltar e Editar
      </AppButton>
      <AppButton
        variant="primary"
        :loading="loading"
        @click="$emit('confirm')"
      >
        Confirmar e Concluir
      </AppButton>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import AppButton from '@/shared/ui/AppButton.vue'
import type { DayConfig } from '../model/onboardingState'

const props = defineProps<{
  timeZoneId: string
  days: DayConfig[]
  loading?: boolean
  error?: string
}>()

defineEmits<{
  (e: 'confirm'): void
  (e: 'back'): void
}>()

const enabledDays = computed(() => props.days.filter((d) => d.enabled))
</script>

<style scoped>
.step-confirmation {
  display: flex;
  flex-direction: column;
  gap: var(--space-5);
  max-width: 520px;
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

.error-alert {
  padding: var(--space-3) var(--space-4);
  background-color: var(--color-danger-subtle);
  border: 1px solid var(--color-danger);
  border-radius: var(--radius-md);
  color: var(--color-danger-text);
  font-size: var(--font-size-sm);
}

.summary-card {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: var(--space-5);
}

.summary-section {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
}

.section-label {
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-semibold);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-muted);
}

.timezone-value {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  font-size: var(--font-size-sm);
  color: var(--color-text-primary);
}

.timezone-icon {
  display: flex;
  align-items: center;
  color: var(--color-text-secondary);
}

.availability-list {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
}

.day-summary-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-2) 0;
  border-bottom: 1px solid var(--color-border-subtle);
}

.day-summary-row:last-child {
  border-bottom: none;
}

.day-label {
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-medium);
  color: var(--color-text-primary);
}

.windows-summary {
  display: flex;
  gap: var(--space-2);
  flex-wrap: wrap;
}

.window-badge {
  padding: 2px var(--space-2);
  background-color: var(--color-surface-subtle);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-xs);
  color: var(--color-text-secondary);
}

.actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: var(--space-2);
}
</style>
