<script setup lang="ts">
import { ref, computed } from 'vue'
import { useTaskInboxQuery } from '@/entities/task-inbox/api/useTaskInboxQuery'
import { useCreateTask } from '@/features/planning-inbox/api/useCreateTask'
import { useEstimateTask } from '@/features/planning-inbox/api/useEstimateTask'

const { data: tasks, isPending: isQueryPending, isError: isQueryError } = useTaskInboxQuery()
const { mutate: createTask, isPending: isCreating } = useCreateTask()
const { mutate: estimateTask, isPending: isEstimating } = useEstimateTask()

// Controle de input para nova tarefa
const newTaskTitle = ref('')

// Captura no Enter
function handleQuickAdd() {
  const title = newTaskTitle.value.trim()
  if (!title || isCreating.value) return

  createTask(
    { title },
    {
      onSuccess: () => {
        newTaskTitle.value = ''
      }
    }
  )
}

// Controle local temporário das estimativas em digitação (referenciado pelo ID da task)
const estimatingMinutes = ref<Record<string, number | null>>({})

function handleEstimate(taskId: string) {
  const minutes = estimatingMinutes.value[taskId]
  if (!minutes || minutes <= 0 || isEstimating.value) return

  estimateTask(
    { id: taskId, request: { estimatedDurationMinutes: minutes } },
    {
      onSuccess: () => {
        // Limpa o estado local após sucesso
        estimatingMinutes.value[taskId] = null
      }
    }
  )
}

// Computeds para manter a visualização fluida e ordenada
const drafts = computed(() => tasks.value?.filter(t => t.status === 'Draft') || [])
const readys = computed(() => tasks.value?.filter(t => t.status === 'Ready') || [])
const hasItems = computed(() => drafts.value.length > 0 || readys.value.length > 0)
</script>

<template>
  <section class="inbox-widget" aria-labelledby="inbox-title">
    <header class="inbox-header">
      <h2 id="inbox-title">Caixa de Entrada</h2>
      <span class="inbox-count" v-if="tasks">{{ tasks.length }} pendências</span>
    </header>

    <!-- Quick Add Input -->
    <div class="quick-add-container">
      <input
        v-model="newTaskTitle"
        type="text"
        class="quick-add-input"
        placeholder="O que precisa ser feito? (Pressione Enter para salvar)"
        @keyup.enter="handleQuickAdd"
        :disabled="isCreating"
        :aria-busy="isCreating"
      />
    </div>

    <!-- Feedback States -->
    <div v-if="isQueryPending" class="state-container" aria-live="polite">
      <p>Sincronizando tarefas...</p>
    </div>

    <div v-else-if="isQueryError" class="state-container error-state" role="alert">
      <p>Não foi possível carregar a caixa de entrada.</p>
    </div>

    <div v-else-if="!hasItems" class="state-container empty-state">
      <p class="empty-title">Tudo limpo.</p>
      <p class="empty-desc">Nenhuma tarefa pendente de planejamento.</p>
    </div>

    <!-- Inbox Lists -->
    <div v-else class="inbox-content">
      
      <!-- Drafts (Aguardando estimativa) -->
      <div v-if="drafts.length > 0" class="list-section">
        <h3 class="section-title">Rascunhos (Sem estimativa)</h3>
        <ul class="task-list">
          <li v-for="task in drafts" :key="task.id" class="task-item draft-item">
            <span class="task-title">{{ task.title }}</span>
            
            <div class="estimate-controls">
              <input 
                v-model.number="estimatingMinutes[task.id]"
                type="number" 
                class="estimate-input" 
                placeholder="Minutos" 
                min="1"
                step="5"
                @keyup.enter="handleEstimate(task.id)"
              />
              <button 
                class="action-text" 
                @click="handleEstimate(task.id)"
                :disabled="!estimatingMinutes[task.id] || isEstimating"
              >
                Estimar
              </button>
            </div>
          </li>
        </ul>
      </div>

      <!-- Ready (Prontas para o plano) -->
      <div v-if="readys.length > 0" class="list-section">
        <h3 class="section-title">Prontas para execução</h3>
        <ul class="task-list">
          <li v-for="task in readys" :key="task.id" class="task-item ready-item">
            <div class="task-details">
              <span class="task-title">{{ task.title }}</span>
              <span class="task-meta">{{ task.estimatedDurationMinutes }} min</span>
            </div>
            <span class="status-label">Ready</span>
          </li>
        </ul>
      </div>

    </div>
  </section>
</template>

<style scoped>
.inbox-widget {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  padding: 1.5rem;
  background-color: var(--color-surface-1);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-medium);
  height: 100%;
}

.inbox-header {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  border-bottom: 1px solid var(--color-border-subtle);
  padding-bottom: 1rem;
}

.inbox-header h2 {
  margin: 0;
  font-size: 1rem;
  font-weight: 600;
  color: var(--color-text-primary);
}

.inbox-count {
  font-size: 0.75rem;
  color: var(--color-text-muted);
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
}

/* Captura rápida (Estilo Notion/Terminal) */
.quick-add-container {
  margin-bottom: 0.5rem;
}

.quick-add-input {
  width: 100%;
  background-color: transparent;
  color: var(--color-text-primary);
  border: none;
  border-bottom: 1px dashed var(--color-border);
  padding: 0.5rem 0;
  font-size: 0.875rem;
  font-family: inherit;
  transition: border-color var(--transition-fast);
}

.quick-add-input::placeholder {
  color: var(--color-text-muted);
}

.quick-add-input:focus {
  outline: none;
  border-bottom-color: var(--color-text-secondary);
}

.quick-add-input:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* Estados */
.state-container {
  padding: 2rem 0;
  color: var(--color-text-secondary);
  font-size: 0.875rem;
  text-align: center;
}

.empty-state .empty-title {
  margin: 0 0 0.25rem 0;
  font-weight: 500;
  color: var(--color-text-primary);
}

.empty-state .empty-desc {
  margin: 0;
  color: var(--color-text-muted);
}

/* Listagem Estrutural */
.inbox-content {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.list-section {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.section-title {
  margin: 0;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-muted);
}

.task-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

/* Linha da Task */
.task-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-small);
  background-color: var(--color-surface-2);
  gap: 1rem;
}

.task-title {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--color-text-primary);
  line-height: 1.4;
  word-break: break-word;
}

/* Draft específico (borda tracejada sutil) */
.draft-item {
  border-style: dashed;
}

/* Ready específico (sólido com meta-dados) */
.ready-item {
  background-color: var(--color-surface-1);
}

.task-details {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
}

.task-meta {
  font-size: 0.75rem;
  color: var(--color-text-muted);
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
}

.status-label {
  font-size: 0.7rem;
  font-weight: 600;
  text-transform: uppercase;
  color: var(--color-text-secondary);
  border: 1px solid var(--color-border-subtle);
  padding: 0.15rem 0.4rem;
  border-radius: var(--radius-small);
}

/* Controles de Estimativa (Text-based actions, sem botões grandes) */
.estimate-controls {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.estimate-input {
  width: 4.5rem;
  background-color: var(--color-surface-3);
  color: var(--color-text-primary);
  border: 1px solid transparent;
  border-radius: var(--radius-small);
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
}

.estimate-input:focus {
  outline: none;
  border-color: var(--color-border-strong);
}

.action-text {
  background: none;
  border: none;
  color: var(--color-text-primary);
  font-size: 0.75rem;
  font-weight: 600;
  cursor: pointer;
  padding: 0 0.25rem;
  transition: opacity var(--transition-fast);
}

.action-text:hover:not(:disabled) {
  opacity: 0.7;
}

.action-text:disabled {
  color: var(--color-text-disabled);
  cursor: not-allowed;
}

/* Oculta as setas de number input nos browsers para ficar mais limpo */
input[type=number]::-webkit-inner-spin-button, 
input[type=number]::-webkit-outer-spin-button { 
  -webkit-appearance: none; 
  margin: 0; 
}
input[type=number] {
  -moz-appearance: textfield;
}
</style>