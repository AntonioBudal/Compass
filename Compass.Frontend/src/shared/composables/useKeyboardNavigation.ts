import { ref, watch, type Ref } from 'vue';

export interface KeyboardStateMachineOptions {
  onSelect: (index: number) => void;
  onDismiss: () => void;
  onSubmitFallback: () => void;
}

/**
 * Máquina de estados isolada para navegação Zero-Mouse em listas suspensas.
 * Garante bloqueio de propagação para evitar submissões acidentais no DOM.
 */
export function useKeyboardNavigation(
  itemsCount: Ref<number>,
  options: KeyboardStateMachineOptions
) {
  const selectedIndex = ref(0);

  // Sempre que a lista de sugestões mudar (ex: digitou nova letra), reseta o cursor para o topo
  watch(itemsCount, () => {
    selectedIndex.value = 0;
  });

  const handleKeyDown = (e: KeyboardEvent, isDropdownOpen: boolean) => {
    // 1. MODO DROPDOWN ATIVO: O teclado controla exclusivamente a lista flutuante
    if (isDropdownOpen && itemsCount.value > 0) {
      if (e.key === 'ArrowDown') {
        e.preventDefault();
        e.stopPropagation();
        selectedIndex.value = (selectedIndex.value + 1) % itemsCount.value;
        return;
      }

      if (e.key === 'ArrowUp') {
        e.preventDefault();
        e.stopPropagation();
        selectedIndex.value = (selectedIndex.value - 1 + itemsCount.value) % itemsCount.value;
        return;
      }

      if (e.key === 'Tab' || e.key === 'Enter') {
        e.preventDefault();
        e.stopPropagation();
        options.onSelect(selectedIndex.value);
        return;
      }

      if (e.key === 'Escape') {
        e.preventDefault();
        e.stopPropagation();
        options.onDismiss();
        return;
      }
    }

    // 2. MODO INPUT LIVRE: Enter sem Shift submete a captura rápida
    if (e.key === 'Enter' && !e.shiftKey && !isDropdownOpen) {
      e.preventDefault();
      options.onSubmitFallback();
    }
  };

  return {
    selectedIndex,
    handleKeyDown
  };
}