<script setup lang="ts">
import { computed } from 'vue';
import { useRoute } from 'vue-router';
import { useDecisionStore } from '@/stores/decisionStore';
import { Clock, Zap } from 'lucide-vue-next';

const route = useRoute();
const decisionStore = useDecisionStore();

const pageTitle = computed(() => route.meta.title || 'Compass');
const availableMinutes = computed(() => decisionStore.availableMinutes);
const nextBlocker = computed(() => decisionStore.activeHardBlocker);
</script>

<template>
  <header class="h-14 px-6 border-b border-zinc-800/80 bg-zinc-950/80 backdrop-blur-md flex items-center justify-between gap-4 flex-shrink-0 z-10 select-none">
    <!-- Lado Esquerdo: Trilha de Navegação (Breadcrumb) -->
    <div class="flex items-center gap-2 text-sm">
      <span class="text-zinc-500">Execução</span>
      <span class="text-zinc-600">/</span>
      <h1 class="font-semibold text-zinc-100 tracking-tight">{{ pageTitle }}</h1>
    </div>

    <!-- Centro: O Telemetrador de Janela Livre (Time Window Monitor - Cap. 4.4) -->
    <div class="hidden md:flex items-center gap-2 px-3 py-1 rounded-full bg-zinc-900 border border-zinc-800 text-xs font-mono text-zinc-300">
      <span class="w-2 h-2 rounded-full bg-emerald-500 animate-pulse" />
      <span>Janela Disponível: <strong class="text-white font-semibold">{{ availableMinutes }}m</strong></span>
      
      <template v-if="nextBlocker">
        <span class="text-zinc-600">•</span>
        <span class="text-zinc-400 truncate max-w-[200px]">Próximo: {{ nextBlocker.title }}</span>
      </template>
    </div>

    <!-- Lado Direito: Controles de Foco e Ações Rápidas -->
    <div class="flex items-center gap-2">
      <button 
        type="button"
        class="inline-flex items-center gap-2 px-2.5 py-1.5 rounded-tactic bg-zinc-900 hover:bg-zinc-800 border border-zinc-800 text-xs font-medium text-zinc-300 hover:text-white transition-colors"
        title="Novo Compromisso Rápido (Tecla C)"
      >
        <span>+ Novo</span>
        <kbd class="px-1 text-[10px] font-mono bg-zinc-950 rounded border border-zinc-800 text-zinc-500">C</kbd>
      </button>
    </div>
  </header>
</template>