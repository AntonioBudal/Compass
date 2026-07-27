import { defineStore } from 'pinia';
import { ref, onMounted, onUnmounted } from 'vue';
import axios from 'axios';
import { useToastStore } from './toastStore';

export interface MorningBriefing {
  date: string;
  pendingTasksCount: number;
  overdueTasksCount: number;
  habitsToCheckCount: number;
  totalEstimatedFocusMinutes: number;
  topFocusTitle: string;
  greetingMessage: string;
}

export interface DailyShutdownPayload {
  completedCount: number;
  postponedCount: number;
  totalFocusMinutes: number;
  notes: string;
  divergenceTags: string[];
}

export const useDailyCycleStore = defineStore('dailyCycle', () => {
  const toastStore = useToastStore();

  const briefing = ref<MorningBriefing | null>(null);
  const isLoading = ref(false);
  
  // TRAVA ANTI-DUPLICAÇÃO: Bloqueia chamadas redundantes ao backend
  const isSubmitting = ref(false);
  const lastShutdownDate = ref<string | null>(null);

  // CANAL DE CONCORRÊNCIA ENTRE ABAS
  let broadcastChannel: BroadcastChannel | null = null;

  function initCrossTabSync() {
    if (typeof window !== 'undefined' && 'BroadcastChannel' in window) {
      broadcastChannel = new BroadcastChannel('compass_daily_cycle');
      broadcastChannel.onmessage = (event) => {
        if (event.data?.type === 'SHUTDOWN_COMPLETED') {
          lastShutdownDate.value = event.data.date;
          toastStore.showToast('Revisão diária sincronizada a partir de outra aba.', 'neutral');
        }
      };
    }
  }

  const fetchMorningBriefing = async (force = false) => {
    if (briefing.value && !force) return;
    isLoading.value = true;
    
    try {
      const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';
      const headers = { 'X-User-Id': '11111111-1111-1111-1111-111111111111' };
      
      const res = await axios.get<MorningBriefing>(
        `${baseUrl}/daily-cycle/morning-briefing?timeZoneId=America/Sao_Paulo`, 
        { headers }
      );
      briefing.value = res.data;
    } catch (e) {
      console.warn('[DailyCycleStore] Falha ao carregar briefing matinal.', e);
    } finally {
      isLoading.value = false;
    }
  };

  const executeShutdown = async (payload: DailyShutdownPayload): Promise<boolean> => {
    if (isSubmitting.value) {
      console.warn('[DailyCycleStore] Submissão já em andamento. Trava anti-duplicação ativa.');
      return false;
    }

    isSubmitting.value = true;
    try {
      const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';
      const headers = { 
        'X-User-Id': '11111111-1111-1111-1111-111111111111',
        'Content-Type': 'application/json'
      };

      const res = await axios.post(
        `${baseUrl}/daily-cycle/shutdown?timeZoneId=America/Sao_Paulo`, 
        payload, 
        { headers }
      );

      if (res.status === 200) {
        lastShutdownDate.value = res.data.reviewDate;
        
        // PROPAGA PARA OUTRAS ABAS ABERTAS
        if (broadcastChannel) {
          broadcastChannel.postMessage({ 
            type: 'SHUTDOWN_COMPLETED', 
            date: res.data.reviewDate 
          });
        }

        toastStore.showToast('Encerramento diário registrado com sucesso!', 'success');
        return true;
      }
    } catch (err: any) {
      console.error('[DailyCycleStore] Falha no encerramento diário.', err);
      toastStore.showToast('Erro ao salvar telemetria do dia. Tente novamente.', 'error');
    } finally {
      isSubmitting.value = false;
    }
    return false;
  };

  initCrossTabSync();

  return {
    briefing,
    isLoading,
    isSubmitting,
    lastShutdownDate,
    fetchMorningBriefing,
    executeShutdown
  };
});