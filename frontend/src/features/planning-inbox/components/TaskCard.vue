<script setup lang="ts">
import { ref, computed } from 'vue'
import type { TaskDto } from '@/entities/task/types'
import AppButton from '@/shared/ui/AppButton.vue'
import AppBadge from '@/shared/ui/AppBadge.vue'

const props = defineProps<{
  task: TaskDto
  isUpdating?: boolean
}>()

const emit = defineEmits<{
  (e: 'set-estimate', taskId: string, minutes: number): void
  (e: 'start', taskId: string): void
  (e: 'complete', taskId: string): void
  (e: 'edit', task: TaskDto): void
}>()

const isInlineEstimating = ref(false)
const inlineMinutes = ref<number | ''>('')
const inlineError = ref('')

const isOverdue = computed(() => {
  if (!props.task.deadline || props.task.status === 'Done') return false
  return new Date(props.task.deadline) < new Date()
})

const formattedDeadline = computed(() => {
  if (!props.task.deadline) return null
  const d = new Date(props.task.deadline)
  return d.toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit'
  })
})

const statusBadgeVariant = computed(() => {
  switch (props.task.status) {
    case 'Ready':
      return 'accent'
    case 'InProgress':
      return 'warning'
    case 'Done':
      return 'success'
    case 'Draft':
    default:
      return 'default'
  }
})

function handleSaveInlineEstimate() {
  if (typeof inlineMinutes.value !== 'number' || inlineMinutes.value <= 0) {
    inlineError.value = 'A estimativa deve ser um número positivo de minutos.'
    return
  }

  inlineError.value = ''
  emit('set-estimate', props.task.id, inlineMinutes.value)
  isInlineEstimating.value = false
  inlineMinutes.value = ''
}

function cancelInlineEstimate() {
  isInlineEstimating.value = false
  inlineMinutes.value = ''
  inlineError.value = ''
}
</script>

<template>
  <article
    class="task-card"
    :class="[`status-${task.status.toLowerCase()}`, { 'is-overdue': isOverdue }]"
    :aria-labelledby="`task-title-${task.id}`"
  >
    <div class="card-main">
      <div class="card-header">
        <AppBadge :variant="statusBadgeVariant" :class="`badge-${task.status.toLowerCase()}`">
          {{ task.status }}
        </AppBadge>

        <span v-if="task.durationMinutes" class="meta-tag duration-badge" :title="`Estimativa: ${task.durationMinutes} minutos`">
          <svg class="meta-icon" xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10" />
            <polyline points="12 6 12 12 16 14" />
          </svg>
          <span>{{ task.durationMinutes }}m</span>
        </span>
        <span v-else class="meta-tag meta-tag--missing duration-missing" title="Sem estimativa de duração">
          <span>Sem estimativa</span>
        </span>

        <span
          v-if="formattedDeadline"
          class="meta-tag deadline-badge"
          :class="{ 'meta-tag--overdue overdue': isOverdue }"
          :title="isOverdue ? 'Prazo expirado' : 'Prazo limite'"
        >
          <svg class="meta-icon" xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
            <line x1="16" y1="2" x2="16" y2="6" />
            <line x1="8" y1="2" x2="8" y2="6" />
            <line x1="3" y1="10" x2="21" y2="10" />
          </svg>
          <span>{{ formattedDeadline }}</span>
        </span>
      </div>

      <h3 :id="`task-title-${task.id}`" class="task-title">
        {{ task.title }}
      </h3>

      <p v-if="task.description" class="task-description">
        {{ task.description }}
      </p>

      <!-- Inline Estimate Form for Draft -->
      <div v-if="isInlineEstimating" class="inline-estimate-box">
        <label :for="`estimate-input-${task.id}`" class="estimate-label">
          Estimativa (minutos):
        </label>
        <div class="estimate-controls">
          <input
            :id="`estimate-input-${task.id}`"
            v-model.number="inlineMinutes"
            type="number"
            min="1"
            placeholder="Ex.: 30"
            class="estimate-input"
            @keyup.enter="handleSaveInlineEstimate"
            @keyup.esc="cancelInlineEstimate"
          />
          <AppButton
            type="button"
            variant="primary"
            size="sm"
            @click="handleSaveInlineEstimate"
          >
            Salvar
          </AppButton>
          <AppButton
            type="button"
            variant="secondary"
            size="sm"
            @click="cancelInlineEstimate"
          >
            Cancelar
          </AppButton>
        </div>
        <p v-if="inlineError" class="inline-error">{{ inlineError }}</p>
      </div>
    </div>

    <footer class="card-actions">
      <!-- Actions depending on status -->
      <button
        v-if="task.status === 'Draft' && !isInlineEstimating"
        type="button"
        class="action-btn btn-estimate"
        @click="isInlineEstimating = true"
      >
        Definir Estimativa
      </button>

      <button
        v-if="task.status === 'Ready'"
        type="button"
        class="action-btn action-btn--accent btn-start"
        :disabled="isUpdating"
        @click="emit('start', task.id)"
      >
        Iniciar
      </button>

      <button
        v-if="task.status === 'Ready' || task.status === 'InProgress'"
        type="button"
        class="action-btn action-btn--success btn-complete"
        :disabled="isUpdating"
        @click="emit('complete', task.id)"
      >
        Concluir
      </button>

      <button
        v-if="task.status !== 'Done'"
        type="button"
        class="action-btn btn-edit"
        :disabled="isUpdating"
        @click="emit('edit', task)"
      >
        Editar
      </button>
    </footer>
  </article>
</template>

<style scoped>
.task-card {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: var(--space-4);
  transition: border-color var(--transition-fast), background-color var(--transition-fast);
}

.task-card:hover {
  border-color: var(--color-text-secondary);
}

.task-card.status-done {
  background-color: var(--color-surface-subtle);
  border-color: var(--color-border-subtle);
}

.task-card.status-done .task-title {
  text-decoration: line-through;
  color: var(--color-text-muted);
}

.task-card.is-overdue {
  border-left: 3px solid var(--color-danger);
}

.card-header {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: var(--space-2);
  margin-bottom: var(--space-2);
}

.meta-tag {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: var(--font-size-xs);
  color: var(--color-text-secondary);
  background-color: var(--color-surface-subtle);
  border: 1px solid var(--color-border-subtle);
  padding: 2px var(--space-2);
  border-radius: var(--radius-sm);
  font-variant-numeric: tabular-nums;
}

.meta-tag--missing {
  color: var(--color-warning-text);
  background-color: var(--color-warning-subtle);
  border-color: var(--color-warning-subtle);
}

.meta-tag--overdue {
  color: var(--color-danger-text);
  background-color: var(--color-danger-subtle);
  border-color: var(--color-danger-subtle);
  font-weight: var(--font-weight-medium);
}

.meta-icon {
  flex-shrink: 0;
}

.task-title {
  font-size: var(--font-size-base);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-primary);
  margin: 0 0 var(--space-1) 0;
  line-height: 1.4;
}

.task-description {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  margin: 0 0 var(--space-3) 0;
  line-height: 1.4;
  white-space: pre-wrap;
}

.inline-estimate-box {
  margin-top: var(--space-2);
  padding: var(--space-3);
  background-color: var(--color-surface-subtle);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
}

.estimate-label {
  display: block;
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-medium);
  color: var(--color-text-secondary);
  margin-bottom: var(--space-1);
}

.estimate-controls {
  display: flex;
  gap: var(--space-2);
  align-items: center;
}

.estimate-input {
  width: 80px;
  padding: 4px var(--space-2);
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  color: var(--color-text-primary);
  font-size: var(--font-size-sm);
}

.estimate-input:focus {
  border-color: var(--color-accent);
  outline: none;
  box-shadow: var(--focus-ring);
}

.inline-error {
  font-size: var(--font-size-xs);
  color: var(--color-danger-text);
  margin-top: var(--space-1);
}

.card-actions {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-2);
  margin-top: var(--space-3);
  padding-top: var(--space-2);
  border-top: 1px solid var(--color-border-subtle);
}

.action-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background-color: var(--color-surface-subtle);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  color: var(--color-text-secondary);
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-medium);
  padding: 3px var(--space-2);
  cursor: pointer;
  transition: background-color var(--transition-fast), color var(--transition-fast), border-color var(--transition-fast);
}

.action-btn:hover:not(:disabled) {
  background-color: var(--color-surface-hover);
  color: var(--color-text-primary);
  border-color: var(--color-text-secondary);
}

.action-btn--accent:hover:not(:disabled) {
  background-color: var(--color-accent-subtle);
  color: var(--color-accent);
  border-color: var(--color-accent);
}

.action-btn--success:hover:not(:disabled) {
  background-color: var(--color-success-subtle);
  color: var(--color-success-text);
  border-color: var(--color-success);
}
</style>
