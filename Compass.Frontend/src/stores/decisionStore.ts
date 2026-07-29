import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import axios from 'axios';
import { useToastStore } from './toastStore';

export interface AdaptiveProfileDto {
  isCalibrated: boolean;
  sampleCount: number;
  eaiMultiplier: number;
  morningEnergyBias: number;
  afternoonEnergyBias: number;
  eveningEnergyBias: number;
}

export interface ScoredActionDto {
  id?: string;
  commitmentId: string;
  title: string;
  type: string;
  nominalDurationMinutes: number;
  effectiveDurationMinutes: number;
  energyRequired: number;
  scorePercentage: number;
  reason: string;
  wasTimeAdjustedByEai: boolean;
  projectName: string | null;
}

export interface DecisionResponseDto {
  generatedAtUtc: string;
  availableWindowMinutes: number;
  operatorEnergyLevel: number;
  adaptiveProfile: AdaptiveProfileDto;
  topActions: ScoredActionDto[];
}

const STORAGE_KEY = 'compass_now_engine_cache_v3';

export const useDecisionStore = defineStore('decision', () => {
  const toastStore = useToastStore();

  // --- Estado Reativo ---
  const topActions = ref<ScoredActionDto[]>([]);
  const adaptiveProfile = ref<AdaptiveProfileDto>({
    isCalibrated: false,
    sampleCount: 0,
    eaiMultiplier: 1.0,
    morningEnergyBias: 1.0,
    afternoonEnergyBias: 1.0,
    eveningEnergyBias: 1.0
  });
  const availableWindow = ref<number>(60);
  const currentEnergy = ref<number>(2);
  const isLoading = ref<boolean>(false);
  const isServingFromCache = ref<boolean>(false);
  const lastSyncedAt = ref<Date | null>(null);

  // --- Persistência em Disco (Resiliência Offline) ---
  function saveToDisk() {
    try {
      const payload = {
        timestamp: new Date().toISOString(),
        profile: adaptiveProfile.value,
        actions: topActions.value,
        window: availableWindow.value,
        energy: currentEnergy.value
      };
      localStorage.setItem(STORAGE_KEY, JSON.stringify(payload));
    } catch (e) {
      console.warn('[DecisionStore] Falha ao gravar perfil adaptativo no localStorage.', e);
    }
  }

  function loadFromDisk(): boolean {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (raw) {
        const parsed = JSON.parse(raw);
        adaptiveProfile.value = parsed.profile || adaptiveProfile.value;
        topActions.value = parsed.actions || [];
        availableWindow.value = parsed.window || 60;
        currentEnergy.value = parsed.energy || 2;
        lastSyncedAt.value = parsed.timestamp ? new Date(parsed.timestamp) : null;
        isServingFromCache.value = true;
        return true;
      }
    } catch (e) {
      console.warn('[DecisionStore] Cache local corrompido. Reiniciando com perfil basal.', e);
    }
    return false;
  }

  // --- Sincronização Principal com o Backend ---
  const fetchDecisions = async (windowMinutes = 60, energy = 2, forceRefresh = false) => {
    availableWindow.value = windowMinutes;
    currentEnergy.value = energy;

    if (topActions.value.length === 0 && !forceRefresh) {
      loadFromDisk();
    }

    isLoading.value = true;
    const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';
    const headers = { 'X-User-Id': '11111111-1111-1111-1111-111111111111' };

    try {
      const res = await axios.get<DecisionResponseDto>(
        `${baseUrl}/now?windowMinutes=${windowMinutes}&energy=${energy}`, 
        { headers, timeout: 5000 }
      );

      if (res.status === 200) {
        topActions.value = res.data.topActions;
        adaptiveProfile.value = res.data.adaptiveProfile;
        isServingFromCache.value = false;
        lastSyncedAt.value = new Date();
        saveToDisk();
      }
    } catch (err: any) {
      console.warn('[DecisionStore] Falha na API do Now Engine. Ativando fallback analítico offline...', err);
      
      const hasOfflineData = topActions.value.length > 0 || loadFromDisk();
      if (hasOfflineData) {
        toastStore.showToast('[OFFLINE] Recomendações servidas do cache comportamental.', 'neutral');
      } else {
        toastStore.showToast('Sem conexão para calcular o Now Engine.', 'error');
      }
    } finally {
      isLoading.value = false;
    }
  };

  // --- Getters Computados Compatíveis ---
  const primaryFocus = computed<ScoredActionDto | null>(() => {
    return topActions.value.length > 0 ? topActions.value[0] : null;
  });

  const secondaryActions = computed<ScoredActionDto[]>(() => {
    return topActions.value.length > 1 ? topActions.value.slice(1) : [];
  });

  const availableMinutes = computed<number>(() => availableWindow.value);

  return {
    topActions,
    adaptiveProfile,
    availableWindow,
    availableMinutes,
    currentEnergy,
    isLoading,
    isServingFromCache,
    lastSyncedAt,
    primaryFocus,
    secondaryActions,
    topFocus: primaryFocus,          // Alias reativo para compatibilidade visual
    alternatives: secondaryActions,  // Alias reativo para compatibilidade visual
    fetchDecisions,
    fetchNow: fetchDecisions,
    loadFromDisk
  };
});