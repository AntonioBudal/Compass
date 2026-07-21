import type { App } from 'vue';

export const setupGlobalErrorHandler = (app: App) => {
  // 1. Captura de Erros Internos do Ciclo de Vida do Vue
  app.config.errorHandler = (err: any, instance, info) => {
    console.error('[Vue Global Error]:', err, info);
    
    if (import.meta.env.DEV) {
      import('@/stores/devStore').then(({ useDevStore }) => {
        try {
          const devStore = useDevStore();
          devStore.logDomainEvent({
            eventType: 'Vue Exception',
            entityId: 'UI_Component',
            message: err?.message || 'Erro Desconhecido no Ciclo de Vida',
            payload: { info, stack: err?.stack }
          });
        } catch (e) { /* Silenciamento de segurança */ }
      });
    }
  };

  // 2. Captura de Erros Nativos de Javascript (Window)
  window.addEventListener('error', (event) => {
    if (import.meta.env.DEV) {
      import('@/stores/devStore').then(({ useDevStore }) => {
        try {
          const devStore = useDevStore();
          devStore.logDomainEvent({
            eventType: 'Window Error',
            entityId: 'Browser',
            message: event.message,
            payload: { filename: event.filename, lineno: event.lineno, colno: event.colno }
          });
        } catch (e) { /* Silenciamento de segurança */ }
      });
    }
  });

  // 3. Captura de Promessas Rejeitadas sem Try/Catch (Unhandled Rejection)
  window.addEventListener('unhandledrejection', (event) => {
    if (import.meta.env.DEV) {
      import('@/stores/devStore').then(({ useDevStore }) => {
        try {
          const devStore = useDevStore();
          devStore.logDomainEvent({
            eventType: 'Unhandled Promise',
            entityId: 'Async_Operation',
            message: event.reason?.message || 'Promessa rejeitada sem tratamento',
            payload: { stack: event.reason?.stack }
          });
        } catch (e) { /* Silenciamento de segurança */ }
      });
    }
  });
};