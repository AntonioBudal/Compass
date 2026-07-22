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
  <header class="h-14 px-6 border-b border-borderbase bg-app/80 backdrop-blur-md flex items-center justify-between gap-4 flex-shrink-0 z-10 select-none">
    <!-- Lado Esquerdo: Trilha de Navegação (Breadcrumb) -->
    <div class="flex items-center gap-2 text-sm">
      <span class="text-content-muted">Execução</span>
      <span class="text-content-muted">/</span>
      <h1 class="font-semibold text-content tracking-tight">{{ pageTitle }}</h1>
    </div>

    <!-- Centro: O Telemetrador de Janela Livre (Time Window Monitor - Cap. 4.4) -->
    <div class="hidden md:flex items-center gap-2 px-3 py-1 rounded-full bg-surface border border-borderbase text-xs font-mono text-content-muted">
      <span class="w-2 h-2 rounded-full bg-status-success-text animate-pulse" />
      <span>Janela Disponível: <strong class="text-content font-semibold">{{ availableMinutes }}m</strong></span>
      
      <template v-if="nextBlocker">
        <span class="text-content-muted">•</span>
        <span class="text-content-muted truncate max-w-[200px]">Próximo: {{ nextBlocker.title }}</span>
      </template>
    </div>

    <!-- Lado Direito: Controles de Foco e Ações Rápidas -->
    <div class="flex items-center gap-2">
      <button 
        type="button"
        class="inline-flex items-center gap-2 px-2.5 py-1.5 rounded-tactic bg-surface hover:bg-surface-hover border border-borderbase text-xs font-medium text-content-muted hover:text-content transition-colors cursor-pointer"
        title="Novo Compromisso Rápido (Tecla C)"
      >
        <span>+ Novo</span>
        <kbd class="px-1 text-[10px] font-mono bg-app rounded border border-borderbase text-content-muted">C</kbd>
      </button>
    </div>
  </header>
</template>