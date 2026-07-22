<script setup lang="ts">
import { useDecisionStore } from '@/stores/decisionStore';
import type { ScoredActionDto } from '@/types/index';
import EnergyIndicator from './EnergyIndicator.vue';
import { Check, Clock, Zap, Calculator } from 'lucide-vue-next';

const props = defineProps<{
  item: ScoredActionDto;
}>();

const store = useDecisionStore();

const handleComplete = () => store.completeTopFocus();
const handlePostpone = () => store.postponeTopFocus();
</script>

<template>
  <div class="p-6 rounded-xl border border-borderbase bg-surface shadow-xl relative overflow-visible transition-all duration-tactic ease-snap-out gpu-accelerated animate-promote">
    <!-- Banner de Explicabilidade Oculto -->
    <div class="mb-4">
      <span class="inline-flex items-center gap-1.5 text-xs font-mono text-content-muted bg-surface-active border border-borderbase px-2.5 py-1 rounded-md">
        <Zap class="w-3.5 h-3.5 text-content-muted" />
        {{ item.reason }}
      </span>
    </div>

    <h2 class="text-xl font-semibold text-content tracking-tight leading-snug mb-4">
      {{ item.title }}
    </h2>

    <div class="flex flex-wrap items-center gap-3 text-xs font-mono text-content-muted mb-6 pb-4 border-b border-borderbase">
      <span v-if="item.projectName" class="text-content-muted font-sans font-medium">
        [{ item.projectName }]
      </span>

      <span class="flex items-center gap-1 bg-app px-2 py-0.5 rounded border border-borderbase">
        <Clock class="w-3.5 h-3.5 text-content-muted" /> {{ item.estimatedDurationMinutes }} min
      </span>

      <EnergyIndicator :level="item.energyRequired || 2" />

      <!-- Tooltip de Explicabilidade Matemática (Cap. 7.6) -->
      <div class="ml-auto relative group">
        <span class="flex items-center gap-1.5 text-content-muted font-semibold cursor-help border-b border-dashed border-borderfocus pb-0.5 transition-colors group-hover:text-content">
          <Calculator class="w-3.5 h-3.5" /> Score: {{ item.scorePercentage }}%
        </span>
        
        <!-- Dropdown do Tooltip -->
        <div class="absolute bottom-full right-0 mb-2 hidden group-hover:block w-64 p-3 bg-app border border-borderfocus rounded shadow-2xl z-20 text-xs font-mono text-content-muted opacity-0 group-hover:opacity-100 transition-opacity">
          <div class="text-content mb-2 font-sans font-semibold border-b border-borderbase pb-1">Composição do Score</div>
          <div class="space-y-1">
            <div class="flex justify-between"><span>Base Estimada:</span> <span>+50</span></div>
            <div class="flex justify-between"><span>Peso de Urgência:</span> <span>+{{ Math.floor(item.scorePercentage * 0.2) }}</span></div>
            <div class="flex justify-between text-status-success-text"><span>Match de Energia:</span> <span>+15</span></div>
            <div class="flex justify-between text-status-danger-text"><span>Penalidade (Atrito):</span> <span>-0</span></div>
          </div>
          <div class="mt-2 text-[10px] text-content-muted pt-1 border-t border-borderbase">Now Engine v1.0</div>
        </div>
      </div>
    </div>

    <!-- Botões de Ação (Aparência Industrial / Monocromática) -->
    <div class="flex items-center justify-between gap-4">
      <button 
        type="button"
        @click="handleComplete"
        class="inline-flex items-center justify-center gap-3 px-4 py-2 bg-content hover:bg-content-accent active:scale-[0.99] text-content-invert text-sm font-semibold rounded-tactic shadow-sm transition-all duration-fast ease-snap-out focus-visible:ring-2 focus-visible:ring-borderfocus focus-visible:ring-offset-2 focus-visible:ring-offset-app flex-1 sm:flex-initial sm:min-w-[200px] cursor-pointer"
      >
        <Check class="w-4 h-4 stroke-[2.5]" /> Concluir Agora
        <kbd class="px-1.5 py-0.5 text-[10px] font-mono bg-surface rounded text-content border border-borderbase">E</kbd>
      </button>

      <button 
        type="button"
        @click="handlePostpone"
        class="inline-flex items-center justify-center gap-3 px-3 py-2 bg-surface hover:bg-surface-hover text-content-muted hover:text-content text-sm font-medium rounded-tactic border border-borderbase transition-all duration-fast ease-fade-smooth focus-visible:ring-2 focus-visible:ring-borderfocus cursor-pointer"
      >
        Adiar
        <kbd class="px-1.5 py-0.5 text-[10px] font-mono bg-app rounded text-content-muted border border-borderbase">S</kbd>
      </button>
    </div>
  </div>
</template>