<script setup lang="ts">
import { useToastStore } from '@/stores/toastStore';
import { RotateCcw, X } from 'lucide-vue-next';

const toastStore = useToastStore();
</script>

<template>
  <div class="fixed bottom-12 right-6 z-50 flex flex-col gap-2 max-w-sm w-full pointer-events-none select-none">
    <transition-group name="toast-slide">
      <div
        v-for="toast in toastStore.toasts"
        :key="toast.id"
        class="pointer-events-auto flex items-center justify-between gap-3 p-3 rounded-tactic bg-surface border shadow-2xl text-xs font-mono transition-all duration-tactic"
        :class="[
          toast.type === 'error' ? 'border-status-danger-border text-status-danger-text' :
          toast.type === 'urgent' ? 'border-status-warning-border text-status-warning' :
          toast.type === 'success' ? 'border-status-success-border text-status-success-text' :
          'border-borderbase text-content'
        ]"
      >
        <span class="truncate flex-1">{{ toast.message }}</span>

        <div class="flex items-center gap-2 flex-shrink-0">
          <button
            v-if="toast.undoAction"
            @click="toastStore.executeUndo(toast.id)"
            class="inline-flex items-center gap-1.5 px-2 py-1 rounded bg-surface-active hover:bg-surface-hover text-content border border-borderfocus transition-colors cursor-pointer"
            title="Desfazer ação (Cmd+Z)"
          >
            <RotateCcw class="w-3 h-3 text-content-accent" />
            <span class="font-sans font-medium">Desfazer</span>
            <kbd class="text-[9px] bg-app px-1 rounded border border-borderbase text-content-muted">Cmd+Z</kbd>
          </button>

          <button
            @click="toastStore.dismissToast(toast.id)"
            class="p-1 rounded hover:bg-surface-hover text-content-muted hover:text-content transition-colors"
          >
            <X class="w-3.5 h-3.5" />
          </button>
        </div>
      </div>
    </transition-group>
  </div>
</template>

<style scoped>
.toast-slide-enter-active,
.toast-slide-leave-active {
  transition: opacity 150ms cubic-bezier(0.16, 1, 0.3, 1), transform 150ms cubic-bezier(0.16, 1, 0.3, 1);
}
.toast-slide-enter-from,
.toast-slide-leave-to {
  opacity: 0;
  transform: translateY(8px) scale(0.98);
}
</style>