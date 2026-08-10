import { defineStore } from 'pinia';
import { ref } from 'vue';
import axios from 'axios';
import { useToastStore } from '@/shared/stores/toastStore';

export interface QueuedRequest {
  id: string;
  url: string;
  method: string;
  payload: any;
  timestamp: number;
  executeAfter?: number; //  ARQ: Permite programar a execução no futuro
}

export const useOfflineStore = defineStore('offline', () => {
  const isOnline = ref(navigator.onLine);
  const isSyncingQueue = ref(false);
  const queue = ref<QueuedRequest[]>([]);

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

  //  ARQ: Agora retorna o ID para que a UI possa cancelar o comando
  const addToQueue = (req: Omit<QueuedRequest, 'id' | 'timestamp'>): string => {
    const operationId = 'queue-' + Math.random().toString(36).substring(2, 9);
    
    const newEntry: QueuedRequest = {
      ...req,
      id: operationId,
      timestamp: Date.now()
    };
    
    queue.value.push(newEntry);
    saveQueueToStorage();

    // Se estiver online e o comando não tiver delay, tenta processar a fila imediatamente
    if (isOnline.value && !req.executeAfter) {
      setTimeout(processQueue, 100);
    }

    return operationId;
  };

  //  ARQ: Cancelamento de Comando (Undo)
  const cancelRequest = (operationId: string) => {
    const initialLength = queue.value.length;
    queue.value = queue.value.filter(q => q.id !== operationId);
    
    if (queue.value.length < initialLength) {
      saveQueueToStorage();
    }
  };

  const processQueue = async () => {
    if (queue.value.length === 0 || isSyncingQueue.value || !isOnline.value) return;

    isSyncingQueue.value = true;
    const toastStore = useToastStore();
    
    const now = Date.now();
    //  ARQ: Só processa itens que não têm delay ou cujo tempo de espera já acabou
    const pending = queue.value.filter(req => !req.executeAfter || now >= req.executeAfter);
    
    if (pending.length === 0) {
      isSyncingQueue.value = false;
      return; 
    }

    let successCount = 0;
    const unresolvedIds = new Set(queue.value.map(p => p.id));
    const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';

    for (const item of pending) {
      try {
        //  ARQ: Ajuste de Mira! Garante que a requisição vá para o Backend (5000) e não para o Vue (5173)
        const fullUrl = item.url.startsWith('http') ? item.url : `${baseUrl}${item.url.startsWith('/') ? '' : '/'}${item.url}`;

        await axios({
          url: fullUrl,
          method: item.method,
          data: item.payload,
          headers: {
            'Content-Type': 'application/json',
            'X-User-Id': '11111111-1111-1111-1111-111111111111',
            'X-Correlation-Id': 'offline-retry-' + item.id
          }
        });
        
        unresolvedIds.delete(item.id);
        successCount++;
      } catch (err: any) {
        if (axios.isAxiosError(err) && err.response && err.response.status >= 400 && err.response.status < 500) {
          
          if (item.method.toUpperCase() === 'DELETE' && err.response.status === 404) {
            unresolvedIds.delete(item.id);
          } 
          // Erros 400 (Bad Request) ou 422 descarta da fila para não criar loop infinito.
          else {
            unresolvedIds.delete(item.id);
            //console.error(`[Offline Sync] Descartando requisição corrompida (HTTP ${err.response.status}):`, fullurl);
          }
        } else {
          // Erro 5xx ou Falha de Rede -> Interrompe o processamento e tenta depois
          break; 
        }
      }
    }

    queue.value = queue.value.filter(q => unresolvedIds.has(q.id));
    saveQueueToStorage();

    isSyncingQueue.value = false;
    
    if (successCount > 0) {
      toastStore.showToast(`${successCount} transações pendentes sincronizadas.`, 'neutral');
      if (typeof window !== 'undefined') {
        window.dispatchEvent(new CustomEvent('compass:offline-sync-complete'));
      }
    }
    
    //  Loop de Autocorreção: Se sobraram itens com delay, agenda uma nova verificação
    const hasDelayedItems = queue.value.some(req => req.executeAfter && req.executeAfter > now);
    if (hasDelayedItems && isOnline.value) {
      setTimeout(processQueue, 2000); 
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
    cancelRequest,
    processQueue,
    initNetworkListeners,
    removeListeners
  };
});