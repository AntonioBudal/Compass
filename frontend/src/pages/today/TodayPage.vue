<template>
  <div class="today-layout">
    <header class="today-header">
      <div class="header-brand">
        <span class="brand-icon" aria-hidden="true">🧭</span>
        <h1 class="brand-title">Compass</h1>
      </div>
      <div v-if="profile" class="header-actions">
        <span class="tz-badge" :title="`Fuso ativo: ${profile.timeZoneId}`">
          🌐 {{ profile.timeZoneId }}
        </span>
        <button
          type="button"
          class="btn-reconfigure"
          @click="handleReconfigure"
        >
          Reconfigurar Perfil
        </button>
      </div>
    </header>

    <main class="today-main">
      <div v-if="isLoading" class="loading-state">
        <div class="spinner" aria-hidden="true"></div>
        <p>Carregando perfil de calendário...</p>
      </div>

      <div v-else-if="profile" class="dashboard-content">
        <section class="today-hero">
          <div class="today-date-box">
            <span class="day-of-week">{{ currentDayName }}</span>
            <span class="full-date">{{ formattedDate }}</span>
          </div>

          <div class="availability-status-box">
            <span class="status-label">Disponibilidade Hoje:</span>
            <div v-if="todayWindows.length > 0" class="today-windows">
              <span
                v-for="(w, idx) in todayWindows"
                :key="idx"
                class="today-window-tag"
              >
                {{ formatWindowTime(w.startTime) }} - {{ formatWindowTime(w.endTime) }}
              </span>
            </div>
            <p v-else class="no-availability-text">
              Nenhum horário de trabalho configurado para hoje (dia de folga/descanso).
            </p>
          </div>
        </section>

        <section class="weekly-overview">
          <h2 class="section-title">Grade de Disponibilidade Semanal</h2>
          <div class="weekly-grid">
            <div
              v-for="day in sortedWeeklyRules"
              :key="day.dayOfWeek"
              class="weekly-day-card"
              :class="{ 'weekly-day-card--today': day.dayOfWeek === currentDayOfWeek }"
            >
              <span class="weekly-day-name">{{ getDayName(day.dayOfWeek) }}</span>
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
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
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
.today-layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background-color: var(--color-bg-primary);
}

.today-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-4) var(--space-8);
  border-bottom: 1px solid var(--color-border-subtle);
}

.header-brand {
  display: flex;
  align-items: center;
  gap: var(--space-2);
}

.brand-icon {
  font-size: 1.5rem;
}

.brand-title {
  font-size: var(--font-size-lg);
  font-weight: 700;
  color: var(--color-text-primary);
}

.header-actions {
  display: flex;
  align-items: center;
  gap: var(--space-3);
}

.tz-badge {
  font-size: var(--font-size-xs);
  padding: var(--space-1) var(--space-3);
  background-color: var(--color-bg-surface);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-full);
  color: var(--color-text-secondary);
}

.btn-reconfigure {
  background: transparent;
  border: none;
  font-size: var(--font-size-xs);
  color: var(--color-accent-primary);
  cursor: pointer;
  text-decoration: underline;
}

.today-main {
  flex: 1;
  padding: var(--space-8);
  max-width: 960px;
  margin: 0 auto;
  width: 100%;
}

.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--space-4);
  min-height: 300px;
  color: var(--color-text-secondary);
}

.spinner {
  width: 2rem;
  height: 2rem;
  border: 3px solid var(--color-border-subtle);
  border-top-color: var(--color-accent-primary);
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.dashboard-content {
  display: flex;
  flex-direction: column;
  gap: var(--space-8);
}

.today-hero {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
  background-color: var(--color-bg-surface);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-6);
}

.today-date-box {
  display: flex;
  flex-direction: column;
}

.day-of-week {
  font-size: var(--font-size-2xl);
  font-weight: 700;
  color: var(--color-text-primary);
}

.full-date {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
}

.availability-status-box {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
  padding-top: var(--space-4);
  border-top: 1px solid var(--color-border-subtle);
}

.status-label {
  font-size: var(--font-size-xs);
  font-weight: 600;
  text-transform: uppercase;
  color: var(--color-text-muted);
}

.today-windows {
  display: flex;
  gap: var(--space-2);
  flex-wrap: wrap;
}

.today-window-tag {
  padding: var(--space-2) var(--space-3);
  background-color: var(--color-accent-primary);
  color: var(--color-accent-text);
  border-radius: var(--radius-md);
  font-size: var(--font-size-sm);
  font-weight: 600;
}

.no-availability-text {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
}

.weekly-overview {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.section-title {
  font-size: var(--font-size-lg);
  font-weight: 600;
  color: var(--color-text-primary);
}

.weekly-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(130px, 1fr));
  gap: var(--space-3);
}

.weekly-day-card {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
  background-color: var(--color-bg-surface);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-md);
  padding: var(--space-3);
}

.weekly-day-card--today {
  border-color: var(--color-accent-primary);
  background-color: #1e2e4a;
}

.weekly-day-name {
  font-size: var(--font-size-xs);
  font-weight: 600;
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
}

.weekly-day-empty {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
}
</style>
