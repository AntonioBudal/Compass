<script setup lang="ts">
import AppLayout from '@/components/layout/AppLayout.vue';
import { useKeyboardShortcuts } from '@/composables/useKeyboardShortcuts';

// Ativa a escuta de teclado global (Linear/Raycast style)
useKeyboardShortcuts();
</script>

<template>
  <AppLayout>
    <router-view v-slot="{ Component }">
      <transition name="fade-tactic" mode="out-in">
        <component :is="Component" />
      </transition>
    </router-view>
  </AppLayout>
</template>

<style>
/* Transição ultrarrápida de 100ms para mudança de tela (Cap. 4.5) */
.fade-tactic-enter-active,
.fade-tactic-leave-active {
  transition: opacity 100ms ease-out;
}

.fade-tactic-enter-from,
.fade-tactic-leave-to {
  opacity: 0;
}
</style>