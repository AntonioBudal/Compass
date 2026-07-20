<script setup lang="ts">
import Sidebar from './Sidebar.vue';
import HeaderContext from './HeaderContext.vue';
import StatusBar from './StatusBar.vue';
import CommandBarModal from '@/components/modals/CommandBarModal.vue';
import QuickCaptureModal from '@/components/modals/QuickCaptureModal.vue';
import EditCommitmentModal from '@/components/modals/EditCommitmentModal.vue';
import DailyShutdownModal from '@/components/modals/DailyShutdownModal.vue';
import ToastContainer from '@/components/core/ToastContainer.vue';
</script>

<template>
  <div class="h-screen w-screen flex overflow-hidden bg-compass-bg text-compass-text font-sans antialiased">
    <Sidebar />

    <div class="flex-1 flex flex-col min-w-0 h-full overflow-hidden">
      <HeaderContext />

      <main class="flex-1 overflow-y-auto min-h-0 relative">
        <div class="max-w-7xl mx-auto p-4 md:p-6 lg:p-8">
          <slot />
        </div>
      </main>

      <StatusBar />
    </div>

    <!-- Modais Flutuantes do Sistema -->
    <CommandBarModal />
    <QuickCaptureModal />
    <EditCommitmentModal />
    <DailyShutdownModal />

    <!-- Resiliência -->
    <ToastContainer />
  </div>
</template>