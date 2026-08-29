<script setup lang="ts">
import { ref, computed } from 'vue'
import AppShell from '@/shared/ui/AppShell.vue'
import AppBadge from '@/shared/ui/AppBadge.vue'
import { useTasksQuery } from '@/entities/task/model/useTasksQuery'
import { useCreateTaskMutation } from '@/entities/task/model/useCreateTaskMutation'
import { useUpdateTaskMutation } from '@/entities/task/model/useUpdateTaskMutation'
import { useStartTaskMutation } from '@/entities/task/model/useStartTaskMutation'
import { useCompleteTaskMutation } from '@/entities/task/model/useCompleteTaskMutation'
import { useScheduleProfileQuery } from '@/entities/schedule-profile/model/useScheduleProfileQuery'
import type { TaskDto, UpdateTaskPayload } from '@/entities/task/types'
import type { FilterValue } from '@/features/planning-inbox/components/TaskFilterTabs.vue'
import QuickTaskCapture from '@/features/planning-inbox/components/QuickTaskCapture.vue'
import TaskFilterTabs from '@/features/planning-inbox/components/TaskFilterTabs.vue'
import TaskCard from '@/features/planning-inbox/components/TaskCard.vue'
import TaskEditModal from '@/features/planning-inbox/components/TaskEditModal.vue'
import EmptyState from '@/shared/ui/EmptyState.vue'

const { data: profile } = useScheduleProfileQuery()

const activeFilter = ref<FilterValue>('ALL')
const editingTask = ref<TaskDto | null>(null)
const isModalOpen = ref(false)
const globalError = ref<string | null>(null)

// Queries & Mutations
const { data: tasks, isLoading, error: queryError } = useTasksQuery()
const createTaskMutation = useCreateTaskMutation()
const updateTaskMutation = useUpdateTaskMutation()
const startTaskMutation = useStartTaskMutation()
const completeTaskMutation = useCompleteTaskMutation()

// Task Counts
const counts = computed(() => {
  const allTasks = tasks.value || []
  return {
    ALL: allTasks.length,
    Draft: allTasks.filter((t) => t.status === 'Draft').length,
    Ready: allTasks.filter((t) => t.status === 'Ready').length,
    InProgress: allTasks.filter((t) => t.status === 'InProgress').length,
    Done: allTasks.filter((t) => t.status === 'Done').length
  }
})

// Filtered tasks
const filteredTasks = computed(() => {
  const allTasks = tasks.value || []
  if (activeFilter.value === 'ALL') {
    return allTasks
  }
  return allTasks.filter((t) => t.status === activeFilter.value)
})

// Handlers
async function handleCapture(title: string) {
  globalError.value = null
  try {
    await createTaskMutation.mutateAsync({ title })
  } catch (err: any) {
    globalError.value = err.message || 'Erro ao capturar tarefa.'
  }
}

async function handleSetInlineEstimate(taskId: string, minutes: number) {
  globalError.value = null
  const currentTask = tasks.value?.find((t) => t.id === taskId)
  if (!currentTask) return

  try {
    await updateTaskMutation.mutateAsync({
      id: taskId,
      payload: {
        title: currentTask.title,
        description: currentTask.description,
        durationMinutes: minutes,
        deadline: currentTask.deadline
      }
    })
  } catch (err: any) {
    globalError.value = err.message || 'Erro ao atualizar estimativa.'
  }
}

async function handleStartTask(taskId: string) {
  globalError.value = null
  try {
    await startTaskMutation.mutateAsync(taskId)
  } catch (err: any) {
    globalError.value = err.message || 'Erro ao iniciar tarefa.'
  }
}

async function handleCompleteTask(taskId: string) {
  globalError.value = null
  try {
    await completeTaskMutation.mutateAsync(taskId)
  } catch (err: any) {
    globalError.value = err.message || 'Erro ao concluir tarefa.'
  }
}

function handleOpenEdit(task: TaskDto) {
  editingTask.value = task
  isModalOpen.value = true
}

function handleCloseEdit() {
  isModalOpen.value = false
  editingTask.value = null
}

async function handleSaveEdit(taskId: string, payload: UpdateTaskPayload) {
  globalError.value = null
  try {
    await updateTaskMutation.mutateAsync({ id: taskId, payload })
    handleCloseEdit()
  } catch (err: any) {
    globalError.value = err.message || 'Erro ao salvar alterações da tarefa.'
  }
}
</script>

<template>
  <AppShell>
    <template #header-actions>
      <div v-if="profile" class="header-tz-info">
        <AppBadge variant="default" size="sm">
          {{ profile.timeZoneId }}
        </AppBadge>
      </div>
    </template>

    <div class="planning-page">
      <div class="planning-container">
        <!-- Title & Subtitle -->
        <div class="page-title-row">
          <h1 class="page-heading">Planning Inbox</h1>
          <p class="page-subheading">
            Capture ideias rapidamente, defina estimativas e acompanhe suas tarefas.
          </p>
        </div>

        <!-- Error Alert Banner -->
        <div v-if="globalError || queryError" class="error-banner" role="alert">
          <div class="error-content">
            <svg class="error-icon" xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
              <circle cx="12" cy="12" r="10" />
              <line x1="12" y1="8" x2="12" y2="12" />
              <line x1="12" y1="16" x2="12.01" y2="16" />
            </svg>
            <span class="error-text">{{ globalError || (queryError as any)?.message }}</span>
          </div>
          <button
            type="button"
            class="btn-dismiss"
            aria-label="Dispensar aviso de erro"
            @click="globalError = null"
          >
            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <line x1="18" y1="6" x2="6" y2="18" />
              <line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
        </div>

        <!-- Quick Capture Form -->
        <section class="capture-section">
          <QuickTaskCapture
            :is-loading="createTaskMutation.isPending.value"
            @capture="handleCapture"
          />
        </section>

        <!-- Filter Tabs -->
        <section class="tabs-section">
          <TaskFilterTabs
            v-model="activeFilter"
            :counts="counts"
          />
        </section>

        <!-- Loading State -->
        <div v-if="isLoading" class="loading-state" role="status">
          <div class="spinner" aria-hidden="true" />
          <p>Carregando tarefas...</p>
        </div>

        <!-- Empty State -->
        <div v-else-if="filteredTasks.length === 0" class="empty-section">
          <EmptyState
            v-if="activeFilter === 'ALL'"
            title="Sua Inbox está vazia"
            description="Capture sua primeira tarefa digitando no campo acima e pressionando Enter."
          />
          <EmptyState
            v-else-if="activeFilter === 'Draft'"
            title="Nenhum rascunho"
            description="Todas as suas tarefas possuem estimativas definidas."
          />
          <EmptyState
            v-else-if="activeFilter === 'Ready'"
            title="Nenhuma tarefa pronta"
            description="Defina uma estimativa em minutos para que tarefas rascunho se tornem prontas."
          />
          <EmptyState
            v-else-if="activeFilter === 'InProgress'"
            title="Nenhuma tarefa em andamento"
            description="Inicie o trabalho em uma tarefa pronta para visualizá-la aqui."
          />
          <EmptyState
            v-else-if="activeFilter === 'Done'"
            title="Nenhuma tarefa concluída"
            description="Tarefas finalizadas aparecerão listadas nesta seção."
          />
        </div>

        <!-- Tasks Grid -->
        <div v-else class="tasks-grid">
          <TaskCard
            v-for="task in filteredTasks"
            :key="task.id"
            :task="task"
            :is-updating="
              updateTaskMutation.isPending.value ||
              startTaskMutation.isPending.value ||
              completeTaskMutation.isPending.value
            "
            @set-estimate="handleSetInlineEstimate"
            @start="handleStartTask"
            @complete="handleCompleteTask"
            @edit="handleOpenEdit"
          />
        </div>
      </div>
    </div>

    <!-- Task Edit Modal -->
    <TaskEditModal
      :task="editingTask"
      :is-open="isModalOpen"
      :is-saving="updateTaskMutation.isPending.value"
      @close="handleCloseEdit"
      @save="handleSaveEdit"
    />
  </AppShell>
</template>

<style scoped>
.planning-page {
  display: flex;
  justify-content: center;
  width: 100%;
}

.planning-container {
  width: 100%;
  max-width: 860px;
  display: flex;
  flex-direction: column;
  gap: var(--space-5);
}

.header-tz-info {
  display: flex;
  align-items: center;
}

.page-title-row {
  display: flex;
  flex-direction: column;
  gap: var(--space-1);
}

.page-heading {
  font-size: var(--font-size-2xl);
  font-weight: var(--font-weight-bold);
  color: var(--color-text-primary);
  letter-spacing: -0.02em;
  margin: 0;
}

.page-subheading {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  margin: 0;
}

.error-banner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background-color: var(--color-danger-subtle);
  border: 1px solid var(--color-danger);
  padding: var(--space-3) var(--space-4);
  border-radius: var(--radius-md);
  color: var(--color-danger-text);
  font-size: var(--font-size-sm);
}

.error-content {
  display: flex;
  align-items: center;
  gap: var(--space-2);
}

.error-icon {
  flex-shrink: 0;
}

.btn-dismiss {
  background: transparent;
  border: none;
  color: var(--color-danger-text);
  cursor: pointer;
  padding: 2px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: var(--radius-sm);
  transition: opacity var(--transition-fast);
}

.btn-dismiss:hover {
  opacity: 0.75;
}

.capture-section {
  background-color: var(--color-surface);
  padding: var(--space-4);
  border-radius: var(--radius-md);
  border: 1px solid var(--color-border);
}

.tabs-section {
  margin-top: var(--space-1);
}

.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: var(--space-8);
  gap: var(--space-3);
  color: var(--color-text-secondary);
}

.spinner {
  width: 28px;
  height: 28px;
  border: 2px solid var(--color-border);
  border-top-color: var(--color-accent);
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.empty-section {
  margin-top: var(--space-2);
}

.tasks-grid {
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
}
</style>
