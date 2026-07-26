<script setup lang="ts">
import { computed } from 'vue';
import { type ScoredActionDto, useDecisionStore } from '@/stores/decisionStore';
import { Activity, Clock, Zap, Target, ShieldAlert } from 'lucide-vue-next';

const props = defineProps<{
  action: ScoredActionDto;
}>();

const decisionStore = useDecisionStore();
const profile = computed(() => decisionStore.adaptiveProfile);

// Derivação determinística visual dos 4 vetores da pontuação (em escala de 0 a 100)
const breakdown = computed(() => {
  const isHabit = props.action.type === 'HABIT';
  const hasProject = Boolean(props.action.projectName);
  const totalScore = props.action.scorePercentage || 50;

  // 1. Urgência Temporal (Baseada no tipo e se é hábito diário)
  const urgency = isHabit ? 85 : Math.min(100, Math.round(totalScore * 1.1));

  // 2. Match de Energia (Cruza a energia da tarefa com o viés do turno atual)
  const energyMatch = Math.min(100, Math.round(
    (props.action.energyRequired === decisionStore.currentEnergy ? 90 : 60) * profile.value.morningEnergyBias
  ));

  // 3. Alinhamento Estratégico (Projetos ativos e metas elevam o escore)
  const strategy = hasProject ? 95 : (props.action.type === 'EVENT' ? 90 : 50);

  // 4. Eficiência de Acurácia / EAI (Penaliza levemente estimativas muito longas ou mal calibradas)
  const eaiEfficiency = props.action.wasTimeAdjustedByEai ? 100 : 80;

  return [
    { label: 'Urgência Temporal', value: urgency, icon: Clock, desc: 'Decaimento até o prazo final' },
    { label: 'Match de Energia', value: energyMatch, icon: Zap, desc: 'Compatibilidade cronobiológica' },
    { label: 'Alinhamento Estratégico', value: strategy, icon: Target, desc: 'Vinculação a projetos ativos' },
    { label: 'Calibração EAI', value: eaiEfficiency, icon: ShieldAlert, desc: 'Acurácia histórica de estimativa' }
  ];
});
</script>

<template>
  <div class="w-full bg-app/60 border border-borderbase rounded-lg p-3 font-mono select-none animate-fade-in gpu-accelerated">
    <!-- Cabeçalho Técnico -->
    <div class="flex items-center justify-between text-[11px] text-content-muted pb-2 border-b border-borderbase/40 mb-2.5">
      <span class="flex items-center gap-1.5 font-bold text-content uppercase tracking-wider">
        <Activity class="w-3.5 h-3.5 text-content" />
        <span>Matriz de Pontuação — Vetores Táticos</span>
      </span>
      <span class="font-sans text-[10px] opacity-75">
        Escore Final: <strong class="text-content font-mono">{{ action.scorePercentage }}%</strong>
      </span>
    </div>

    <!-- Grid de Barras de Explicabilidade (CSS Grid / Zero Layout Shift) -->
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-x-4 gap-y-2.5">
      <div 
        v-for="item in breakdown" 
        :key="item.label"
        class="flex flex-col gap-1"
        :title="item.desc"
      >
        <div class="flex items-center justify-between text-[10px] text-content-muted">
          <span class="flex items-center gap-1 font-sans truncate">
            <component :is="item.icon" class="w-3 h-3 flex-shrink-0 text-content" />
            <span class="truncate">{{ item.label }}</span>
          </span>
          <span class="font-bold text-content font-mono">{{ item.value }}%</span>
        </div>

        <!-- Barra Monocromática de Alta Densidade -->
        <div class="w-full h-1.5 bg-surface-hover rounded-full overflow-hidden border border-borderbase/30">
          <div 
            class="h-full bg-content transition-all duration-300 ease-out rounded-full"
            :style="{ width: `${item.value}%` }"
          ></div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.gpu-accelerated {
  will-change: transform, opacity;
  transform: translateZ(0);
}
.animate-fade-in {
  animation: fadeIn 150ms cubic-bezier(0.16, 1, 0.3, 1) forwards;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(2px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>