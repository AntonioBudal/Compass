<template>
  <div class="onboarding-layout">
    <header class="onboarding-header">
      <div class="header-container">
        <div class="header-brand">
          <span class="brand-title">Compass</span>
        </div>
        <div v-if="state.currentStep > 1" class="step-progress" aria-label="Progresso do Onboarding">
          <span class="step-indicator">Passo {{ state.currentStep - 1 }} de 3</span>
        </div>
      </div>
    </header>

    <main class="onboarding-main">
      <div class="onboarding-content">
        <StepPresentation
          v-if="state.currentStep === 1"
          @next="state.currentStep = 2"
        />

        <StepTimeZone
          v-else-if="state.currentStep === 2"
          v-model="state.timeZoneId"
          @back="state.currentStep = 1"
          @next="state.currentStep = 3"
        />

        <StepAvailability
          v-else-if="state.currentStep === 3"
          :days="state.days"
          @back="state.currentStep = 2"
          @next="state.currentStep = 4"
        />

        <StepConfirmation
          v-else-if="state.currentStep === 4"
          :time-zone-id="state.timeZoneId"
          :days="state.days"
          :loading="createProfileMutation.isPending.value"
          :error="errorMessage"
          @back="state.currentStep = 3"
          @confirm="handleConfirm"
        />
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import {
  createInitialOnboardingState,
  toWeeklyAvailability
} from '@/features/onboarding/model/onboardingState'
import StepPresentation from '@/features/onboarding/components/StepPresentation.vue'
import StepTimeZone from '@/features/onboarding/components/StepTimeZone.vue'
import StepAvailability from '@/features/onboarding/components/StepAvailability.vue'
import StepConfirmation from '@/features/onboarding/components/StepConfirmation.vue'
import { useCreateScheduleProfileMutation } from '@/entities/schedule-profile/model/useCreateScheduleProfileMutation'

const router = useRouter()
const state = createInitialOnboardingState()
const createProfileMutation = useCreateScheduleProfileMutation()
const customError = ref('')

const errorMessage = computed(() => {
  if (customError.value) return customError.value
  if (createProfileMutation.error.value) {
    return createProfileMutation.error.value.message
  }
  return ''
})

async function handleConfirm() {
  customError.value = ''
  try {
    const weeklyAvailability = toWeeklyAvailability(state.days)
    await createProfileMutation.mutateAsync({
      timeZoneId: state.timeZoneId,
      weeklyAvailability
    })
    // Step 5: Redirecionamento para a tela Hoje
    await router.push('/today')
  } catch (err: any) {
    customError.value = err?.message || 'Ocorreu um erro ao criar o perfil.'
  }
}
</script>

<style scoped>
.onboarding-layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background-color: var(--color-bg-app);
}

.onboarding-header {
  height: 56px;
  background-color: var(--color-surface);
  border-bottom: 1px solid var(--color-border);
  display: flex;
  align-items: center;
}

.header-container {
  width: 100%;
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 var(--space-4);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header-brand {
  display: flex;
  align-items: center;
}

.brand-title {
  font-size: var(--font-size-base);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-primary);
  letter-spacing: -0.01em;
}

.step-indicator {
  font-size: var(--font-size-xs);
  color: var(--color-text-secondary);
  font-weight: var(--font-weight-medium);
  background-color: var(--color-surface-subtle);
  border: 1px solid var(--color-border-subtle);
  padding: 2px var(--space-2);
  border-radius: var(--radius-sm);
}

.onboarding-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
  padding: var(--space-8) var(--space-4);
}

.onboarding-content {
  width: 100%;
  max-width: 640px;
  margin: 0 auto;
}

@media (max-width: 640px) {
  .header-container {
    padding: 0 var(--space-3);
  }

  .onboarding-main {
    padding: var(--space-4) var(--space-3);
  }
}
</style>
