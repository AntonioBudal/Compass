<template>
  <AppShell>
    <template #header-actions>
      <div v-if="profile" class="header-tz-info">
        <AppBadge variant="default" size="sm">
          {{ profile.timeZoneId }}
        </AppBadge>
        <button
          type="button"
          class="btn-reconfigure"
          @click="handleReconfigure"
        >
          Reconfigurar
        </button>
      </div>
    </template>

    <div class="today-page">
      <div v-if="isLoading" class="loading-state">
        <div class="spinner" aria-hidden="true" />
        <p>Carregando perfil de calendário...</p>
      </div>

      <div v-else-if="profile" class="dashboard-content">
        <!-- Hero Section: Notion-style Date & Daily Availability -->
        <section class="today-hero">
          <div class="date-header">
            <h1 class="day-of-week">{{ currentDayName }}</h1>
            <p class="full-date">{{ formattedDate }}</p>
          </div>

          <div class="availability-section">
            <h2 class="section-subtitle">Disponibilidade Hoje</h2>
            <div v-if="todayWindows.length > 0" class="today-windows">
              <span
                v-for="(w, idx) in todayWindows"
                :key="idx"
                class="window-badge-primary"
              >
                {{ formatWindowTime(w.startTime) }} - {{ formatWindowTime(w.endTime) }}
              </span>
            </div>
            <p v-else class="no-availability-text">
              Nenhum horário de trabalho configurado para hoje (dia de folga/descanso).
            </p>
          </div>
        </section>

        <hr class="section-divider" />

        <!-- Weekly Availability Grid -->
        <section class="weekly-section">
          <div class="section-header">
            <h2 class="section-title">Grade de Disponibilidade Semanal</h2>
            <p class="section-description">Horários de trabalho baseados no fuso {{ profile.timeZoneId }}.</p>
          </div>

          <div class="weekly-grid">
            <div
              v-for="day in sortedWeeklyRules"
              :key="day.dayOfWeek"
              class="weekly-day-box"
              :class="{ 'weekly-day-box--today': day.dayOfWeek === currentDayOfWeek }"
            >
              <div class="weekly-day-header">
                <span class="weekly-day-name">{{ getDayName(day.dayOfWeek) }}</span>
                <AppBadge v-if="day.dayOfWeek === currentDayOfWeek" variant="accent" size="sm">
                  Hoje
                </AppBadge>
              </div>

              <div v-if="day.windows.length > 0" class="weekly-day-windows">
                <span
                  v-for="(w, idx) in day.windows"
                  :key="idx"
                  class="window-chip"
                >
                  {{ formatWindowTime(w.startTime) }} - {{ formatWindowTime(w.endTime) }}
                </span>
              </div>
              <span v-else class="weekly-day-empty">Sem disponibilidade</span>
            </div>
          </div>
        </section>
      </div>
    </div>
  </AppShell>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import AppShell from '@/shared/ui/AppShell.vue'
import AppBadge from '@/shared/ui/AppBadge.vue'
import { useScheduleProfileQuery } from '@/entities/schedule-profile/model/useScheduleProfileQuery'
import { profileStorage } from '@/entities/schedule-profile/model/profileStorage'
import type { TimeWindow } from '@/entities/schedule-profile/api/types'

const router = useRouter()
const { data: profile, isLoading } = useScheduleProfileQuery()

const now = new Date()
const currentDayOfWeek = now.getDay() // 0 = Sunday, 1 = Monday, etc.

const DAY_NAMES: Record<number, string> = {
  0: 'Domingo',
  1: 'Segunda-feira',
  2: 'Terça-feira',
  3: 'Quarta-feira',
  4: 'Quinta-feira',
  5: 'Sexta-feira',
  6: 'Sábado'
}

const currentDayName = computed(() => DAY_NAMES[currentDayOfWeek])

const formattedDate = computed(() => {
  return now.toLocaleDateString('pt-BR', {
    day: 'numeric',
    month: 'long',
    year: 'numeric'
  })
})

const todayRule = computed(() => {
  if (!profile.value?.weeklyAvailability) return null
  return profile.value.weeklyAvailability.find((r) => r.dayOfWeek === currentDayOfWeek)
})

const todayWindows = computed<TimeWindow[]>(() => {
  return todayRule.value?.windows || []
})

const sortedWeeklyRules = computed(() => {
  if (!profile.value?.weeklyAvailability) return []
  // Sort starting from Monday (1) to Sunday (0)
  const daysOrder = [1, 2, 3, 4, 5, 6, 0]
  return daysOrder.map((dayNum) => {
    const existing = profile.value!.weeklyAvailability.find((r) => r.dayOfWeek === dayNum)
    return existing || { dayOfWeek: dayNum, windows: [] }
  })
})

function getDayName(dayOfWeek: number): string {
  return DAY_NAMES[dayOfWeek] || `Dia ${dayOfWeek}`
}

function formatWindowTime(time: string): string {
  if (!time) return ''
  return time.substring(0, 5) // "09:00:00" -> "09:00"
}

function handleReconfigure() {
  profileStorage.clearActiveProfileId()
  router.push('/onboarding')
}
</script>

<style scoped>
.today-page {
  display: flex;
  flex-direction: column;
  gap: var(--space-6);
  max-width: 960px;
  margin: 0 auto;
  width: 100%;
}

.header-tz-info {
  display: flex;
  align-items: center;
  gap: var(--space-3);
}

.btn-reconfigure {
  background: transparent;
  border: none;
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-medium);
  color: var(--color-text-secondary);
  cursor: pointer;
  padding: var(--space-1) var(--space-2);
  border-radius: var(--radius-sm);
  transition: color var(--transition-fast), background-color var(--transition-fast);
}

.btn-reconfigure:hover {
  color: var(--color-text-primary);
  background-color: var(--color-surface-hover);
}

.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--space-3);
  min-height: 240px;
  color: var(--color-text-secondary);
}

.spinner {
  width: 24px;
  height: 24px;
  border: 2px solid var(--color-border);
  border-top-color: var(--color-accent);
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.dashboard-content {
  display: flex;
  flex-direction: column;
  gap: var(--space-6);
}

.today-hero {
  display: flex;
  flex-direction: column;
  gap: var(--space-5);
}

.date-header {
  display: flex;
  flex-direction: column;
  gap: var(--space-1);
}

.day-of-week {
  font-size: var(--font-size-2xl);
  font-weight: var(--font-weight-bold);
  color: var(--color-text-primary);
  letter-spacing: -0.02em;
}

.full-date {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
}

.availability-section {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
  background-color: var(--color-surface-subtle);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: var(--space-4) var(--space-5);
}

.section-subtitle {
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-semibold);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-secondary);
}

.today-windows {
  display: flex;
  gap: var(--space-2);
  flex-wrap: wrap;
  margin-top: var(--space-1);
}

.window-badge-primary {
  padding: 4px var(--space-3);
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  color: var(--color-text-primary);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-medium);
  font-variant-numeric: tabular-nums;
}

.no-availability-text {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  line-height: 1.4;
}

.section-divider {
  border: none;
  border-top: 1px solid var(--color-border);
  margin: 0;
}

.weekly-section {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.section-header {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.section-title {
  font-size: var(--font-size-lg);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-primary);
}

.section-description {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
}

.weekly-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(130px, 1fr));
  gap: var(--space-3);
}

.weekly-day-box {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: var(--space-3);
  transition: border-color var(--transition-fast);
}

.weekly-day-box--today {
  border-color: var(--color-accent);
  background-color: var(--color-surface-subtle);
}

.weekly-day-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.weekly-day-name {
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-primary);
}

.weekly-day-windows {
  display: flex;
  flex-direction: column;
  gap: var(--space-1);
}

.window-chip {
  font-size: var(--font-size-xs);
  color: var(--color-text-secondary);
  font-variant-numeric: tabular-nums;
}

.weekly-day-empty {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
}
</style>
