<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useOnboardingStore } from '@/stores/onboardingStore';
import { useToastStore } from '@/stores/toastStore';
// 1. Importe os novos componentes do Dia 26:
import SpotlightOverlay from '@/components/onboarding/SpotlightOverlay.vue';
import OnboardingSteps from '@/components/onboarding/OnboardingSteps.vue';
import { Terminal, CheckCircle2, Clock, ShieldAlert, ArrowRight, Play, XCircle, BookOpen } from 'lucide-vue-next';

const onboardingStore = useOnboardingStore();
const toastStore = useToastStore();

// Controle reativo do modal de treinamento
const isTrainingOpen = ref(false);

const unlockNavigation = () => {
  try {
    localStorage.setItem('compass_onboarded', 'true');
  } catch (e) {}
};

const handleTrainingComplete = () => {
  isTrainingOpen.value = false;
  unlockNavigation(); 
  toastStore.showToast('Treinamento concluído! O menu lateral está 100% liberado para navegação.', 'success');
};

const handleTrainingClose = () => {
  isTrainingOpen.value = false;
  unlockNavigation(); 
};

onMounted(() => {
  onboardingStore.seedSandboxData();
  // Abre o guia interativo automaticamente ao entrar no Sandbox
  setTimeout(() => {
    isTrainingOpen.value = true;
  }, 500);
});
</script>

<template>
  <div class="min-h-screen w-full bg-app text-content flex flex-col justify-between p-6 md:p-12 select-none relative">
    
    <!-- BANNER DE AVISO EPÊMERO -->
    <header class="max-w-4xl mx-auto w-full flex items-center justify-between pb-6 border-b border-borderbase">
      <div class="flex items-center gap-3">
        <div class="p-2 rounded bg-surface border border-borderfocus text-content-accent">
          <Terminal class="w-5 h-5 stroke-[2.5]" />
        </div>
        <div>
          <h1 class="text-sm font-bold tracking-wider uppercase">Compass RAM Sandbox</h1>
          <p class="text-[11px] text-content-muted font-mono">Ambiente isolado em memória (Zero-DB Impact)</p>
        </div>
      </div>

      <!-- Ações do Cabeçalho: Reabrir Guia ou Pular -->
      <div class="flex items-center gap-4">
        <button 
          @click="isTrainingOpen = true"
          class="text-xs px-3 py-1.5 rounded bg-surface border border-borderbase hover:border-borderfocus text-content font-mono transition-all flex items-center gap-1.5 cursor-pointer"
        >
          <BookOpen class="w-3.5 h-3.5 text-content-accent" />
          <span>Guia Interativo [T]</span>
        </button>

        <button 
          @click="onboardingStore.skipOnboarding()"
          class="text-xs text-content-muted hover:text-content transition-colors flex items-center gap-1.5 cursor-pointer font-mono"
        >
          <span>Pular introdução</span>
          <XCircle class="w-4 h-4" />
        </button>
      </div>
    </header>


    <!-- INJEÇÃO DO ESCUDO DE TREINAMENTO (DIA 26) -->
    <SpotlightOverlay :is-open="isTrainingOpen" @close="handleTrainingClose">
      <OnboardingSteps 
        @complete="handleTrainingComplete" 
        @skip="handleTrainingClose" 
      />
    </SpotlightOverlay>

  </div>
</template>