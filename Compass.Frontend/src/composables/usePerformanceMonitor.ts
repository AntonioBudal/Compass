import { onMounted, onUnmounted } from 'vue';

export function usePerformanceMonitor() {
  let observer: PerformanceObserver | null = null;

  const initLongTaskObserver = () => {
    // Verifica se o navegador suporta a API nativa
    if (typeof PerformanceObserver === 'undefined') return;

    try {
      observer = new PerformanceObserver((list) => {
        const entries = list.getEntries();
        for (const entry of entries) {
          // Captura tarefas que congelaram a UI por > 50ms
          if (entry.duration > 50) {
            if (import.meta.env.DEV) {
              import('@/stores/devStore').then(({ useDevStore }) => {
                try {
                  const devStore = useDevStore();
                  devStore.logMetric({
                    type: 'LONG_TASK',
                    name: entry.name || 'Main Thread Blocked',
                    durationMs: Math.round(entry.duration),
                    timestamp: new Date().toISOString().slice(11, 23),
                    detail: `Execução pesada bloqueou a UI por ${Math.round(entry.duration)}ms`
                  });
                } catch (e) { /* Silenciamento seguro */ }
              });
            }
          }
        }
      });

      observer.observe({ entryTypes: ['longtask'] });
    } catch (e) {
      console.warn('[PerformanceMonitor]: Falha ao iniciar observador de Long Tasks.', e);
    }
  };

  const measureComponentRender = (componentName: string, callback: () => void) => {
    const start = performance.now();
    callback();
    const duration = Math.round(performance.now() - start);

    if (duration > 16 && import.meta.env.DEV) { // > 16ms significa que perdeu 1 frame de 60FPS
      import('@/stores/devStore').then(({ useDevStore }) => {
        try {
          const devStore = useDevStore();
          devStore.logMetric({
            type: 'RENDER_SLOW',
            name: componentName,
            durationMs: duration,
            timestamp: new Date().toISOString().slice(11, 23),
            detail: `Componente excedeu orçamento de 16ms (60FPS)`
          });
        } catch (e) { /* Silenciamento seguro */ }
      });
    }
  };

  onMounted(() => {
    initLongTaskObserver();
  });

  onUnmounted(() => {
    if (observer) observer.disconnect();
  });

  return {
    measureComponentRender
  };
}