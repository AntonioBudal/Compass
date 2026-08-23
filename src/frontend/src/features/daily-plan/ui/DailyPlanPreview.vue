<script setup lang="ts">
import { toRefs, ref, computed } from 'vue'
import { isAxiosError } from 'axios'
import { useDailyPlanPreview } from '../api/useDailyPlanPreview'
import { useAcceptDailyPlan } from '../api/useAcceptDailyPlan'
import { formatTimeWithTimezone } from '@/shared/lib/dateFormatter'


interface Props {
  profileId: string
  date: string 
  timeZone: string
}

const props = defineProps<Props>()
const { profileId, date } = toRefs(props)

const { data: plan, isPending, isError } = useDailyPlanPreview(profileId, date)
const { mutate: acceptPlan, isPending: isAccepting } = useAcceptDailyPlan()

const planAcceptedLocally = ref(false)
const conflictError = ref(false)
const mutationError = ref(false)

const isPlanAccepted = computed(() => planAcceptedLocally.value || conflictError.value)

function formatTime(utcString: string) {
  return formatTimeWithTimezone(utcString, props.timeZone)
}

function handleAcceptPlan() {
  if (isAccepting.value || isPlanAccepted.value) return
  mutationError.value = false

  acceptPlan(
    { profileId: profileId.value, date: date.value },
    {
      onSuccess: () => {
        planAcceptedLocally.value = true
        conflictError.value = false
      },
      onError: (error: unknown) => {
        if (isAxiosError(error) && error.response?.status === 409) {
          conflictError.value = true
        } else {
          mutationError.value = true
        }
      }
    }
  )
}
</script>

<template>
  <section class="daily-plan-preview" aria-labelledby="preview-title">
    <header class="preview-header">
      <div class="header-titles">
        <h2 id="preview-title">Plano do dia</h2>
        <span v-if="isPlanAccepted" class="label-accepted">Plano aceito</span>
      </div>
    </header>

    <div v-if="isPending" class="state-container" aria-live="polite">
      <p>Preparando o plano do dia…</p>
    </div>

    <div v-else-if="isError" class="state-container error" role="alert">
      <p class="error-title">Não foi possível gerar um plano.</p>
      <p class="error-desc">Verifique a disponibilidade de horário ou conexão.</p>
    </div>

    <div v-else-if="plan?.suggestions.length === 0" class="state-container">
      <p>Nenhuma tarefa alocada para hoje.</p>
    </div>

    <div v-else-if="plan" class="plan-content">
      <ul class="task-list">
        <li v-for="suggestion in plan.suggestions" :key="suggestion.referenceId" class="task-item">
          <div class="task-time">
            <time :datetime="suggestion.start">{{ formatTime(suggestion.start) }}</time>
            <time :datetime="suggestion.end" class="time-end">{{ formatTime(suggestion.end) }}</time>
          </div>
          <div class="task-details">
            <span class="task-title">{{ suggestion.title }}</span>
            <span class="task-type">{{ suggestion.type }}</span>
          </div>
        </li>
      </ul>

      <footer class="plan-actions" v-if="!isPlanAccepted">
        <p v-if="mutationError" class="inline-error" role="alert">
          Falha ao aceitar o plano. Tente novamente.
        </p>
        <button 
          class="btn-primary" 
          @click="handleAcceptPlan" 
          :disabled="isAccepting"
          :aria-busy="isAccepting"
        >
          {{ isAccepting ? 'Aceitando plano…' : 'Aceitar plano' }}
        </button>
      </footer>
      <footer class="plan-actions-readonly" v-else>
        <p class="readonly-text">Plano aceito. As sugestões deste dia foram preservadas.</p>
      </footer>
    </div>
  </section>
</template>

<style scoped>
.daily-plan-preview {
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  height: 100%;
}

.preview-header {
  border-bottom: 1px solid var(--color-border-subtle);
  padding-bottom: 1rem;
  margin-bottom: 1.5rem;
}

.header-titles {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.header-titles h2 {
  margin: 0;
  font-size: 1rem;
  font-weight: 600;
  color: var(--color-text-primary);
}

.label-accepted {
  font-size: 0.75rem;
  font-weight: 500;
  color: var(--color-text-secondary);
  border: 1px solid var(--color-border);
  padding: 0.15rem 0.5rem;
  border-radius: var(--radius-small);
}

/* Estados Vazio/Erro/Loading */
.state-container {
  padding: 2rem 0;
  color: var(--color-text-secondary);
  font-size: 0.875rem;
}

.state-container.error {
  color: var(--color-text-primary);
}

.error-title {
  margin: 0 0 0.25rem 0;
  font-weight: 500;
}

.error-desc {
  margin: 0;
  color: var(--color-text-muted);
}

/* Lista (Agenda) */
.plan-content {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.task-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
}

.task-item {
  display: flex;
  gap: 1.5rem;
  padding: 0.75rem 0.5rem;
  border-bottom: 1px solid var(--color-border-subtle);
  transition: background-color var(--transition-fast);
}

.task-item:last-child {
  border-bottom: none;
}

.task-item:hover {
  background-color: var(--color-surface-hover);
}

.task-time {
  display: flex;
  flex-direction: column;
  min-width: 3.5rem;
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
  font-size: 0.8rem;
  color: var(--color-text-secondary);
  line-height: 1.4;
}

.time-end {
  color: var(--color-text-muted);
}

.task-details {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
}

.task-title {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--color-text-primary);
  line-height: 1.3;
}

.task-type {
  font-size: 0.7rem;
  color: var(--color-text-muted);
}

/* Ações */
.plan-actions {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.inline-error {
  margin: 0;
  font-size: 0.875rem;
  color: var(--color-text-primary);
  border-left: 2px solid var(--color-border-strong);
  padding-left: 0.5rem;
}

.btn-primary {
  background-color: var(--color-action);
  color: var(--color-on-action);
  border: 1px solid transparent;
  border-radius: var(--radius-small);
  padding: 0.5rem 1rem;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: background-color var(--transition-fast), opacity var(--transition-fast);
  align-self: flex-start;
}

.btn-primary:hover:not(:disabled) {
  background-color: var(--color-action-hover);
}

.btn-primary:disabled {
  background-color: var(--color-action-disabled);
  color: var(--color-text-disabled);
  cursor: not-allowed;
}

.plan-actions-readonly {
  padding-top: 1rem;
  border-top: 1px solid var(--color-border-subtle);
}

.readonly-text {
  margin: 0;
  font-size: 0.875rem;
  color: var(--color-text-muted);
}

/* Responsividade Mobile */
@media (max-width: 600px) {
  .task-item {
    flex-direction: column;
    gap: 0.25rem;
  }
  
  .task-time {
    flex-direction: row;
    gap: 0.5rem;
  }
  
  .time-end::before {
    content: "—";
    margin-right: 0.5rem;
    color: var(--color-text-disabled);
  }
}
</style>