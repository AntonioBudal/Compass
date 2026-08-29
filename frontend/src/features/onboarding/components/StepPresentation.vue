<template>
  <div class="step-presentation">
    <div class="header-badge">
      <span class="badge-dot" aria-hidden="true" />
      <span>Primeiro Acesso</span>
    </div>
    <h2 class="title">Bem-vindo ao Compass</h2>
    <p class="subtitle">
      Sistema modular para gerenciamento de calendário, planejamento diário estruturado e execução.
    </p>

    <div class="features-list">
      <div class="feature-item">
        <div class="feature-indicator" aria-hidden="true">
          <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10" />
            <polyline points="12 6 12 12 16 14" />
          </svg>
        </div>
        <div>
          <strong>Fuso Horário Centralizado</strong>
          <p>Seus horários e disponibilidade sempre sincronizados com base no fuso IANA.</p>
        </div>
      </div>

      <div class="feature-item">
        <div class="feature-indicator" aria-hidden="true">
          <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
            <line x1="16" y1="2" x2="16" y2="6" />
            <line x1="8" y1="2" x2="8" y2="6" />
            <line x1="3" y1="10" x2="21" y2="10" />
          </svg>
        </div>
        <div>
          <strong>Disponibilidade Semanal</strong>
          <p>Defina suas janelas de tempo ativas para cada dia da semana.</p>
        </div>
      </div>

      <div class="feature-item">
        <div class="feature-indicator" aria-hidden="true">
          <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
            <polyline points="14 2 14 8 20 8" />
            <line x1="16" y1="13" x2="8" y2="13" />
            <line x1="16" y1="17" x2="8" y2="17" />
            <polyline points="10 9 9 9 8 9" />
          </svg>
        </div>
        <div>
          <strong>Planning Inbox</strong>
          <p>Capture ideias e tarefas como Drafts e refine estimativas para torná-las Ready.</p>
        </div>
      </div>
    </div>

    <div class="actions">
      <AppButton variant="primary" @click="$emit('next')">
        Configurar Perfil Inicial
      </AppButton>
    </div>
  </div>
</template>

<script setup lang="ts">
import AppButton from '@/shared/ui/AppButton.vue'

defineEmits<{
  (e: 'next'): void
}>()
</script>

<style scoped>
.step-presentation {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: var(--space-5);
  max-width: 520px;
  margin: 0 auto;
}

.header-badge {
  display: inline-flex;
  align-items: center;
  gap: var(--space-2);
  padding: var(--space-1) var(--space-3);
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-medium);
  color: var(--color-text-secondary);
  background-color: var(--color-surface-subtle);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-full);
}

.badge-dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background-color: var(--color-accent);
}

.title {
  font-size: var(--font-size-xl);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-primary);
  letter-spacing: -0.01em;
}

.subtitle {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  line-height: 1.5;
}

.features-list {
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
  width: 100%;
  text-align: left;
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: var(--space-4);
}

.feature-item {
  display: flex;
  gap: var(--space-3);
  align-items: flex-start;
  padding: var(--space-2) 0;
}

.feature-item:not(:last-child) {
  border-bottom: 1px solid var(--color-border-subtle);
}

.feature-indicator {
  width: 28px;
  height: 28px;
  border-radius: var(--radius-sm);
  background-color: var(--color-surface-subtle);
  border: 1px solid var(--color-border-subtle);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-text-secondary);
  flex-shrink: 0;
  margin-top: 2px;
}

.feature-item strong {
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-medium);
  color: var(--color-text-primary);
}

.feature-item p {
  font-size: var(--font-size-xs);
  color: var(--color-text-secondary);
  margin-top: 2px;
  line-height: 1.4;
}

.actions {
  display: flex;
  justify-content: center;
  width: 100%;
  margin-top: var(--space-2);
}
</style>
