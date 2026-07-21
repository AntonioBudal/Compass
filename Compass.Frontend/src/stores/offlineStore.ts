import { defineStore } from 'pinia';
import { ref, onMounted, onUnmounted } from 'vue';
import axios from 'axios';
import { useToastStore } from '@/stores/toastStore';

export interface QueuedRequest {
  id: string;
  url: string;
  method: string;
  payload: any;
  timestamp: number;
}

export const useOfflineStore = defineStore('offline', () => {
  const isOnline = ref(navigator.onLine);
  const isSyncingQueue = ref(false);
  const queue = ref<QueuedRequest[]>([]);

  // Carrega fila persistida de sessões anteriores
  const loadQueueFromStorage = () => {
    try {
      const saved = localStorage.getItem('compass_offline_queue');
      if (saved) {
        queue.value = JSON.parse(saved);
      }
    } catch (e) {
      console.warn('[OfflineStore]: Falha ao carregar fila local.', e);
    }
  };

  const saveQueueToStorage = () => {
    try {
      localStorage.setItem('compass_offline_queue', JSON.stringify(queue.value));
    } catch (e) {
      console.warn('[OfflineStore]: Falha ao persistir fila local.', e);
    }
  };

  const addToQueue = (req: Omit<QueuedRequest, 'id' | 'timestamp'>) => {
    const newEntry: QueuedRequest = {
      ...req,
      id: 'queue-' + Math.random().toString(36).substring(2, 9),
      timestamp: Date.now()
    };
    queue.value.push(newEntry);
    saveQueueToStorage();

    if (import.meta.env.DEV) {
      import('@/stores/devStore').then(({ useDevStore }) => {
        try {
          const devStore = useDevStore();
          devStore.logDomainEvent({
            eventType: 'Offline Queue',
            entityId: 'Network_Layer',
            message: `Transação [${req.method}] enfileirada localmente.`,
            payload: newEntry
          });
        } catch (e) { /* Silenciamento seguro */ }
      });
    }
  };

  // Processa a fila silenciosamente quando a conexão retorna
  const processQueue = async () => {
    if (queue.value.length === 0 || isSyncingQueue.value || !isOnline.value) return;

    isSyncingQueue.value = true;
    const toastStore = useToastStore();
    const pending = [...queue.value];
    let successCount = 0;

    for (const item of pending) {
      try {
        await axios({
          url: item.url,
          method: item.method,
          data: item.payload,
          headers: {
            'Content-Type': 'application/json',
            'X-User-Id': '11111111-1111-1111-1111-111111111111',
            'X-Correlation-Id': 'offline-retry-' + item.id
          }
        });
        
        // Remove da fila ao sincronizar com sucesso
        queue.value = queue.value.filter(q => q.id !== item.id);
        saveQueueToStorage();
        successCount++;
      } catch (err) {
        // Se falhar por erro de validação (400/422), descarta para não bloquear a fila.
        // Se for erro de rede/5xx, mantém na fila para tentar mais tarde.
        if (axios.isAxiosError(err) && err.response && err.response.status < 500) {
          queue.value = queue.value.filter(q => q.id !== item.id);
          saveQueueToStorage();
        }
        break;
      }
    }

    isSyncingQueue.value = false;
    if (successCount > 0) {
      toastStore.showToast(`${successCount} transações offline foram sincronizadas com o servidor.`, 'success');
    }
  };

  const handleOnline = () => {
    isOnline.value = true;
    processQueue();
  };

  const handleOffline = () => {
    isOnline.value = false;
  };

  const initNetworkListeners = () => {
    loadQueueFromStorage();
    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);
    
    // Tenta processar a fila ao inicializar o app
    if (isOnline.value && queue.value.length > 0) {
      setTimeout(processQueue, 2000);
    }
  };

  const removeListeners = () => {
    window.removeEventListener('online', handleOnline);
    window.removeEventListener('offline', handleOffline);
  };

  return {
    isOnline,
    isSyncingQueue,
    queue,
    addToQueue,
    processQueue,
    initNetworkListeners,
    removeListeners
  };
});