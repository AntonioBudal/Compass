import { watch, nextTick, type Ref } from 'vue';

export function useFocusTrap(elRef: Ref<HTMLElement | null>, isOpen: Ref<boolean>) {
  let previousActiveElement: HTMLElement | null = null;

  // Busca apenas elementos que podem receber foco e que não estão desabilitados
  const getFocusableElements = (): HTMLElement[] => {
    if (!elRef.value) return [];
    const selector = 'button:not([disabled]), [href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"]):not([disabled])';
    return Array.from(elRef.value.querySelectorAll(selector)) as HTMLElement[];
  };

  const handleKeyDown = (e: KeyboardEvent) => {
    if (!isOpen.value || e.key !== 'Tab') return;

    const focusables = getFocusableElements();
    if (focusables.length === 0) {
      e.preventDefault();
      return;
    }

    const firstElement = focusables[0];
    const lastElement = focusables[focusables.length - 1];

    if (e.shiftKey) {
      // Shift + Tab: Se estiver no primeiro elemento, joga para o último
      if (document.activeElement === firstElement || !elRef.value?.contains(document.activeElement)) {
        e.preventDefault();
        lastElement.focus();
      }
    } else {
      // Tab normal: Se estiver no último elemento, joga para o primeiro
      if (document.activeElement === lastElement || !elRef.value?.contains(document.activeElement)) {
        e.preventDefault();
        firstElement.focus();
      }
    }
  };

  watch(isOpen, async (newVal) => {
    if (newVal) {
      // Salva o elemento da tela de fundo que abriu o modal para devolver o foco depois
      previousActiveElement = document.activeElement as HTMLElement;
      window.addEventListener('keydown', handleKeyDown);
      
      await nextTick();
      const focusables = getFocusableElements();
      if (focusables.length > 0) {
        // Tenta focar automaticamente quem tiver o atributo 'autofocus' ou o primeiro da lista
        const autoFocusEl = focusables.find(el => el.hasAttribute('autofocus')) || focusables[0];
        autoFocusEl.focus();
      }
    } else {
      window.removeEventListener('keydown', handleKeyDown);
      // Ao fechar o modal, devolve o foco com precisão para o elemento original
      if (previousActiveElement && typeof previousActiveElement.focus === 'function') {
        previousActiveElement.focus();
      }
    }
  });
}