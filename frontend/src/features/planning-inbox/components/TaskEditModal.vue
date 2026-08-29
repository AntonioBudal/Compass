<script setup lang="ts">
import { ref, watch } from 'vue'
import type { TaskDto, UpdateTaskPayload } from '@/entities/task/types'
import AppButton from '@/shared/ui/AppButton.vue'
import AppInput from '@/shared/ui/AppInput.vue'
import AppModal from '@/shared/ui/AppModal.vue'

const props = defineProps<{
  task: TaskDto | null
  isOpen: boolean
  isSaving?: boolean
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'save', taskId: string, payload: UpdateTaskPayload): void
}>()

const title = ref('')
const description = ref('')
const durationMinutes = ref<number | ''>('')
const deadline = ref('')
const titleError = ref('')
const durationError = ref('')

watch(
  () => props.task,
  (t) => {
    if (t) {
      title.value = t.title
      description.value = t.description || ''
      durationMinutes.value = t.durationMinutes ?? ''
      deadline.value = t.deadline ? formatDateTimeLocal(t.deadline) : ''
      titleError.value = ''
      durationError.value = ''
    }
  },
  { immediate: true }
)

function formatDateTimeLocal(isoString: string): string {
  const d = new Date(isoString)
  const pad = (n: number) => n.toString().padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`
}

function handleSave() {
  const trimmedTitle = title.value.trim()
  if (!trimmedTitle) {
    titleError.value = 'O título é obrigatório.'
    return
  }

  let finalDuration: number | null = null
  if (typeof durationMinutes.value === 'number') {
    if (durationMinutes.value <= 0) {
      durationError.value = 'A estimativa deve ser maior que 0 minutos.'
      return
    }
    finalDuration = durationMinutes.value
  }

  titleError.value = ''
  durationError.value = ''

  let finalDeadline: string | null = null
  if (deadline.value) {
    finalDeadline = new Date(deadline.value).toISOString()
  }

  if (props.task) {
    emit('save', props.task.id, {
      title: trimmedTitle,
      description: description.value.trim() || null,
      durationMinutes: finalDuration,
      deadline: finalDeadline
    })
  }
}
</script>

<template>
  <AppModal
    :model-value="isOpen && !!task"
    title="Editar Tarefa"
    @update:model-value="emit('close')"
    @close="emit('close')"
  >
    <form id="task-edit-form" class="modal-form" @submit.prevent="handleSave">
      <div class="form-group">
        <AppInput
          v-model="title"
          label="Título da Tarefa"
          placeholder="Ex.: Criar apresentação"
          :error="titleError"
          required
          aria-label="Título"
        />
      </div>

      <div class="form-group">
        <label for="task-desc" class="field-label">Descrição / Notas</label>
        <textarea
          id="task-desc"
          v-model="description"
          rows="3"
          class="textarea-control"
          placeholder="Detalhes adicionais sobre a tarefa..."
        />
      </div>

      <div class="form-row">
        <div class="form-group flex-1">
          <AppInput
            v-model="durationMinutes"
            type="number"
            label="Estimativa (minutos)"
            placeholder="Ex.: 45"
            :error="durationError"
          />
        </div>

        <div class="form-group flex-1">
          <label for="task-deadline" class="field-label">Data Limite (Deadline)</label>
          <input
            id="task-deadline"
            v-model="deadline"
            type="datetime-local"
            class="datetime-control"
          />
        </div>
      </div>
    </form>

    <template #footer>
      <AppButton
        type="button"
        variant="secondary"
        :disabled="isSaving"
        @click="emit('close')"
      >
        Cancelar
      </AppButton>
      <AppButton
        type="submit"
        form="task-edit-form"
        variant="primary"
        :loading="isSaving"
      >
        Salvar Alterações
      </AppButton>
    </template>
  </AppModal>
</template>

<style scoped>
.modal-form {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: var(--space-1);
}

.field-label {
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-medium);
  color: var(--color-text-secondary);
  line-height: 1.4;
}

.textarea-control,
.datetime-control {
  width: 100%;
  padding: var(--space-2) var(--space-3);
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  color: var(--color-text-primary);
  font-family: inherit;
  font-size: var(--font-size-sm);
  line-height: 1.4;
  transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
}

.textarea-control:focus,
.datetime-control:focus {
  outline: none;
  border-color: var(--color-accent);
  box-shadow: var(--focus-ring);
}

.form-row {
  display: flex;
  gap: var(--space-3);
}

.flex-1 {
  flex: 1;
}

@media (max-width: 640px) {
  .form-row {
    flex-direction: column;
  }
}
</style>
