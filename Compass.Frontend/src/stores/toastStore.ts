import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { DefensiveIntervention, DefensiveAction } from '@/types/index';

export interface ToastItem {
  id: string;
  message: string;
  type: 'neutral' | 'success' | 'urgent' | 'error';
  undoAction?: () => void | Promise<void>;
  intervention?: DefensiveIntervention;
}

export const useToastStore = defineStore('toast', () => {
  const toasts = ref<ToastItem[]>([]);
  let toastTimer: ReturnType<typeof setTimeout> | null = null;

  // 1. Toast Padrão (Curta duração / Feedback simples)
  const showToast = (
    message: string, 
    type: ToastItem['type'] = 'neutral', 
    undoAction?: () => void | Promise<void>,
    durationMs: number = 6000
  ) => {
    const id = `toast-${Date.now()}`;
    toasts.value = [{ id, message, type, undoAction }];

    if (toastTimer) clearTimeout(toastTimer);
    toastTimer = setTimeout(() => {
      dismissToast(id);
    }, durationMs);
  };

  // 2. UX Defensiva: Intervenção Explicativa Acionável
  const showIntervention = (
    intervention: Omit<DefensiveIntervention, 'id'>,
    durationMs?: number
  ) => {
    const id = `intervention-${Date.now()}`;
    const fullIntervention: DefensiveIntervention = { ...intervention, id };

    // Determina a duração automática com base na severidade (blocking = permace aberto)
    const effectiveDuration = durationMs ?? (
      intervention.severity === 'blocking' ? 0 : 
      intervention.severity === 'warning' ? 10000 : 7000
    );

    toasts.value = [{
      id,
      message: intervention.title,
      type: intervention.severity === 'blocking' ? 'error' : 
            intervention.severity === 'warning' ? 'urgent' : 'neutral',
      intervention: fullIntervention
    }];

    if (toastTimer) clearTimeout(toastTimer);
    if (effectiveDuration > 0) {
      toastTimer = setTimeout(() => {
        dismissToast(id);
      }, effectiveDuration);
    }
  };

  const dismissToast = (id: string) => {
    toasts.value = toasts.value.filter(t => t.id !== id);
    if (toasts.value.length === 0 && toastTimer) {
      clearTimeout(toastTimer);
      toastTimer = null;
    }
  };

  const executeUndo = async (id: string) => {
    const target = toasts.value.find(t => t.id === id);
    if (target && target.undoAction) {
      await target.undoAction();
    }
    dismissToast(id);
  };

  // Executa o handler escolhido pelo usuário na intervenção e fecha o card
  const executeInterventionAction = async (toastId: string, action: DefensiveAction) => {
    try {
      await action.handler();
    } catch (e) {
      console.error('[ToastStore] Erro ao executar ação de intervenção:', e);
    } finally {
      dismissToast(toastId);
    }
  };

  return {
    toasts,
    showToast,
    showIntervention,
    dismissToast,
    executeUndo,
    executeInterventionAction
  };
});