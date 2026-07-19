<script setup lang="ts">
import Sidebar from './Sidebar.vue';
import HeaderContext from './HeaderContext.vue';
import StatusBar from './StatusBar.vue';
import CommandBarModal from '@/components/modals/CommandBarModal.vue';
import QuickCaptureModal from '@/components/modals/QuickCaptureModal.vue';
</script>

<template>
  <div class="h-screen w-screen flex overflow-hidden bg-compass-bg text-compass-text font-sans antialiased">
    <!-- Painel Esquerdo: Sidebar de Navegação -->
    <Sidebar />

    <!-- Painel Central: Viewport de Trabalho -->
    <div class="flex-1 flex flex-col min-w-0 h-full overflow-hidden">
      <HeaderContext />

      <!-- Área Exclusiva de Rolagem Vertical (Cap. 4.1) -->
      <main class="flex-1 overflow-y-auto min-h-0 relative">
        <div class="max-w-7xl mx-auto p-4 md:p-6 lg:p-8">
          <slot />
        </div>
      </main>

      <StatusBar />
    </div>

    <!-- Modais Globais Flutuantes (Acionados por Teclado ou Botões) -->
    <CommandBarModal />
    <QuickCaptureModal />
  </div>
</template>