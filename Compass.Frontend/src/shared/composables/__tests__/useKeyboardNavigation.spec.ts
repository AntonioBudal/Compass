import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ref } from 'vue';
import { useKeyboardNavigation } from '../../shared/composables/useKeyboardNavigation';

describe('useKeyboardNavigation — Máquina de Estados de Teclado Zero-Mouse', () => {
  const itemsCount = ref(5);
  let onSelectMock: any;
  let onDismissMock: any;
  let onSubmitFallbackMock: any;
  let nav: ReturnType<typeof useKeyboardNavigation>;

  beforeEach(() => {
    onSelectMock = vi.fn();
    onDismissMock = vi.fn();
    onSubmitFallbackMock = vi.fn();
    itemsCount.value = 5;

    nav = useKeyboardNavigation(itemsCount, {
      onSelect: onSelectMock,
      onDismiss: onDismissMock,
      onSubmitFallback: onSubmitFallbackMock
    });
  });

  it('deve navegar para baixo cíclicamente sem submeter o modal', () => {
    const event = new KeyboardEvent('keydown', { key: 'ArrowDown', cancelable: true });
    const preventSpy = vi.spyOn(event, 'preventDefault');
    const stopSpy = vi.spyOn(event, 'stopPropagation');

    nav.handleKeyDown(event, true);
    expect(nav.selectedIndex.value).toBe(1);
    expect(preventSpy).toHaveBeenCalled();
    expect(stopSpy).toHaveBeenCalled();
    expect(onSubmitFallbackMock).not.toHaveBeenCalled();
  });

  it('deve dar a volta para o final da lista ao pressionar seta para cima no índice 0', () => {
    const event = new KeyboardEvent('keydown', { key: 'ArrowUp' });
    nav.handleKeyDown(event, true);
    expect(nav.selectedIndex.value).toBe(4); // (0 - 1 + 5) % 5 = 4
  });

  it('deve acionar onSelect ao pressionar Tab ou Enter no dropdown ativo', () => {
    nav.selectedIndex.value = 2;
    const enterEvent = new KeyboardEvent('keydown', { key: 'Enter' });
    
    nav.handleKeyDown(enterEvent, true);
    expect(onSelectMock).toHaveBeenCalledWith(2);
    expect(onSubmitFallbackMock).not.toHaveBeenCalled();
  });

  it('deve acionar onSubmitFallback quando Enter for pressionado com o dropdown FECHADO', () => {
    const enterEvent = new KeyboardEvent('keydown', { key: 'Enter' });
    
    nav.handleKeyDown(enterEvent, false); // isDropdownOpen = false
    expect(onSubmitFallbackMock).toHaveBeenCalled();
    expect(onSelectMock).not.toHaveBeenCalled();
  });
});