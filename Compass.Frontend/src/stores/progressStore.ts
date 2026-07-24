import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import axios from 'axios';
import { useCommitmentsStore } from './commitmentsStore';
import { useToastStore } from './toastStore';

// --- Contratos DTO alinhados com o C# (.NET 10) ---
export interface ProgressOverviewDto {
  totalCompleted: number;
  totalPlanned: number;
  completionRatePercentage: number;
  estimationAccuracyIndex: number;
  hasImputedAccuracyData: boolean;
  totalDeepWorkMinutes: number;
  totalUsefulMinutes: number;
  totalPostponements: number;
  periodStartDateUtc: string;
  periodEndDateUtc: string;
}

export interface DailyTimeSliceDto {
  dateIso: string; // YYYY-MM-DD
  completedCount: number;
  estimatedMinutes: number;
  actualMinutes: number;
  deepWorkMinutes: number;
  postponedCount: number;
}

export interface ProcrastinationHeatmapDto {
  commitmentType: string;
  energyRequired: number;
  totalCount: number;
  postponedCount: number;
  postponementRatePercentage: number;
}

export interface ChronologyPeakDto {
  timeBucket: 'Morning' | 'Afternoon' | 'Evening' | 'Night';
  completedCount: number;
  deepWorkMinutes: number;
  efficiencyPercentage: number;
}

export type TimeRangeOption = '7d' | '30d' | '90d' | '1y';

export const useProgressStore = defineStore('progress', () => {
  const commitmentsStore = useCommitmentsStore();
  const toastStore = useToastStore();

  // --- Estado Reativo (State) ---
  const selectedRange = ref<TimeRangeOption>('30d');
  const isLoading = ref<boolean>(false);
  const isServingFromCache = ref<boolean>(false);
  const lastETag = ref<string | null>(null);

  // Armazenamento bruto vindo do servidor (Estritamente de Ontem para trás!)
  const rawOverview = ref<ProgressOverviewDto | null>(null);
  const rawDailySeries = ref<DailyTimeSliceDto[]>([]);
  const rawHeatmap = ref<ProcrastinationHeatmapDto[]>([]);
  const rawChronology = ref<ChronologyPeakDto[]>([]);

  // --- Funções Auxiliares Locais ---
  function getLocalIsoDate(date: Date = new Date()): string {
    const offset = date.getTimezoneOffset() * 60000;
    return new Date(date.getTime() - offset).toISOString().slice(0, 10);
  }

  function getStorageKey(endpoint: string, range: string): string {
    return `compass_progress_${endpoint}_${range}`;
  }

  function saveToDisk(range: string) {
    try {
      localStorage.setItem(getStorageKey('overview', range), JSON.stringify(rawOverview.value));
      localStorage.setItem(getStorageKey('daily', range), JSON.stringify(rawDailySeries.value));
      localStorage.setItem(getStorageKey('heatmap', range), JSON.stringify(rawHeatmap.value));
      localStorage.setItem(getStorageKey('chronology', range), JSON.stringify(rawChronology.value));
    } catch (e) {
      console.warn('[ProgressStore] Falha ao persistir telemetria no disco local.', e);
    }
  }

  function loadFromDisk(range: string): boolean {
    try {
      const ov = localStorage.getItem(getStorageKey('overview', range));
      const ds = localStorage.getItem(getStorageKey('daily', range));
      const hm = localStorage.getItem(getStorageKey('heatmap', range));
      const ch = localStorage.getItem(getStorageKey('chronology', range));

      if (ov && ds && hm && ch) {
        rawOverview.value = JSON.parse(ov);
        rawDailySeries.value = JSON.parse(ds);
        rawHeatmap.value = JSON.parse(hm);
        rawChronology.value = JSON.parse(ch);
        isServingFromCache.value = true;
        return true;
      }
    } catch (e) {
      console.warn('[ProgressStore] Cache local corrompido ou ausente.', e);
    }
    return false;
  }

  // --- Ação Principal de Carregamento (Actions) ---
  const fetchProgress = async (range: TimeRangeOption = selectedRange.value, forceRefresh = false) => {
    selectedRange.value = range;
    isLoading.value = true;
    isServingFromCache.value = false;

    const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';
    const headers: Record<string, string> = {
      'X-User-Id': '11111111-1111-1111-1111-111111111111'
    };

    if (!forceRefresh && lastETag.value) {
      headers['If-None-Match'] = lastETag.value;
    }

    try {
      // Dispara as 4 consultas analíticas em paralelo para máxima velocidade
      const [ovRes, dsRes, hmRes, chRes] = await Promise.all([
        axios.get(`${baseUrl}/progress/overview?timeRange=${range}`, { headers, validateStatus: () => true }),
        axios.get(`${baseUrl}/progress/daily-series?timeRange=${range}`, { headers, validateStatus: () => true }),
        axios.get(`${baseUrl}/progress/heatmap?timeRange=${range}`, { headers, validateStatus: () => true }),
        axios.get(`${baseUrl}/progress/chronology?timeRange=${range}`, { headers, validateStatus: () => true })
      ]);

      // Se qualquer endpoint retornar 304 Not Modified, mantemos o estado em RAM intacto!
      if (ovRes.status === 304) {
        console.log('[ProgressStore] ETag revalidada (304 Not Modified). Servindo da RAM.');
        isLoading.value = false;
        return;
      }

      if (ovRes.status === 200 && dsRes.status === 200 && hmRes.status === 200 && chRes.status === 200) {
        rawOverview.value = ovRes.data;
        rawDailySeries.value = dsRes.data;
        rawHeatmap.value = hmRes.data;
        rawChronology.value = chRes.data;

        if (ovRes.headers['etag']) {
          lastETag.value = ovRes.headers['etag'];
        }

        saveToDisk(range);
      } else {
        throw new Error(`HTTP Status Inesperado: ${ovRes.status}`);
      }
    } catch (err: any) {
      // --- INTERCEPTADOR DE RESILIÊNCIA LOCAL-FIRST ---
      console.warn('[ProgressStore] Falha de rede ou servidor inacessível. Hidratando via Disk Cache...', err);
      const restored = loadFromDisk(range);
      
      if (restored) {
        toastStore.showToast('[OFFLINE MODE] Telemetria servida do cache local.', 'neutral');
      } else {
        toastStore.showToast('Sem conexão e sem cache histórico para este período.', 'error');
      }
    } finally {
      isLoading.value = false;
    }
  };

  // --- MOTOR DE FUSÃO TEMPORAL (MERGE ENGINE IN RAM) ---
  
  // 1. Cálculo do Delta Operacional de HOJE em Memória RAM
  const todayDelta = computed(() => {
    const todayIso = getLocalIsoDate();
    const completedToday = commitmentsStore.items.filter(i => i.status === 'COMPLETED');
    const pendingToday = commitmentsStore.items.filter(i => i.status === 'PENDING' || i.status === 'IN_PROGRESS');

    const completedCount = completedToday.length;
    const plannedCount = completedCount + pendingToday.length;
    
    const usefulMinutes = completedToday.reduce((acc, curr) => acc + (curr.estimatedDurationMinutes || 30), 0);
    const deepWorkMinutes = completedToday
      .filter(i => i.energyRequired === 3)
      .reduce((acc, curr) => acc + (curr.estimatedDurationMinutes || 30), 0);

    const postponements = commitmentsStore.items.reduce((acc, curr) => acc + (curr.postponedCount || 0), 0);

    return {
      dateIso: todayIso,
      completedCount,
      plannedCount,
      usefulMinutes,
      deepWorkMinutes,
      postponements
    };
  });

  // 2. Série Temporal Diária Fundida (Histórico de Ontem + Delta de Hoje)
  const mergedDailySeries = computed<DailyTimeSliceDto[]>(() => {
    const historical = [...rawDailySeries.value];
    const today = todayDelta.value;

    if (today.completedCount === 0 && today.postponements === 0) {
      return historical; // Se nada aconteceu hoje ainda, retorna só o histórico
    }

    const todaySlice: DailyTimeSliceDto = {
      dateIso: today.dateIso,
      completedCount: today.completedCount,
      estimatedMinutes: today.usefulMinutes,
      actualMinutes: today.usefulMinutes, // No MVP, assumimos imputação contínua no dia corrente
      deepWorkMinutes: today.deepWorkMinutes,
      postponedCount: today.postponements
    };

    // Remove qualquer fatia acidental de hoje no histórico e anexa o cálculo em tempo real
    const filteredHistorical = historical.filter(s => s.dateIso !== today.dateIso);
    return [...filteredHistorical, todaySlice];
  });

  // 3. KPIs Gerais Fundidos (Overview Otimista)
  const mergedOverview = computed<ProgressOverviewDto | null>(() => {
    const base = rawOverview.value;
    const today = todayDelta.value;

    if (!base) {
      // Se não houver histórico, constrói um overview inteiramente a partir da RAM de hoje!
      if (today.plannedCount === 0) return null;
      return {
        totalCompleted: today.completedCount,
        totalPlanned: today.plannedCount,
        completionRatePercentage: Math.round((today.completedCount / today.plannedCount) * 100),
        estimationAccuracyIndex: 1.0,
        hasImputedAccuracyData: true,
        totalDeepWorkMinutes: today.deepWorkMinutes,
        totalUsefulMinutes: today.usefulMinutes,
        totalPostponements: today.postponements,
        periodStartDateUtc: new Date().toISOString(),
        periodEndDateUtc: new Date().toISOString()
      };
    }

    const newCompleted = base.totalCompleted + today.completedCount;
    const newPlanned = base.totalPlanned + today.plannedCount;
    const newRate = newPlanned > 0 ? Math.round((newCompleted / newPlanned) * 1000) / 10 : 0;

    return {
      ...base,
      totalCompleted: newCompleted,
      totalPlanned: newPlanned,
      completionRatePercentage: newRate,
      totalDeepWorkMinutes: base.totalDeepWorkMinutes + today.deepWorkMinutes,
      totalUsefulMinutes: base.totalUsefulMinutes + today.usefulMinutes,
      totalPostponements: base.totalPostponements + today.postponements
    };
  });

  return {
    selectedRange,
    isLoading,
    isServingFromCache,
    rawOverview,
    rawDailySeries,
    rawHeatmap,
    rawChronology,
    mergedOverview,
    mergedDailySeries,
    fetchProgress
  };
});