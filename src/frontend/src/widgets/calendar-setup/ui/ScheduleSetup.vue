<script setup lang="ts">
import { ref } from 'vue'
import { useScheduleProfileQuery } from '@/entities/calendar-profile/api/useScheduleProfileQuery'
import type { ScheduleWindowDto } from '@/entities/calendar-profile/api/useScheduleProfileQuery'
import { useAddScheduleWindow } from '@/features/calendar-setup/api/useAddScheduleWindow'
import { useRemoveScheduleWindow } from '@/features/calendar-setup/api/useRemoveScheduleWindow'

const props = defineProps<{
  profileId: string
}>()

const { data: profile, isPending, isError } = useScheduleProfileQuery(props.profileId)
const { mutate: addWindow, isPending: isAdding } = useAddScheduleWindow()
const { mutate: removeWindow, isPending: isRemoving } = useRemoveScheduleWindow()

// Mapeamento de exibição. Os 'values' devem bater exatamente com o enum DayOfWeek do C#
const daysOfWeek = [
  { value: 'Monday', label: 'Segunda-feira' },
  { value: 'Tuesday', label: 'Terça-feira' },
  { value: 'Wednesday', label: 'Quarta-feira' },
  { value: 'Thursday', label: 'Quinta-feira' },
  { value: 'Friday', label: 'Sexta-feira' },
  { value: 'Saturday', label: 'Sábado' },
  { value: 'Sunday', label: 'Domingo' }
]

// Estado local para abrigar os rascunhos de horário para cada dia ("HH:mm")
const newWindowDrafts = ref<Record<string, { start: string, end: string }>>({
  Monday: { start: '', end: '' },
  Tuesday: { start: '', end: '' },
  Wednesday: { start: '', end: '' },
  Thursday: { start: '', end: '' },
  Friday: { start: '', end: '' },
  Saturday: { start: '', end: '' },
  Sunday: { start: '', end: '' }
})

// Filtra e ordena as janelas do dia
function getWindowsForDay(day: string): ScheduleWindowDto[] {
  if (!profile.value) return []
  return profile.value.windows
    .filter(w => w.dayOfWeek === day)
    .sort((a, b) => a.startTime.localeCompare(b.startTime))
}

// O backend retorna TimeSpan stringificado: "08:00:00". Removemos os segundos para a UI.
function formatTime(timeSpanStr: string) {
  return timeSpanStr.slice(0, 5)
}

function handleAddWindow(day: string) {
  const draft = newWindowDrafts.value[day]
  if (!draft.start || !draft.end || isAdding.value) return

  addWindow(
    {
      profileId: props.profileId,
      request: {
        dayOfWeek: day,
        startTime: draft.start,
        endTime: draft.end
      }
    },
    {
      onSuccess: () => {
        // Limpa os campos após inserção bem sucedida
        draft.start = ''
        draft.end = ''
      }
    }
  )
}

function handleRemoveWindow(windowId: string) {
  if (isRemoving.value) return
  removeWindow({ profileId: props.profileId, windowId })
}
</script>

<template>
  <section class="schedule-setup" aria-labelledby="setup-title">
    <header class="setup-header">
      <h2 id="setup-title">Sua Semana Base</h2>
      <div class="setup-meta" v-if="profile">
        <span class="meta-label">Timezone:</span>
        <code class="meta-value">{{ profile.timezone }}</code>
      </div>
    </header>

    <div v-if="isPending" class="state-container" aria-live="polite">
      <p>Carregando perfil de agenda...</p>
    </div>

    <div v-else-if="isError || !profile" class="state-container error-state" role="alert">
      <p class="error-title">Perfil não encontrado.</p>
      <p class="error-desc">Não foi possível carregar a configuração da sua semana base.</p>
    </div>

    <!-- Grid Semanal -->
    <div v-else class="week-grid">
      <div v-for="day in daysOfWeek" :key="day.value" class="day-row">
        
        <!-- Coluna: Nome do Dia -->
        <div class="day-label">
          <span>{{ day.label }}</span>
        </div>

        <!-- Coluna: Janelas de Tempo -->
        <div class="day-content">
          
          <ul v-if="getWindowsForDay(day.value).length > 0" class="window-list">
            <li v-for="window in getWindowsForDay(day.value)" :key="window.id" class="window-item">
              <span class="window-time">
                {{ formatTime(window.startTime) }} — {{ formatTime(window.endTime) }}
              </span>
              <button 
                class="action-text danger" 
                @click="handleRemoveWindow(window.id)"
                :disabled="isRemoving"
                title="Remover janela"
              >
                Remover
              </button>
            </li>
          </ul>

          <div v-else class="empty-day">
            <span>Sem janelas de disponibilidade</span>
          </div>

          <!-- Formulário Inline: Adicionar Janela -->
          <div class="add-window-form">
            <input 
              v-model="newWindowDrafts[day.value].start"
              type="time"
              class="time-input"
              aria-label="Hora de início"
              :disabled="isAdding"
            />
            <span class="time-separator">-</span>
            <input 
              v-model="newWindowDrafts[day.value].end"
              type="time"
              class="time-input"
              aria-label="Hora de término"
              :disabled="isAdding"
            />
            <button 
              class="action-text" 
              @click="handleAddWindow(day.value)"
              :disabled="!newWindowDrafts[day.value].start || !newWindowDrafts[day.value].end || isAdding"
            >
              Adicionar
            </button>
          </div>

        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.schedule-setup {
  display: flex;
  flex-direction: column;
  background-color: var(--color-surface-1);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-medium);
  width: 100%;
}

.setup-header {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  padding: 1rem 1.5rem;
  border-bottom: 1px solid var(--color-border);
}

.setup-header h2 {
  margin: 0;
  font-size: 1rem;
  font-weight: 600;
  color: var(--color-text-primary);
}

.setup-meta {
  display: flex;
  gap: 0.5rem;
  align-items: baseline;
}

.meta-label {
  font-size: 0.75rem;
  color: var(--color-text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.meta-value {
  font-size: 0.75rem;
  color: var(--color-text-secondary);
  background-color: var(--color-surface-2);
  padding: 0.15rem 0.4rem;
  border-radius: var(--radius-small);
  border: 1px solid var(--color-border-subtle);
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
}

/* Estados */
.state-container {
  padding: 3rem 1.5rem;
  color: var(--color-text-secondary);
  font-size: 0.875rem;
  text-align: center;
}

.error-title {
  margin: 0 0 0.25rem 0;
  font-weight: 500;
  color: var(--color-text-primary);
}

.error-desc {
  margin: 0;
  color: var(--color-text-muted);
}

/* Grid da Semana */
.week-grid {
  display: flex;
  flex-direction: column;
}

.day-row {
  display: flex;
  padding: 1.25rem 1.5rem;
  border-bottom: 1px solid var(--color-border-subtle);
  gap: 2rem;
}

.day-row:last-child {
  border-bottom: none;
}

/* Coluna 1: Label do dia */
.day-label {
  flex: 0 0 120px;
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--color-text-primary);
  padding-top: 0.25rem;
}

/* Coluna 2: Janelas e Form */
.day-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.empty-day {
  font-size: 0.875rem;
  color: var(--color-text-muted);
  font-style: italic;
}

/* Lista de Janelas Existentes */
.window-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.window-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  background-color: var(--color-surface-2);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-small);
  padding: 0.4rem 0.75rem;
  width: fit-content;
}

.window-time {
  font-size: 0.875rem;
  color: var(--color-text-primary);
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
}

/* Formulário de Adicionar */
.add-window-form {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.time-input {
  background-color: var(--color-surface-1);
  color: var(--color-text-primary);
  border: 1px dashed var(--color-border);
  border-radius: var(--radius-small);
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
  font-family: ui-monospace, SFMono-Regular, Consolas, monospace;
  width: auto;
  transition: border-color var(--transition-fast);
}

.time-input:focus {
  outline: none;
  border-style: solid;
  border-color: var(--color-text-secondary);
}

.time-input:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.time-separator {
  color: var(--color-text-muted);
}

/* Botões Textuais Sutis */
.action-text {
  background: none;
  border: none;
  font-size: 0.75rem;
  font-weight: 600;
  cursor: pointer;
  padding: 0 0.5rem;
  transition: opacity var(--transition-fast), color var(--transition-fast);
  color: var(--color-text-secondary);
}

.action-text:hover:not(:disabled) {
  color: var(--color-text-primary);
}

.action-text.danger:hover:not(:disabled) {
  color: #d73a49; /* Única exceção de cor para exclusão (Red GitHub-like) */
}

.action-text:disabled {
  color: var(--color-text-disabled);
  cursor: not-allowed;
}

/* Responsividade */
@media (max-width: 600px) {
  .day-row {
    flex-direction: column;
    gap: 0.75rem;
    padding: 1rem;
  }
  
  .day-label {
    flex: none;
    padding-top: 0;
  }
}
</style>