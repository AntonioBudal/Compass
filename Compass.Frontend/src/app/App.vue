<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import AppLayout from '@/components/layout/AppLayout.vue';
import { useKeyboardShortcuts } from '@/shared/composables/useKeyboardShortcuts';

// Stores Core
import { useProjectsStore } from '@/modules/strategy/stores/projectsStore';
import { useGoalsStore } from '@/modules/strategy/stores/goalsStore';
import { useCommitmentsStore } from '@/modules/tactical/stores/commitmentsStore';

const projectsStore = useProjectsStore();
const goalsStore = useGoalsStore();
const commitmentsStore = useCommitmentsStore();
const router = useRouter();

// ARQ: Barreira de Estado Global
const isAppReady = ref(false);

useKeyboardShortcuts(); // Mantém o cérebro dos atalhos ativo

onMounted(async () => {
  try {
    const isOnboarded = localStorage.getItem('compass_onboarded');
    if (!isOnboarded) {
      localStorage.setItem('compass_onboarded', 'true');
      router.push('/sandbox');
    }

    //  ARQ BOOTSTRAPPER: Single Source of Initialization
    // Hidrata todas as coleções core ANTES da interface ser liberada para uso.
    // Isso mata os bugs de "F5", flashes de tela vazia e chamadas duplicadas.
    await Promise.all([
      goalsStore.fetchGoals(),
      projectsStore.fetchCatalog(),
      commitmentsStore.fetchAllActive()
    ]);

  } catch (e) {
    console.error('[Bootstrap] Falha na hidratação inicial do ecossistema:', e);
  } finally {
    // Libera a renderização das Views
    isAppReady.value = true;
  }
});
</script>

<template>
  <!-- Loading Global Elegante (Protege o F5) -->
  <div v-if="!isAppReady" class="w-screen h-screen flex flex-col items-center justify-center bg-app text-content-muted">
     <div class="animate-spin w-8 h-8 border-4 border-borderfocus border-t-transparent rounded-full mb-4"></div>
     <span class="font-mono text-xs tracking-widest uppercase">Sincronizando...</span>
  </div>
  
  <!-- Aplicação Real -->
  <AppLayout v-else />
</template>

<style>
/* Estilos globais mantidos */
</style>