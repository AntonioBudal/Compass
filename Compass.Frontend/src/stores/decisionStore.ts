import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { CompassApi } from '@/services/api';
import { useCommitmentsStore } from './commitmentsStore';
import type { 
  DecisionResponseDto, 
  ScoredActionDto 
} from '@/types/index';

export const useDecisionStore = defineStore('decision', () => {
  const commitmentsStore = useCommitmentsStore();

  // --- Estado Reativo (State) ---
  const currentDecision = ref<DecisionResponseDto | null>(null);
  const isLoading = ref<boolean>(false);
  const error = ref<string | null>(null);
  
  // Override local de energia (Permite que o usuário mude de 2 para 3 na UI instantaneamente)
  const userOverrideEnergy = ref<number | null>(null);

  // --- Getters Computados ---
  const topFocus = computed<ScoredActionDto | null>(() => 
    currentDecision.value?.topFocus || null
  );

  const alternatives = computed<ScoredActionDto[]>(() => 
    currentDecision.value?.alternatives || []
  );

  const availableMinutes = computed<number>(() => 
    currentDecision.value?.context.availableWindowMinutes || 0
  );

  const effectiveEnergy = computed<number>(() => 
    userOverrideEnergy.value ?? currentDecision.value?.context.effectiveEnergy ?? 2
  );

  const activeHardBlocker = computed(() => 
    currentDecision.value?.context.activeHardBlocker || null
  );

  // --- Ações do Motor (Actions) ---
  const fetchNow = async (timeZoneId = 'America/Sao_Paulo') => {
    isLoading.value = true;
    error.value = null;
    try {
      const data = await CompassApi.getNowDecision(timeZoneId);
      currentDecision.value = data;
    } catch (err: any) {
      error.value = 'Não foi possível consultar o motor de decisão.';
      console.error('Erro em fetchNow:', err);
    } finally {
      isLoading.value = false;
    }
  };

  // Executa a transição de foco: Conclui o Top 1 e promove a alternativa imediatamente (Cap. 6.2)
  const completeTopFocus = async () => {
    if (!topFocus.value || !currentDecision.value) return;

    const completedId = topFocus.value.id;
    const snapshotId = `snap-${currentDecision.value.generatedAt}`; // ID transacional de rastreio

    // 1. Promoção Visual Otimista Instantânea (< 16ms)
    const promotedNext = alternatives.value.length > 0 ? alternatives.value[0] : null;
    const remainingAlternatives = alternatives.value.slice(1);

    // Atualiza o estado da store local para que a animação de slide-up aconteça no DOM
    currentDecision.value = {
      ...currentDecision.value,
      topFocus: promotedNext,
      alternatives: remainingAlternatives
    };

    // 2. Disparo sincronizado para a store de compromissos e telemetria
    try {
      // Conclui a tarefa no banco de dados e processa cascatas
      await commitmentsStore.updateStatus(completedId, 'COMPLETED');
      // Registra a auditoria da escolha para calibrar o algoritmo
      await CompassApi.registerChoice(snapshotId, completedId);
    } catch (err) {
      console.error('Erro ao sincronizar conclusão do Top Focus. Sincronizando motor...');
      // Em caso de erro, recarrega a visão do motor para restaurar a consistência
      await fetchNow();
    }
  };

  const postponeTopFocus = async () => {
    if (!topFocus.value || !currentDecision.value) return;

    const postponedId = topFocus.value.id;

    // 1. Otimismo: Rebaixa o Top Focus para o final das alternativas e promove a próxima
    const currentTop = topFocus.value;
    const promotedNext = alternatives.value.length > 0 ? alternatives.value[0] : null;
    const newAlternatives = [...alternatives.value.slice(1), currentTop].filter(Boolean) as ScoredActionDto[];

    currentDecision.value = {
      ...currentDecision.value,
      topFocus: promotedNext,
      alternatives: newAlternatives
    };

    // 2. Atualiza no backend (Adiamento incrementa o PostponedCount e gera penalidade P_atrito)
    try {
      await commitmentsStore.updateStatus(postponedId, 'PENDING');
    } catch (err) {
      await fetchNow();
    }
  };

  const setEnergyOverride = (level: number) => {
    userOverrideEnergy.value = level;
    // Opcional: Re-consultar o motor passando a nova energia
    fetchNow();
  };

  return {
    currentDecision,
    isLoading,
    error,
    userOverrideEnergy,
    topFocus,
    alternatives,
    availableMinutes,
    effectiveEnergy,
    activeHardBlocker,
    fetchNow,
    completeTopFocus,
    postponeTopFocus,
    setEnergyOverride
  };
});