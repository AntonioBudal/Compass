import { onMounted, onUnmounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useDecisionStore } from '@/modules/execution/stores/decisionStore';
import { useToastStore } from '@/shared/stores/toastStore';
import { useInspectorStore } from '@/modules/tactical/stores/inspectorStore';

export const isCommandBarOpen = ref(false);
export const isQuickCaptureOpen = ref(false);

export function useKeyboardShortcuts() {
  const router = useRouter();
  const decisionStore = useDecisionStore();
  const toastStore = useToastStore();
  const inspectorStore = useInspectorStore();

  let gKeyPressed = false;
  let gKeyTimeout: ReturnType<typeof setTimeout> | null = null;

  const handleKeyDown = (e: KeyboardEvent) => {
    const target = e.target as HTMLElement;

    const isInput =
      target.tagName === 'INPUT' ||
      target.tagName === 'TEXTAREA' ||
      target.isContentEditable;

    // ESC
    if (e.key === 'Escape') {
      if (inspectorStore.isOpen) {
        e.preventDefault();
        inspectorStore.flushAndClose();
        return;
      }

      if (isCommandBarOpen.value) {
        e.preventDefault();
        isCommandBarOpen.value = false;
        return;
      }

      if (isQuickCaptureOpen.value) {
        e.preventDefault();
        isQuickCaptureOpen.value = false;
        return;
      }

      return;
    }

    // atalhos ficam bloqueados quando digitando
    if (isInput) return;

    // Ctrl/Cmd + Z
    if (
      (e.ctrlKey || e.metaKey) &&
      e.key.toLowerCase() === 'z' &&
      !e.shiftKey
    ) {
      e.preventDefault();

      const lastToast = [...toastStore.toasts]
        .reverse()
        .find(t => t.undoAction);

      if (lastToast) {
        toastStore.executeUndo(lastToast.id);
      }

      return;
    }

    // Ctrl/Cmd + K
    if (
      (e.ctrlKey || e.metaKey) &&
      e.key.toLowerCase() === 'k'
    ) {
      e.preventDefault();

      if (!inspectorStore.isOpen) {
        isCommandBarOpen.value = !isCommandBarOpen.value;
      }

      return;
    }

    // /
    if (
      e.key === '/' &&
      !isCommandBarOpen.value &&
      !isQuickCaptureOpen.value &&
      !inspectorStore.isOpen
    ) {
      e.preventDefault();
      isCommandBarOpen.value = true;
      return;
    }

    // C
    if (
      e.key.toLowerCase() === 'c' &&
      !e.ctrlKey &&
      !e.metaKey &&
      !inspectorStore.isOpen
    ) {
      e.preventDefault();
      isQuickCaptureOpen.value = true;
      return;
    }

    // atalhos da tela Now
    if (
      router.currentRoute.value.path === '/now' &&
      !inspectorStore.isOpen
    ) {
      if (e.key.toLowerCase() === 'e') { 
        e.preventDefault(); 
        // TODO: decisionStore.completeTopFocus(); 
        console.warn('Atalho E temporariamente desativado');
        return; 
      }
      if (e.key.toLowerCase() === 's') { 
        e.preventDefault(); 
        // TODO: decisionStore.postponeTopFocus(); 
        console.warn('Atalho S temporariamente desativado');
        return; 
      }
    }

    // sequência G
    if (
      e.key.toLowerCase() === 'g' &&
      !gKeyPressed &&
      !inspectorStore.isOpen
    ) {
      gKeyPressed = true;

      if (gKeyTimeout) {
        clearTimeout(gKeyTimeout);
      }

      gKeyTimeout = setTimeout(() => {
        gKeyPressed = false;
      }, 1500);

      return;
    }

    if (gKeyPressed) {
      gKeyPressed = false;

      if (gKeyTimeout) {
        clearTimeout(gKeyTimeout);
      }

      switch (e.key.toLowerCase()) {
        case 'n':
          router.push('/now');
          break;

        case 'a':
          router.push('/agenda');
          break;

        case 'p':
          router.push('/projects');
          break;

        case 'g':
          router.push('/goals');
          break;

        case 'h':
          router.push('/habits');
          break;

        case 'j':
          router.push('/journal');
          break;
      }
    }
  };

  onMounted(() => {
    window.addEventListener('keydown', handleKeyDown, { capture: true });
  });

  onUnmounted(() => {
    window.removeEventListener('keydown', handleKeyDown, { capture: true });

    if (gKeyTimeout) {
      clearTimeout(gKeyTimeout);
    }
  });
}