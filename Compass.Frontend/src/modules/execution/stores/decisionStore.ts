import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { DecisionsApi } from '@/modules/execution/api/decisions.api';
import { useToastStore } from '../../../shared/stores/toastStore';
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

    isLoading.value = true;

    try {
      //  FIX 1: Forçamos o 'any' para ignorar o conflito de interface global vs local
      const data: any = await DecisionsApi.getNowDecision(windowMinutes, energy);
      
      //  FIX 2: Adaptamos para o formato atual do backend C# (que devolve topFocus e alternatives)
      // Caso o backend antigo devolvesse 'topActions', fazemos o fallback.
      if (data.topFocus) {
          rawTopActions.value = [data.topFocus, ...(data.alternatives || [])];
      } else {
          rawTopActions.value = data.topActions || [];
      }
      
      adaptiveProfile.value = data.adaptiveProfile || data.context?.adaptiveProfile || {
        isCalibrated: false, sampleCount: 0, eaiMultiplier: 1.0, morningEnergyBias: 1.0, afternoonEnergyBias: 1.0, eveningEnergyBias: 1.0
      };

      isServingFromCache.value = false;
      lastSyncedAt.value = new Date();
      
      //  FIX 3: O bloco antigo do `if (res.status === 200)` foi deletado completamente!

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