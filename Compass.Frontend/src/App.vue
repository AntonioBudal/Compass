<script setup lang="ts">
import { onMounted, defineAsyncComponent } from 'vue';
import { useRouter } from 'vue-router';
import AppLayout from '@/components/layout/AppLayout.vue';
import { useKeyboardShortcuts, isCommandBarOpen, isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';

// 2. MODAIS SOB DEMANDA (Zero impacto na carga inicial da página)
const AsyncCommandBarModal = defineAsyncComponent(() => 
  import('@/components/modals/CommandBarModal.vue')
);

const AsyncQuickCaptureModal = defineAsyncComponent(() => 
  import('@/components/modals/QuickCaptureModal.vue')
);

useKeyboardShortcuts();
const router = useRouter();

onMounted(() => {
  try {
    const isOnboarded = localStorage.getItem('compass_onboarded');
    if (!isOnboarded) {
      localStorage.setItem('compass_onboarded', 'true');
      router.push('/sandbox');
    }
  } catch (e) {}
});
</script>

<template>
  <AppLayout>
    <router-view v-slot="{ Component }">
      <transition name="fade-tactic" mode="out-in">
        <component :is="Component" />
      </transition>
    </router-view>

    <!-- Modais só são baixados e montados quando seus estados reativos viram true! -->
    <AsyncCommandBarModal :is-open="isCommandBarOpen" />
    <AsyncQuickCaptureModal v-if="isQuickCaptureOpen" :is-open="isQuickCaptureOpen" />
  </AppLayout>
</template>

<style>
.fade-tactic-enter-active,
.fade-tactic-leave-active {
  transition: opacity 100ms ease-out;
}
.fade-tactic-enter-from,
.fade-tactic-leave-to {
  opacity: 0;
}
</style>