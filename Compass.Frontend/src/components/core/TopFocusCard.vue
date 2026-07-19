<script setup lang="ts">
import { computed } from 'vue';
import { useDecisionStore } from '@/stores/decisionStore';
import type { ScoredActionDto } from '@/types/index';
import TacticBadge from './TacticBadge.vue';
import { Check, Clock, Zap, ArrowRight, CornerDownRight } from 'lucide-vue-next';

const props = defineProps<{
  item: ScoredActionDto;
}>();

const store = useDecisionStore();

const energyLabel = computed(() => {
  switch (props.item.energyRequired) {
    case 3: return '⚡⚡⚡ Energia Alta';
    case 1: return '⚡ Energia Baixa';
    case 2:
    default: return '⚡⚡ Energia Média';
  }
});

const handleComplete = () => store.completeTopFocus();
const handlePostpone = () => store.postponeTopFocus();
</script>

<template>
  <div class="p-6 rounded-xl border border-zinc-700/60 bg-gradient-to-b from-zinc-900/90 to-zinc-900/40 shadow-xl relative overflow-hidden transition-all duration-tactic">
    <!-- Banner de Explicabilidade (The Reason Banner - Cap. 3.5) -->
    <div class="mb-4">
      <span class="inline-flex items-center gap-1.5 text-xs font-mono text-indigo-400 bg-indigo-500/10 border border-indigo-500/20 px-2.5 py-1 rounded-md">
        <Zap class="w-3.5 h-3.5 text-indigo-400" />
        {{ item.reason }}
      </span>
    </div>

    <!-- Título Primário em Escala Saliência -->
    <h2 class="text-xl font-semibold text-zinc-100 tracking-tight leading-snug mb-4">
      {{ item.title }}
    </h2>

    <!-- Metadados da Tarefa em Foco -->
    <div class="flex flex-wrap items-center gap-3 text-xs font-mono text-zinc-400 mb-6 pb-4 border-b border-zinc-800">
      <span v-if="item.projectName" class="text-zinc-300 font-sans font-medium">
        📁 {{ item.projectName }}
      </span>

      <span class="flex items-center gap-1 bg-zinc-900 px-2 py-0.5 rounded border border-zinc-800">
        <Clock class="w-3.5 h-3.5 text-zinc-400" /> {{ item.estimatedDurationMinutes }} minutos
      </span>

      <span class="bg-zinc-800/80 text-zinc-300 px-2 py-0.5 rounded border border-zinc-700/50">
        {{ energyLabel }}
      </span>

      <span class="ml-auto text-indigo-400/80 font-semibold">
        Score: {{ item.scorePercentage }}%
      </span>
    </div>

    <!-- Barra de Ação Principal (Buttons Architecture - Cap. 3.1) -->
    <div class="flex items-center justify-between gap-4">
      <button 
        type="button"
        @click="handleComplete"
        class="inline-flex items-center justify-between gap-4 px-4 py-2 bg-emerald-600 hover:bg-emerald-500 active:scale-[0.99] text-white text-sm font-medium rounded-tactic shadow-sm transition-all duration-150 focus-visible:ring-2 focus-visible:ring-emerald-500 focus-visible:ring-offset-2 focus-visible:ring-offset-zinc-950 flex-1 sm:flex-initial sm:min-w-[200px]"
      >
        <span class="flex items-center gap-2">
          <Check class="w-4 h-4 stroke-[2.5]" /> Concluir Agora
        </span>
        <kbd class="px-1.5 py-0.5 text-[10px] font-mono bg-emerald-700/60 rounded text-emerald-100 border border-emerald-500/30">E</kbd>
      </button>

      <button 
        type="button"
        @click="handlePostpone"
        class="inline-flex items-center justify-between gap-3 px-3 py-2 bg-zinc-800/80 hover:bg-zinc-800 text-zinc-300 hover:text-white text-sm font-medium rounded-tactic border border-zinc-700/50 transition-all duration-150 focus-visible:ring-2 focus-visible:ring-zinc-600"
      >
        <span>Adiar</span>
        <kbd class="px-1.5 py-0.5 text-[10px] font-mono bg-zinc-900 rounded text-zinc-400 border border-zinc-700/50">S</kbd>
      </button>
    </div>
  </div>
</template>