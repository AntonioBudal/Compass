import { onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import { useDecisionStore } from '@/stores/decisionStore';

// Estado global para comunicação entre os atalhos e os Modais (que faremos na Parte 2)
import { ref } from 'vue';
export const isCommandBarOpen = ref(false);
export const isQuickCaptureOpen = ref(false);

export function useKeyboardShortcuts() {
  const router = useRouter();
  const decisionStore = useDecisionStore();
  let gKeyPressed = false;
  let gKeyTimeout: ReturnType<typeof setTimeout> | null = null;

  const handleKeyDown = (e: KeyboardEvent) => {
    // 1. Ignora atalhos se o usuário estiver digitando em um formulário (Cap. 4.6)
    const target = e.target as HTMLElement;
    const isInput = target.tagName === 'INPUT' || target.tagName === 'TEXTAREA' || target.isContentEditable;

    // Permitir Escape em qualquer lugar para fechar modais
    if (e.key === 'Escape') {
      if (isCommandBarOpen.value) { isCommandBarOpen.value = false; return; }
      if (isQuickCaptureOpen.value) { isQuickCaptureOpen.value = false; return; }
      return;
    }

    if (isInput) return;

    // 2. Atalho de Busca Global: Cmd+K (Mac) ou Ctrl+K (Win) / Tecla '/'
    if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === 'k') {
      e.preventDefault();
      isCommandBarOpen.value = !isCommandBarOpen.value;
      return;
    }
    if (e.key === '/' && !isCommandBarOpen.value && !isQuickCaptureOpen.value) {
      e.preventDefault();
      isCommandBarOpen.value = true;
      return;
    }

    // 3. Captura Rápida: Tecla 'C' ou 'c'
    if (e.key.toLowerCase() === 'c' && !e.metaKey && !e.ctrlKey) {
      e.preventDefault();
      isQuickCaptureOpen.value = true;
      return;
    }

    // 4. Ações da Tela "Agora" (Concluir 'E' | Adiar 'S')
    if (router.currentRoute.value.path === '/now') {
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

    // 5. Navegação em 2 Teclas (G -> N, G -> A, etc.)
    if (e.key.toLowerCase() === 'g' && !gKeyPressed) {
      gKeyPressed = true;
      if (gKeyTimeout) clearTimeout(gKeyTimeout);
      gKeyTimeout = setTimeout(() => { gKeyPressed = false; }, 1500); // 1.5s de janela
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
        case 'd': router.push('/audit'); break;
        case 's': router.push('/settings'); break;
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