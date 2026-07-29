<script setup lang="ts">
import { useToastStore } from '@/stores/toastStore';
import { RotateCcw, X, ShieldAlert, AlertTriangle, Info, ArrowRight } from 'lucide-vue-next';

const toastStore = useToastStore();
</script>

<template>
  <div class="fixed bottom-10 right-6 z-50 flex flex-col gap-3 max-w-md w-full pointer-events-none select-none">
    <transition-group name="toast-slide">
      <div
        v-for="toast in toastStore.toasts"
        :key="toast.id"
        class="pointer-events-auto w-full rounded-xl bg-surface border shadow-2xl transition-all duration-tactic overflow-hidden"
        :class="[
          toast.intervention?.severity === 'blocking' || toast.type === 'error' ? 'border-status-danger-border' :
          toast.intervention?.severity === 'warning' || toast.type === 'urgent' ? 'border-status-warning-border' :
          'border-borderfocus'
        ]"
      >
        <!-- LAYOUT 1: INTERVENÇÃO DEFENSIVA ("EXPLICAR & RESOLVER") -->
        <div v-if="toast.intervention" class="p-4 flex flex-col gap-3 font-mono">
          <!-- Cabeçalho do Diagnóstico -->
          <div class="flex items-start justify-between gap-3 pb-2.5 border-b border-borderbase/60">
            <div class="flex items-center gap-2">
              <ShieldAlert v-if="toast.intervention.severity === 'blocking'" class="w-4 h-4 text-status-danger-text flex-shrink-0" />
              <AlertTriangle v-else-if="toast.intervention.severity === 'warning'" class="w-4 h-4 text-status-warning flex-shrink-0" />
              <Info v-else class="w-4 h-4 text-content-accent flex-shrink-0" />
              <span class="text-xs font-bold font-sans text-content leading-tight">
                {{ toast.intervention.title }}
              </span>
            </div>
            <button
              @click="toastStore.dismissToast(toast.id)"
              class="text-content-muted hover:text-content p-0.5 rounded transition-colors"
              title="Fechar aviso"
            >
              <X class="w-4 h-4" />
            </button>
          </div>

          <!-- Corpo: Explicação do Motivo (Por que aconteceu?) -->
          <p class="text-xs text-content-muted font-sans leading-relaxed">
            {{ toast.intervention.explanation }}
          </p>

          <!-- Rodapé: Botões de Solução Acionável -->
          <div class="pt-1 flex flex-wrap items-center justify-end gap-2">
            <button
              v-for="(action, idx) in toast.intervention.actions"
              :key="idx"
              @click="toastStore.executeInterventionAction(toast.id, action)"
              class="px-3 py-1.5 rounded text-xs font-semibold transition-all flex items-center gap-1.5 cursor-pointer"
              :class="action.isPrimary 
                ? 'bg-content text-content-invert hover:opacity-90 shadow-sm font-bold' 
                : 'bg-surface-hover hover:bg-surface-active text-content border border-borderbase'"
            >
              <span>{{ action.label }}</span>
              <ArrowRight v-if="action.isPrimary" class="w-3 h-3 stroke-[2.5]" />
            </button>
          </div>
        </div>

        <!-- LAYOUT 2: TOAST SIMPLES TRADICIONAL -->
        <div v-else class="flex items-center justify-between gap-3 p-3 text-xs font-mono">
          <span class="truncate flex-1 font-sans text-content">{{ toast.message }}</span>

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
      </div>
    </transition-group>
  </div>
</template>

<style scoped>
.toast-slide-enter-active,
.toast-slide-leave-active {
  transition: opacity 200ms cubic-bezier(0.16, 1, 0.3, 1), transform 200ms cubic-bezier(0.16, 1, 0.3, 1);
}
.toast-slide-enter-from,
.toast-slide-leave-to {
  opacity: 0;
  transform: translateY(12px) scale(0.96);
}
</style>