<template>
  <AppShell>
    <div class="not-found-container">
      <div class="not-found-content">
        <span class="status-code">404</span>
        <h1 class="not-found-title">Página não encontrada</h1>
        <p class="not-found-description">
          O caminho solicitado não existe ou foi movido para outro endereço.
        </p>
        <div class="actions">
          <AppButton variant="primary" @click="goHome">
            Voltar para o Início
          </AppButton>
        </div>
      </div>
    </div>
  </AppShell>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import AppShell from '@/shared/ui/AppShell.vue'
import AppButton from '@/shared/ui/AppButton.vue'
import { profileStorage } from '@/entities/schedule-profile/model/profileStorage'

const router = useRouter()

function goHome() {
  if (profileStorage.hasActiveProfile()) {
    router.push('/today')
  } else {
    router.push('/onboarding')
  }
}
</script>

<style scoped>
.not-found-container {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 400px;
  text-align: center;
}

.not-found-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-3);
  max-width: 440px;
}

.status-code {
  font-size: var(--font-size-2xl);
  font-weight: var(--font-weight-bold);
  color: var(--color-text-muted);
  font-variant-numeric: tabular-nums;
}

.not-found-title {
  font-size: var(--font-size-xl);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-primary);
  letter-spacing: -0.01em;
}

.not-found-description {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  line-height: 1.5;
}

.actions {
  margin-top: var(--space-2);
}
</style>
