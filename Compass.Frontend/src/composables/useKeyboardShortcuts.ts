
import { onMounted, onUnmounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useDecisionStore } from '@/stores/decisionStore';
import { useToastStore } from '@/stores/toastStore';
import type { CommitmentItem } from '@/stores/commitmentsStore';

export const isCommandBarOpen = ref(false);
export const isQuickCaptureOpen = ref(false);
export const editingCommitment = ref<CommitmentItem | null>(null);

export function useKeyboardShortcuts() {
  const router = useRouter();
  const decisionStore = useDecisionStore();
  const toastStore = useToastStore();
  let gKeyPressed = false;
  let gKeyTimeout: ReturnType<typeof setTimeout> | null = null;

  const handleKeyDown = (e: KeyboardEvent) => {
    const target = e.target as HTMLElement;
    const isInput = target.tagName === 'INPUT' || target.tagName === 'TEXTAREA' || target.isContentEditable;

    // Teclas globais de controle de Modal
    if (e.key === 'Escape') {
      if (editingCommitment.value) { editingCommitment.value = null; return; }
      if (isCommandBarOpen.value) { isCommandBarOpen.value = false; return; }
      if (isQuickCaptureOpen.value) { isQuickCaptureOpen.value = false; return; }
      return;
    }

    if (isInput) return;

    // Atalho de Desfazer: Cmd+Z ou Ctrl+Z
    if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === 'z' && !e.shiftKey) {
      e.preventDefault();
      const lastToast = toastStore.toasts.find(t => t.undoAction);
      if (lastToast) {
        toastStore.executeUndo(lastToast.id);
      }
      return;
    }

    // Busca Global
    if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === 'k') {
      e.preventDefault();
      isCommandBarOpen.value = !isCommandBarOpen.value;
      return;
    }
    if (e.key === '/' && !isCommandBarOpen.value && !isQuickCaptureOpen.value && !editingCommitment.value) {
      e.preventDefault();
      isCommandBarOpen.value = true;
      return;
    }

    // Captura Rápida
    if (e.key.toLowerCase() === 'c' && !e.metaKey && !e.ctrlKey && !editingCommitment.value) {
      e.preventDefault();
      isQuickCaptureOpen.value = true;
      return;
    }

    // Ações do Motor na Tela Agora
    if (router.currentRoute.value.path === '/now' && !editingCommitment.value) {
      if (e.key.toLowerCase() === 'e') {
        e.preventDefault();
        decisionStore.completeTopFocus();
        return;
      }
      if (e.key.toLowerCase() === 's') {
        e.preventDefault();
        decisionStore.postponeTopFocus();
        return;
      }
    }

    // Navegação em 2 Teclas (G -> X)
    if (e.key.toLowerCase() === 'g' && !gKeyPressed && !editingCommitment.value) {
      gKeyPressed = true;
      if (gKeyTimeout) clearTimeout(gKeyTimeout);
      gKeyTimeout = setTimeout(() => { gKeyPressed = false; }, 1500);
      return;
    }

    if (gKeyPressed) {
      gKeyPressed = false;
      if (gKeyTimeout) clearTimeout(gKeyTimeout);
      
      switch (e.key.toLowerCase()) {
        case 'n': router.push('/now'); break;
        case 'a': router.push('/agenda'); break;
        case 'p': router.push('/projects'); break;
        case 'g': router.push('/goals'); break;
        case 'h': router.push('/habits'); break;
        case 'j': router.push('/journal'); break;
      }
    }
  };

  onMounted(() => {
    window.addEventListener('keydown', handleKeyDown, { capture: true });
  });

  onUnmounted(() => {
    window.removeEventListener('keydown', handleKeyDown, { capture: true });
    if (gKeyTimeout) clearTimeout(gKeyTimeout);
  });
}