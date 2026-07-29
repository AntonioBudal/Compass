<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import Sidebar from '@/components/layout/Sidebar.vue';
import HeaderContext from '@/components/layout/HeaderContext.vue';
import StatusBar from '@/components/layout/StatusBar.vue';
import ToastContainer from '@/components/core/ToastContainer.vue';
import SandboxTopBanner from '@/components/core/SandboxTopBanner.vue';
import CommandBarModal from '@/components/modals/CommandBarModal.vue';
import QuickCaptureModal from '@/components/modals/QuickCaptureModal.vue';
import PilotChecklistWidget from '@/components/onboarding/PilotChecklistWidget.vue';
import { isCommandBarOpen, isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';

// Flag de Desbloqueio Progressivo (Boot Sequence)
const isAppReady = ref(false);

const handleBootSequence = () => {
  isAppReady.value = true;
};

onMounted(() => {
  // 1. Checa se já fez onboarding no passado
  isAppReady.value = localStorage.getItem('compass_onboarded') === 'true';
  
  // 2. Escuta o evento exato do momento em que o Laboratório termina
  window.addEventListener('compass:boot-sequence', handleBootSequence);
});

onUnmounted(() => {
  window.removeEventListener('compass:boot-sequence', handleBootSequence);
});
</script>

<template>
  <div class="h-screen w-screen flex flex-col bg-app text-content overflow-hidden select-none font-sans">
    
    <SandboxTopBanner />

    <div class="flex-1 flex min-h-0 relative">
      
      <!-- BOOT SEQUENCE DA SIDEBAR -->
      <transition name="boot-sidebar">
        <Sidebar v-if="isAppReady" />
      </transition>

      <div class="flex-1 flex flex-col min-w-0 overflow-hidden relative">
        
        <!-- BOOT SEQUENCE DO HEADER -->
        <transition name="boot-header">
          <HeaderContext v-if="isAppReady" />
        </transition>

        <main class="flex-1 overflow-y-auto" :class="isAppReady ? 'p-6 md:p-8' : 'p-0'">
          <router-view v-slot="{ Component }">
            <transition name="page-fade" mode="out-in">
              <component :is="Component" />
            </transition>
          </router-view>
        </main>

        <!-- BOOT SEQUENCE DA BARRA DE STATUS -->
        <transition name="boot-status">
          <StatusBar v-if="isAppReady" />
        </transition>
      </div>
    </div>

    <!-- WIDGET DO PILOTO (Só aparece se o app estiver bootado) -->
    <PilotChecklistWidget v-if="isAppReady" />

    <!-- MODAIS E TOASTS -->
    <ToastContainer />
    <CommandBarModal :is-open="isCommandBarOpen" @close="isCommandBarOpen = false" />
    <QuickCaptureModal :is-open="isQuickCaptureOpen" @close="isQuickCaptureOpen = false" />
  </div>
</template>

<style scoped>
.page-fade-enter-active,
.page-fade-leave-active { transition: opacity 120ms ease; }
.page-fade-enter-from,
.page-fade-leave-to { opacity: 0; }

/* Animações Cinematográficas de Boot */
.boot-sidebar-enter-active { transition: all 600ms cubic-bezier(0.16, 1, 0.3, 1); }
.boot-sidebar-enter-from { opacity: 0; transform: translateX(-100%); }
.boot-sidebar-leave-active { transition: all 300ms ease; }
.boot-sidebar-leave-to { opacity: 0; transform: translateX(-100%); }

.boot-header-enter-active { transition: all 600ms cubic-bezier(0.16, 1, 0.3, 1) 200ms; }
.boot-header-enter-from { opacity: 0; transform: translateY(-100%); }

.boot-status-enter-active { transition: all 600ms cubic-bezier(0.16, 1, 0.3, 1) 300ms; }
.boot-status-enter-from { opacity: 0; transform: translateY(100%); }
</style>