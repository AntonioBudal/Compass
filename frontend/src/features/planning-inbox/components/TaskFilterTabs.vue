<script setup lang="ts">
import type { TaskStatus } from '@/entities/task/types'

export type FilterValue = TaskStatus | 'ALL'

interface TabOption {
  value: FilterValue
  label: string
}

defineProps<{
  modelValue: FilterValue
  counts?: Record<FilterValue, number>
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: FilterValue): void
}>()

const tabs: TabOption[] = [
  { value: 'ALL', label: 'Todas' },
  { value: 'Draft', label: 'Draft' },
  { value: 'Ready', label: 'Ready' },
  { value: 'InProgress', label: 'Em Andamento' },
  { value: 'Done', label: 'Concluídas' }
]

function selectTab(value: FilterValue) {
  emit('update:modelValue', value)
}
</script>

<template>
  <nav class="task-tabs-nav" role="tablist" aria-label="Filtro de Tarefas por Status">
    <button
      v-for="tab in tabs"
      :key="tab.value"
      type="button"
      role="tab"
      :aria-selected="modelValue === tab.value"
      :class="['tab-button', { active: modelValue === tab.value }]"
      @click="selectTab(tab.value)"
    >
      <span class="tab-label">{{ tab.label }}</span>
      <span v-if="counts && counts[tab.value] !== undefined" class="tab-count">
        {{ counts[tab.value] }}
      </span>
    </button>
  </nav>
</template>

<style scoped>
.task-tabs-nav {
  display: flex;
  gap: var(--space-1);
  overflow-x: auto;
  padding-bottom: 0;
  border-bottom: 1px solid var(--color-border);
}

.tab-button {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  padding: var(--space-2) var(--space-3);
  background: transparent;
  border: none;
  border-bottom: 2px solid transparent;
  color: var(--color-text-secondary);
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-medium);
  cursor: pointer;
  border-radius: var(--radius-sm) var(--radius-sm) 0 0;
  transition: color var(--transition-fast), border-color var(--transition-fast), background-color var(--transition-fast);
  white-space: nowrap;
  margin-bottom: -1px;
}

.tab-button:hover {
  color: var(--color-text-primary);
  background-color: var(--color-surface-hover);
}

.tab-button.active {
  color: var(--color-text-primary);
  border-bottom-color: var(--color-accent);
  font-weight: var(--font-weight-semibold);
}

.tab-count {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: var(--font-size-xs);
  padding: 1px var(--space-2);
  border-radius: var(--radius-full);
  background-color: var(--color-surface-subtle);
  border: 1px solid var(--color-border-subtle);
  color: var(--color-text-secondary);
  font-weight: var(--font-weight-medium);
  min-width: 18px;
}

.tab-button.active .tab-count {
  background-color: var(--color-surface-hover);
  border-color: var(--color-border);
  color: var(--color-text-primary);
}
</style>
