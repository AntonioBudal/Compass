<script setup lang="ts">
import { toRefs } from 'vue'
import { useDailyAdherenceQuery } from '@/entities/daily-adherence/api/useAdherenceQuery'

interface Props {
  profileId: string
  date: string
}

const props = defineProps<Props>()
const { profileId, date } = toRefs(props)

// O hook exposto pela Entity busca passivamente os dados calculados do backend
const { data: report, isPending, isError } = useDailyAdherenceQuery(profileId, date)
</script>

<template>
  <section class="adherence-dashboard" aria-labelledby="adherence-title">
    <header class="dashboard-header">
      <h2 id="adherence-title">Aderência do Plano Diário</h2>
      <div v-if="report" class="conformity-badge">
        {{ report.globalConformityPercentage.toFixed(1) }}% Conformidade Global
      </div>
    </header>

    <!-- Estados -->
    <div v-if="isPending" class="state-container" aria-live="polite">
      <p>Analisando aderência…</p>
    </div>

    <div v-else-if="isError" class="state-container empty-state" role="alert">
      <p class="intro-text">Nenhum plano diário encontrado para esta data.</p>
      <p class="sub-text">O relatório de aderência exige que um plano tenha sido aceito previamente.</p>
    </div>

    <!-- Relatório Carregado -->
    <div v-else-if="report" class="dashboard-content">
      <div class="totals-section">
        <div class="total-row">
          <span class="total-label">Tempo Planejado:</span>
          <span class="total-value">{{ report.totalPlannedMinutes }} min</span>
        </div>
        <div class="total-row">
          <span class="total-label">Tempo Executado:</span>
          <span class="total-value">{{ report.totalExecutedMinutes }} min</span>
        </div>
        <p class="total-note">(Nota: Tempo executado reflete apenas tarefas que existiam no plano)</p>
      </div>

      <div class="tasks-section" v-if="report.tasks.length > 0">
        <div 
          v-for="task in report.tasks" 
          :key="task.referenceId"
          class="task-card"
        >
          <h3 class="task-title">{{ task.title }}</h3>
          <div class="task-metrics">
            <div class="metric">
              <span class="metric-label">Planejado</span>
              <span class="metric-value">{{ task.plannedMinutes }} min</span>
            </div>
            <div class="metric highlight">
              <span class="metric-label">Executado</span>
              <span class="metric-value">{{ task.executedMinutes }} min</span>
            </div>
            <div class="metric">
              <span class="metric-label">Adimplido</span>
              <span class="metric-value">{{ task.intersectedMinutes }} min</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.adherence-dashboard {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  padding: 1.5rem;
  background-color: var(--color-surface-1);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-medium);
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid var(--color-border-subtle);
  padding-bottom: 1rem;
}

.dashboard-header h2 {
  margin: 0;
  font-size: 1rem;
  font-weight: 600;
  color: var(--color-text-primary);
}

.conformity-badge {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--color-text-primary);
  background-color: var(--color-surface-3);
  padding: 0.25rem 0.5rem;
  border-radius: var(--radius-small);
  border: 1px solid var(--color-border-strong);
}

/* Estados */
.state-container {
  padding: 2rem 0;
  color: var(--color-text-secondary);
  font-size: 0.875rem;
}

.empty-state {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.intro-text {
  margin: 0;
  color: var(--color-text-primary);
  font-weight: 500;
}

.sub-text {
  margin: 0;
  color: var(--color-text-muted);
}

/* Totais */
.dashboard-content {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.totals-section {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.total-row {
  display: flex;
  justify-content: space-between;
  max-width: 20rem;
}

.total-label {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--color-text-secondary);
}

.total-value {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--color-text-primary);
}

.total-note {
  margin: 0.5rem 0 0 0;
  font-size: 0.75rem;
  color: var(--color-text-muted);
}

/* Tasks */
.tasks-section {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.task-card {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  padding: 1rem;
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-small);
  background-color: var(--color-surface-2);
}

.task-title {
  margin: 0;
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--color-text-primary);
}

.task-metrics {
  display: flex;
  gap: 2rem;
}

.metric {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.metric-label {
  font-size: 0.7rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-muted);
}

.metric-value {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--color-text-secondary);
}

/* Destaque visual branco/negrito pro que foi executado, conforme identidade */
.metric.highlight .metric-value {
  font-weight: 600;
  color: var(--color-text-primary);
}

@media (max-width: 480px) {
  .dashboard-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.75rem;
  }
  
  .task-metrics {
    flex-direction: column;
    gap: 0.5rem;
  }
}
</style>