<script setup lang="ts">
import { toRefs, computed, ref } from 'vue'
import { 
  useDailyCycle, 
  useStartCycle,
  useCloseCycle 
} from '../api/useDailyCycleActions'
import { useAcceptedDailyPlan } from '../api/useDailyPlanQuery'
import { formatTimeWithTimezone } from '@/shared/lib/dateFormatter'
import RecordExecutionForm from './RecordExecutionForm.vue'
import type { ExecutionReferenceOption } from './RecordExecutionForm.vue'

interface Props {
  profileId: string
  date: string 
  timeZone: string
}

const props = defineProps<Props>()
const { profileId, date } = toRefs(props)

// 1. Hook do Ciclo Ativo
const { data: cycle, isPending: isLoadingCycle, isError: isCycleError } = useDailyCycle(date)

// 2. Hook do Plano Aceito (Fonte de Referências Imutáveis)
const { data: acceptedPlan } = useAcceptedDailyPlan(profileId, date)

// Mutações de Estado do Ciclo
const { mutate: startCycle, isPending: isStarting } = useStartCycle(date)
const { mutate: closeCycle, isPending: isClosing } = useCloseCycle(date)

const cycleState = computed(() => {
  if (isLoadingCycle.value) return 'Loading'
  if (isCycleError.value) return 'Error'
  if (cycle.value === null) return 'Inexistent'
  if (cycle.value?.status === 'Active') return 'Active'
  if (cycle.value?.status === 'Closed') return 'Closed'
  return 'Unknown'
})

// Mapeamento do DTO para a Prop Burra do Formulário
const formReferences = computed<ExecutionReferenceOption[]>(() => {
  if (!acceptedPlan.value) return []
  
  return acceptedPlan.value.items.map(item => ({
    referenceId: item.referenceId,
    title: item.title,
    sourceType: item.type
  }))
})

const showCloseConfirm = ref(false)

function handleStartCycle() {
  if (isStarting.value) return
  startCycle({ date: date.value })
}

function handleCloseCycle() {
  if (isClosing.value || !cycle.value) return
  closeCycle(cycle.value.id, {
    onSuccess: () => {
      showCloseConfirm.value = false
    }
  })
}

function formatTime(utcString: string) {
  return formatTimeWithTimezone(utcString, props.timeZone)
}

function resolveReferenceTitle(refId: string | null) {
  if (!refId) return 'Pausa / Descanso' // Break
  
  const found = formReferences.value.find(r => r.referenceId === refId)
  if (found) return found.title
  
  // Fallback se o apontamento for antigo/órfão e o plano n tiver o id
  return `Ref: ${refId.split('-')[0]}` 
}
</script>

<template>
  <section class="execution-panel" aria-labelledby="execution-title">
    <header class="panel-header">
      <div class="header-titles">
        <h2 id="execution-title">Execução do dia</h2>
        <span v-if="cycleState === 'Closed'" class="label-state closed">Ciclo encerrado</span>
        <span v-else-if="cycleState === 'Active'" class="label-state active">Ciclo ativo</span>
      </div>
      <p class="panel-desc">Registre o que foi realmente realizado.</p>
    </header>

    <!-- Estados Base -->
    <div v-if="cycleState === 'Loading'" class="state-container" aria-live="polite">
      <p>Sincronizando estado do ciclo…</p>
    </div>

    <div v-else-if="cycleState === 'Error'" class="state-container error" role="alert">
      <p class="error-title">Falha ao acessar o ciclo diário.</p>
      <p class="error-desc">Verifique a conexão de rede ou tente novamente.</p>
    </div>

    <div v-else-if="cycleState === 'Inexistent'" class="state-container inexistent">
      <p class="intro-text">O ciclo diário ainda não foi iniciado.</p>
      <button 
        class="btn-primary" 
        @click="handleStartCycle"
        :disabled="isStarting"
        :aria-busy="isStarting"
      >
        {{ isStarting ? 'Iniciando…' : 'Iniciar ciclo' }}
      </button>
    </div>

    <!-- Área Ativa (Ciclo Iniciado) -->
    <div v-else-if="cycleState === 'Active' || cycleState === 'Closed'" class="active-workspace">
      
      <!-- O Formulário Injetado e Desacoplado -->
      <RecordExecutionForm 
        v-if="cycleState === 'Active' && cycle?.id"
        :cycle-id="cycle.id"
        :date="date"
        :time-zone="timeZone"
        :references="formReferences"
      />

      <div class="logs-container">
        <h3 class="logs-title">Registros</h3>
        <ul v-if="cycle?.logs.length" class="log-list">
          <li v-for="log in cycle.logs" :key="log.id" class="log-item">
            <div class="log-time">
              <time :datetime="log.start">{{ formatTime(log.start) }}</time>
              <time :datetime="log.end" class="time-end">{{ formatTime(log.end) }}</time>
            </div>
            <div class="log-details">
              <span class="log-type">{{ log.type }}</span>
              <span class="log-ref">{{ resolveReferenceTitle(log.referenceId) }}</span>
            </div>
          </li>
        </ul>
        <p v-else class="empty-logs">Nenhum apontamento registrado neste ciclo.</p>
      </div>

      <footer class="panel-actions" v-if="cycleState === 'Active'">
        <div v-if="showCloseConfirm" class="inline-confirm" role="alert">
          <p class="confirm-text">Após encerrar o ciclo, não será possível adicionar novos registros. Deseja continuar?</p>
          <div class="confirm-actions">
            <button class="btn-primary" @click="handleCloseCycle" :disabled="isClosing" :aria-busy="isClosing">
              {{ isClosing ? 'Encerrando…' : 'Confirmar encerramento' }}
            </button>
            <button class="btn-secondary" @click="showCloseConfirm = false" :disabled="isClosing">
              Cancelar
            </button>
          </div>
        </div>
        <button 
          v-else
          class="btn-secondary full-width" 
          @click="showCloseConfirm = true"
        >
          Encerrar ciclo
        </button>
      </footer>
    </div>
  </section>
</template>

<style scoped>
/* Os estilos continuam intocados e iguais ao que aprovamos no Redesign Monocromático! */
.execution-panel {
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  height: 100%;
}

.panel-header {
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

.label-state {
  font-size: 0.75rem;
  font-weight: 500;
  padding: 0.15rem 0.5rem;
  border-radius: var(--radius-small);
  border: 1px solid var(--color-border);
}

.label-state.active {
  color: var(--color-text-primary);
  border-color: var(--color-border-strong);
}

.label-state.closed {
  color: var(--color-text-secondary);
}

.panel-desc {
  margin: 0.25rem 0 0 0;
  font-size: 0.875rem;
  color: var(--color-text-muted);
}

/* Estados */
.state-container {
  padding: 2rem 0;
  color: var(--color-text-secondary);
  font-size: 0.875rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.state-container.error {
  color: var(--color-text-primary);
  gap: 0.25rem;
}

.error-title {
  margin: 0;
  font-weight: 500;
}

.error-desc {
  margin: 0;
  color: var(--color-text-muted);
}

.intro-text {
  margin: 0;
}

/* Área Ativa */
.active-workspace {
  display: flex;
  flex-direction: column;
  gap: 2rem;
  flex: 1;
}

.logs-container {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.logs-title {
  margin: 0;
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--color-text-primary);
}

.log-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
}

.log-item {
  display: flex;
  gap: 1.5rem;
  padding: 0.75rem 0.5rem;
  border-bottom: 1px solid var(--color-border-subtle);
  transition: background-color var(--transition-fast);
}

.log-item:last-child {
  border-bottom: none;
}

.log-item:hover {
  background-color: var(--color-surface-hover);
}

.log-time {
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

.log-details {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
}

.log-type {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--color-text-primary);
  line-height: 1.3;
}

.log-ref {
  font-size: 0.7rem;
  color: var(--color-text-muted);
}

.empty-logs {
  margin: 0;
  font-size: 0.875rem;
  color: var(--color-text-muted);
  padding: 1rem 0;
}

/* Ações e Confirmação Inline */
.panel-actions {
  margin-top: auto;
  border-top: 1px solid var(--color-border-subtle);
  padding-top: 1.5rem;
}

.inline-confirm {
  background-color: var(--color-surface-2);
  border-left: 2px solid var(--color-border-strong);
  padding: 1rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.confirm-text {
  margin: 0;
  font-size: 0.875rem;
  color: var(--color-text-primary);
}

.confirm-actions {
  display: flex;
  gap: 0.75rem;
}

/* Botões */
.btn-primary, .btn-secondary {
  border-radius: var(--radius-small);
  padding: 0.5rem 1rem;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.btn-primary {
  background-color: var(--color-action);
  color: var(--color-on-action);
  border: 1px solid transparent;
}

.btn-primary:hover:not(:disabled) {
  background-color: var(--color-action-hover);
}

.btn-primary:disabled {
  background-color: var(--color-action-disabled);
  color: var(--color-text-disabled);
  cursor: not-allowed;
}

.btn-secondary {
  background-color: transparent;
  color: var(--color-text-secondary);
  border: 1px solid var(--color-border);
}

.btn-secondary:hover:not(:disabled) {
  color: var(--color-text-primary);
  border-color: var(--color-border-strong);
}

.btn-secondary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.full-width {
  width: 100%;
}

@media (max-width: 600px) {
  .log-item {
    flex-direction: column;
    gap: 0.25rem;
  }
  
  .log-time {
    flex-direction: row;
    gap: 0.5rem;
  }
  
  .time-end::before {
    content: "—";
    margin-right: 0.5rem;
    color: var(--color-text-disabled);
  }

  .confirm-actions {
    flex-direction: column;
  }
}
</style>