<script setup lang="ts">
import Sidebar from '@/components/layout/Sidebar.vue';
import HeaderContext from '@/components/layout/HeaderContext.vue';
import StatusBar from '@/components/layout/StatusBar.vue';
import ToastContainer from '@/components/core/ToastContainer.vue';
import SandboxTopBanner from '@/components/core/SandboxTopBanner.vue';
import CommandBarModal from '@/components/modals/CommandBarModal.vue';
import QuickCaptureModal from '@/components/modals/QuickCaptureModal.vue';
import { isCommandBarOpen, isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';
</script>

<template>
  <div class="h-screen w-screen flex flex-col bg-app text-content overflow-hidden select-none font-sans">
    <!-- 1. BLINDAGEM DO SANDBOX (Faixa Fixa no Topo) -->
    <SandboxTopBanner />

    <!-- 2. LAYOUT PRINCIPAL (Sidebar + Área de Trabalho) -->
    <div class="flex-1 flex min-h-0 relative">
      <Sidebar />

      <div class="flex-1 flex flex-col min-w-0 overflow-hidden">
        <HeaderContext />

        <main class="flex-1 overflow-y-auto p-6 md:p-8">
          <router-view v-slot="{ Component }">
            <transition name="page-fade" mode="out-in">
              <component :is="Component" />
            </transition>
          </router-view>
        </main>

        <StatusBar />
      </div>
    </div>

    <!-- 3. GESTÃO GLOBAL DE MODAIS E NOTIFICAÇÕES -->
    <ToastContainer />
    <CommandBarModal :is-open="isCommandBarOpen" @close="isCommandBarOpen = false" />
    <QuickCaptureModal :is-open="isQuickCaptureOpen" @close="isQuickCaptureOpen = false" />
  </div>
</template>

<style scoped>
.page-fade-enter-active,
.page-fade-leave-active {
  transition: opacity 120ms ease;
}
.page-fade-enter-from,
.page-fade-leave-to {
  opacity: 0;
}
</style>