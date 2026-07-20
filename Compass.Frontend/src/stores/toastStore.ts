import { defineStore } from 'pinia';
import { ref } from 'vue';

export interface ToastItem {
  id: string;
  message: string;
  type: 'neutral' | 'success' | 'urgent' | 'error';
  undoAction?: () => void | Promise<void>;
}

export const useToastStore = defineStore('toast', () => {
  const toasts = ref<ToastItem[]>([]);
  let toastTimer: ReturnType<typeof setTimeout> | null = null;

  const showToast = (
    message: string, 
    type: ToastItem['type'] = 'neutral', 
    undoAction?: () => void | Promise<void>,
    durationMs: number = 6000
  ) => {
    const id = `toast-${Date.now()}`;
    // No padrão industrial do Compass, exibimos apenas 1 toast ativo para evitar poluição visual
    toasts.value = [{ id, message, type, undoAction }];

    if (toastTimer) clearTimeout(toastTimer);
    toastTimer = setTimeout(() => {
      dismissToast(id);
    }, durationMs);
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

  return {
    toasts,
    showToast,
    dismissToast,
    executeUndo
  };
});