<template>
  <div class="step-presentation">
    <div class="header-icon" aria-hidden="true">🧭</div>
    <h2 class="title">Bem-vindo ao Compass</h2>
    <p class="subtitle">
      Seu sistema modular de calendário, planejamento e execução de atividades.
    </p>
    <div class="features-list">
      <div class="feature-item">
        <span class="feature-icon" aria-hidden="true">🕒</span>
        <div>
          <strong>Fuso Horário Centralizado</strong>
          <p>Seus horários e disponibilidade sempre sincronizados e precisos.</p>
        </div>
      </div>
      <div class="feature-item">
        <span class="feature-icon" aria-hidden="true">📅</span>
        <div>
          <strong>Disponibilidade Semanal</strong>
          <p>Configure os dias e blocos de horário em que você realiza suas atividades.</p>
        </div>
      </div>
      <div class="feature-item">
        <span class="feature-icon" aria-hidden="true">⚡</span>
        <div>
          <strong>Planejamento Inteligente</strong>
          <p>Gere planos diários estruturados com total controle do seu tempo.</p>
        </div>
      </div>
    </div>
    <div class="actions">
      <AppButton variant="primary" @click="$emit('next')">
        Configurar meu Perfil
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
  gap: var(--space-6);
  max-width: 540px;
  margin: 0 auto;
}

.header-icon {
  font-size: 3rem;
  line-height: 1;
}

.title {
  font-size: var(--font-size-2xl);
  font-weight: 700;
  color: var(--color-text-primary);
}

.subtitle {
  font-size: var(--font-size-base);
  color: var(--color-text-secondary);
}

.features-list {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
  width: 100%;
  text-align: left;
  background-color: var(--color-bg-surface);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-4);
}

.feature-item {
  display: flex;
  gap: var(--space-3);
  align-items: flex-start;
}

.feature-icon {
  font-size: 1.25rem;
  flex-shrink: 0;
}

.feature-item strong {
  font-size: var(--font-size-sm);
  color: var(--color-text-primary);
}

.feature-item p {
  font-size: var(--font-size-xs);
  color: var(--color-text-secondary);
  margin-top: var(--space-1);
}

.actions {
  display: flex;
  justify-content: center;
  width: 100%;
  margin-top: var(--space-2);
}
</style>
