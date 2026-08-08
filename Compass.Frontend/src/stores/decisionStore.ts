import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import axios from 'axios';
import { useToastStore } from './toastStore';
import { useCommitmentsStore } from './commitmentsStore'; 

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

  const rawTopActions = ref<ScoredActionDto[]>([]);
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

  



  const fetchDecisions = async (windowMinutes = 60, energy = 2, forceRefresh = false) => {
    availableWindow.value = windowMinutes;
    currentEnergy.value = energy;

    if (rawTopActions.value.length === 0 && !forceRefresh) {
    
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
        rawTopActions.value = res.data.topActions;
        adaptiveProfile.value = res.data.adaptiveProfile;
        isServingFromCache.value = false;
        lastSyncedAt.value = new Date();
     
      }
    } catch (err: any) {
      const hasOfflineData = rawTopActions.value.length > 0 
      if (!hasOfflineData) {
        toastStore.showToast('Sem conexão para calcular o Now Engine.', 'error');
      }
    } finally {
      isLoading.value = false;
    }
  };

  //  SOLUÇÃO 1: EXTERMÍNIO DE FANTASMAS (JOIN BLINDADO)
  const validTopActions = computed<ScoredActionDto[]>(() => {
    const commitmentsStore = useCommitmentsStore();
    
    return rawTopActions.value
      .filter(action => {
        const sourceEntity = commitmentsStore.entities[action.commitmentId];
        
        // Se a tarefa sumiu da Fonte de Verdade (Excluída), aborta a sugestão!
        if (!sourceEntity) return false; 
        
        // Se foi concluída ou arquivada, aborta a sugestão!
        if (sourceEntity.status === 'COMPLETED' || sourceEntity.status === 'ARCHIVED') return false;
        
        return true; 
      })
      .map(action => {
        // Hidratação segura (se chegou aqui, a entidade existe 100%)
        const sourceEntity = commitmentsStore.entities[action.commitmentId];
        return {
          ...action,
          title: sourceEntity.title,
          projectName: sourceEntity.projectName,
          type: sourceEntity.type
        };
      });
  });

  const primaryFocus = computed<ScoredActionDto | null>(() => {
    return validTopActions.value.length > 0 ? validTopActions.value[0] : null;
  });

  const secondaryActions = computed<ScoredActionDto[]>(() => {
    return validTopActions.value.length > 1 ? validTopActions.value.slice(1) : [];
  });

  const availableMinutes = computed<number>(() => availableWindow.value);

  return {
    topActions: validTopActions,
    adaptiveProfile,
    availableWindow,
    availableMinutes,
    currentEnergy,
    isLoading,
    isServingFromCache,
    lastSyncedAt,
    primaryFocus,
    secondaryActions,
    topFocus: primaryFocus,
    alternatives: secondaryActions,
    fetchDecisions,
    fetchNow: fetchDecisions
  
  };
});