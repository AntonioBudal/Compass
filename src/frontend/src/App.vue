<script setup lang="ts">
import { ref } from 'vue'
import DailyPlanPreview from './features/daily-plan/ui/DailyPlanPreview.vue'
import ExecutionPanel from './features/daily-cycles/ui/ExecutionPanel.vue'
import PlanningInbox from '@/widgets/planning-inbox/ui/PlanningInbox.vue'
import ScheduleSetup from '@/widgets/calendar-setup/ui/ScheduleSetup.vue'
import { getCurrentCivilDate } from '@/shared/lib/dateFormatter'

// O timezone fixo aqui atua como o fuso do perfil do usuário na simulação técnica
const timeZone = ref('America/Sao_Paulo') 
const profileId = ref('a1111111-1111-1111-1111-111111111111') 

// Obtém a data civil correta para hoje no fuso configurado
const date = ref(getCurrentCivilDate(timeZone.value)) 

const showPreview = ref(false)

function loadPreview() {
  if(profileId.value.length > 30) {
    showPreview.value = true
  }
}
</script>

<template>
  <main class="app-container">
    <header class="app-header">
      <div class="header-titles">
        <h1 class="logo">Compass</h1>
        <p class="subtitle">Planejamento e execução diária</p>
      </div>
      <div class="header-context">
        <span class="context-date" v-if="showPreview">{{ date }}</span>
      </div>
    </header>

    <div class="workspace">
      <!-- Configuração Técnica Recolhível -->
      <details class="technical-config" v-if="!showPreview">
        <summary>Configuração técnica (Temporário)</summary>
        <div class="config-content">
          <label class="control-field">
            <span class="control-label">Profile ID</span>
            <input v-model="profileId" type="text" placeholder="Guid do BD..." />
          </label>
          
          <label class="control-field">
            <span class="control-label">Data</span>
            <input v-model="date" type="date" />
          </label>
          
          <label class="control-field">
            <span class="control-label">Timezone</span>
            <input v-model="timeZone" type="text" />
          </label>
          
          <button class="btn-primary" @click="loadPreview">
            Carregar planejamento
          </button>
        </div>
      </details>

      <!-- NOVO: Configuração da Semana Base -->
      <ScheduleSetup 
        v-if="showPreview" 
        :profile-id="profileId"
        class="inbox-widget-container" 
      />

      <!-- Caixa de Entrada (Inbox) -->
      <PlanningInbox 
        v-if="showPreview" 
        class="inbox-widget-container" 
      />

      <!-- Painel Principal em Grid -->
      <div v-if="showPreview" class="dashboard-grid">
        <!-- Coluna Esquerda: O Plano -->
        <DailyPlanPreview 
          :profile-id="profileId" 
          :date="date" 
          :time-zone="timeZone" 
        />
        
        <!-- Coluna Direita: O Ciclo -->
        <ExecutionPanel 
          :profile-id="profileId" 
          :date="date" 
          :time-zone="timeZone" 
        />
      </div>
    </div>
  </main>
</template>

<style scoped>
.app-container {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

/* Header Minimalista (GitHub-like) */
.app-header {
  background-color: var(--color-surface-1);
  border-bottom: 1px solid var(--color-border);
  padding: 0.75rem 1.5rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.header-titles {
  display: flex;
  align-items: baseline;
  gap: 1rem;
}

.logo {
  font-size: 1rem;
  font-weight: 600;
  color: var(--color-text-primary);
  margin: 0;
}

.subtitle {
  font-size: 0.875rem;
  color: var(--color-text-muted);
  margin: 0;
  display: none; /* Esconde no mobile pequeno */
}

@media (min-width: 600px) {
  .subtitle {
    display: block;
  }
}

.context-date {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--color-text-secondary);
}

/* Área principal */
.workspace {
  padding: 1.5rem;
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1.5rem;
}

@media (max-width: 768px) {
  .workspace {
    padding: 1rem;
  }
}

/* Configurações Técnicas Recolhíveis */
.technical-config {
  background-color: var(--color-surface-1);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-medium);
  width: 100%;
  max-width: 1200px;
}

.technical-config summary {
  padding: 1rem;
  cursor: pointer;
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--color-text-secondary);
  user-select: none;
}

.technical-config summary:hover {
  color: var(--color-text-primary);
}

.config-content {
  padding: 1rem;
  border-top: 1px solid var(--color-border-subtle);
  display: flex;
  gap: 1.5rem;
  flex-wrap: wrap;
  align-items: flex-end;
}

/* Inputs de Configuração */
.control-field {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  flex: 1;
  min-width: 200px;
}

.control-label {
  font-size: 0.75rem;
  font-weight: 500;
  color: var(--color-text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

input {
  background-color: var(--color-surface-2);
  color: var(--color-text-primary);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-small);
  padding: 0.5rem 0.75rem;
  font-size: 0.875rem;
  font-family: inherit;
  transition: border-color var(--transition-fast), background-color var(--transition-fast);
}

input:hover {
  background-color: var(--color-surface-hover);
  border-color: var(--color-border);
}

input:focus {
  outline: none;
  border-color: var(--color-border-strong);
  background-color: var(--color-surface-3);
}

/* Botões */
.btn-primary {
  background-color: var(--color-action);
  color: var(--color-on-action);
  border: 1px solid transparent;
  border-radius: var(--radius-small);
  padding: 0.5rem 1rem;
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

/* Controle de Largura do Inbox/Setup */
.inbox-widget-container {
  width: 100%;
  max-width: 1200px;
}

/* Grid de Dashboard Unificado */
.dashboard-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(0, 1fr);
  width: 100%;
  max-width: 1200px;
  background: var(--color-surface-1);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-medium);
}

.dashboard-grid > * {
  min-width: 0;
}

/* Divisor Vertical do Desktop */
.dashboard-grid > * + * {
  border-left: 1px solid var(--color-border);
}

/* Empilhamento no Mobile */
@media (max-width: 860px) {
  .dashboard-grid {
    grid-template-columns: 1fr;
  }

  .dashboard-grid > * + * {
    border-left: 0;
    border-top: 1px solid var(--color-border);
  }
}
</style>