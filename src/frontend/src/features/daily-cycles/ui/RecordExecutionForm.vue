<script setup lang="ts">
import { ref, toRefs, computed } from 'vue'
import { isAxiosError } from 'axios'
import { useRecordExecution } from '../api/useDailyCycleActions'
import { buildAbsoluteTime } from '@/shared/lib/dateFormatter'

export interface ExecutionReferenceOption {
  referenceId: string
  title: string
  sourceType: string
}

interface Props {
  cycleId: string
  date: string
  timeZone: string
  references: readonly ExecutionReferenceOption[]
}

const props = defineProps<Props>()
const { cycleId, date, timeZone, references } = toRefs(props)

// O hook envia Date para invalidação correta da query de cache
const { mutate: recordExecution, isPending } = useRecordExecution(date)

// Estado local do form
const selectedType = ref('DeepWork')
const selectedReferenceId = ref('')
const startTime = ref('')
const endTime = ref('')

// Controle de sucesso/erros
const isSuccess = ref(false)
const errorMessage = ref('')
const validationError = ref('')

const isBreak = computed(() => selectedType.value === 'Break')

// Reseta o id se o tipo mudar para Break (limpeza de estado)
function handleTypeChange() {
  if (isBreak.value) {
    selectedReferenceId.value = ''
  }
}

function resetForm() {
  selectedReferenceId.value = ''
  startTime.value = ''
  endTime.value = ''
}

function handleSubmit() {
  validationError.value = ''
  errorMessage.value = ''
  isSuccess.value = false

  // 1. Validação de Frontend
  if (!isBreak.value && !selectedReferenceId.value) {
    validationError.value = 'Selecione uma tarefa ou hábito.'
    return
  }
  if (!startTime.value || !endTime.value) {
    validationError.value = 'Preencha o horário de início e fim.'
    return
  }
  if (startTime.value >= endTime.value) {
    validationError.value = 'O fim deve ser posterior ao início.'
    return
  }

  // 2. Conversão Temporal Segura
  const startAbs = buildAbsoluteTime(date.value, startTime.value, timeZone.value)
  const endAbs = buildAbsoluteTime(date.value, endTime.value, timeZone.value)

  // 3. Montagem do Request
  const payload = {
    referenceId: isBreak.value ? null : selectedReferenceId.value,
    type: selectedType.value,
    start: startAbs,
    end: endAbs
  }

  // 4. Disparo
  recordExecution(
    { cycleId: cycleId.value, req: payload },
    {
      onSuccess: () => {
        isSuccess.value = true
        resetForm()
        
        // Limpa a notificação de sucesso após 3 segundos
        setTimeout(() => {
          isSuccess.value = false
        }, 3000)
      },
      onError: (error: unknown) => {
        if (isAxiosError(error)) {
          errorMessage.value = error.response?.data?.error || 'Erro ao registrar execução.'
        } else {
          errorMessage.value = 'Ocorreu um erro inesperado.'
        }
      }
    }
  )
}
</script>

<template>
  <form 
    class="record-form" 
    @submit.prevent="handleSubmit" 
    aria-labelledby="record-title"
  >
    <header class="form-header">
      <h3 id="record-title">Novo apontamento</h3>
    </header>

    <div class="form-grid">
      <!-- 1. Tipo -->
      <label class="field-container">
        <span class="field-label">Tipo</span>
        <select 
          v-model="selectedType" 
          @change="handleTypeChange"
          :disabled="isPending"
        >
          <option value="DeepWork">Deep Work</option>
          <option value="Routine">Rotina</option>
          <option value="Break">Pausa / Descanso</option>
        </select>
      </label>

      <!-- 2. Referência (Escondido em caso de Break) -->
      <label class="field-container" v-if="!isBreak">
        <span class="field-label">Referência</span>
        <select 
          v-model="selectedReferenceId" 
          :disabled="isPending"
        >
          <option value="" disabled>Selecione o alvo…</option>
          <option 
            v-for="opt in references" 
            :key="opt.referenceId" 
            :value="opt.referenceId"
          >
            {{ opt.title }} ({{ opt.sourceType }})
          </option>
        </select>
      </label>

      <!-- 3. Horários -->
      <div class="time-group">
        <label class="field-container">
          <span class="field-label">Início</span>
          <input 
            type="time" 
            v-model="startTime" 
            :disabled="isPending"
          />
        </label>
        <span class="time-separator">—</span>
        <label class="field-container">
          <span class="field-label">Fim</span>
          <input 
            type="time" 
            v-model="endTime" 
            :disabled="isPending"
          />
        </label>
      </div>
    </div>

    <!-- Feedback e Ações -->
    <footer class="form-actions">
      <div class="feedback-area" aria-live="polite">
        <p v-if="validationError" class="inline-msg error" role="alert">{{ validationError }}</p>
        <p v-if="errorMessage" class="inline-msg error" role="alert">{{ errorMessage }}</p>
        <p v-if="isSuccess" class="inline-msg success">Registro inserido com sucesso!</p>
      </div>

      <button 
        type="submit" 
        class="btn-primary" 
        :disabled="isPending"
        :aria-busy="isPending"
      >
        {{ isPending ? 'Registrando…' : 'Registrar' }}
      </button>
    </footer>
  </form>
</template>

<style scoped>
.record-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  padding-bottom: 1.5rem;
  border-bottom: 1px solid var(--color-border-subtle);
  margin-bottom: 1.5rem;
}

.form-header h3 {
  margin: 0;
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--color-text-primary);
}

.form-grid {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.field-container {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  flex: 1;
}

.field-label {
  font-size: 0.75rem;
  font-weight: 500;
  color: var(--color-text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

select, input[type="time"] {
  background-color: var(--color-surface-2);
  color: var(--color-text-primary);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-small);
  padding: 0.5rem 0.75rem;
  font-size: 0.875rem;
  font-family: inherit;
  transition: border-color var(--transition-fast), background-color var(--transition-fast);
  width: 100%;
}

select:hover:not(:disabled), input[type="time"]:hover:not(:disabled) {
  background-color: var(--color-surface-hover);
  border-color: var(--color-border);
}

select:focus, input[type="time"]:focus {
  outline: none;
  border-color: var(--color-border-strong);
  background-color: var(--color-surface-3);
}

select:disabled, input:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Campos de Tempo lado a lado */
.time-group {
  display: flex;
  align-items: flex-end;
  gap: 0.5rem;
}

.time-separator {
  padding-bottom: 0.5rem;
  color: var(--color-text-disabled);
  font-weight: 500;
}

/* Área inferior */
.form-actions {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
}

.feedback-area {
  flex: 1;
}

.inline-msg {
  margin: 0;
  font-size: 0.8rem;
  font-weight: 500;
  padding: 0.35rem 0.5rem;
  border-radius: var(--radius-small);
}

.inline-msg.error {
  color: var(--color-text-primary);
  border-left: 2px solid var(--color-border-strong);
  background-color: var(--color-surface-2);
}

.inline-msg.success {
  color: var(--color-text-secondary);
  border-left: 2px solid var(--color-border-subtle);
}

.btn-primary {
  background-color: var(--color-action);
  color: var(--color-on-action);
  border: 1px solid transparent;
  border-radius: var(--radius-small);
  padding: 0.5rem 1.25rem;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  transition: background-color var(--transition-fast), opacity var(--transition-fast);
  white-space: nowrap;
}

.btn-primary:hover:not(:disabled) {
  background-color: var(--color-action-hover);
}

.btn-primary:disabled {
  background-color: var(--color-action-disabled);
  color: var(--color-text-disabled);
  cursor: not-allowed;
}

/* Mobile */
@media (max-width: 480px) {
  .time-group {
    flex-direction: column;
    align-items: stretch;
  }
  
  .time-separator {
    display: none;
  }

  .form-actions {
    flex-direction: column;
    align-items: stretch;
  }
}
</style>