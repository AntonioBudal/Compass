<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import { useJournalStore } from '@/stores/journalStore';
import { useCommitmentsStore } from '@/stores/commitmentsStore';
import { useProgressStore, type TimeRangeOption } from '@/stores/progressStore';

// Importação dos 4 Componentes Analíticos criados no Dia 4
import ProgressKpiGrid from '@/components/progress/ProgressKpiGrid.vue';
import EstimationAccuracyChart from '@/components/progress/EstimationAccuracyChart.vue';
import FrictionHeatmapGrid from '@/components/progress/FrictionHeatmapGrid.vue';
import ExecutionChronology from '@/components/progress/ExecutionChronology.vue';

import EnergyIndicator from '@/components/core/EnergyIndicator.vue';
import { 
  FileText, Power, Clock, CheckCircle2, 
  Filter, Folder, BarChart3, RefreshCw, WifiOff 
} from 'lucide-vue-next';

const journalStore = useJournalStore();
const commitmentsStore = useCommitmentsStore();
const progressStore = useProgressStore();

// Aba Principal de Nível IDE: 'evolution' (Painel Analítico) vs 'audit' (Log Diário & Shutdown)
const mainTab = ref<'evolution' | 'audit'>('evolution');

// Opções de filtro temporal para a telemetria
const timeRanges: { label: string; value: TimeRangeOption }[] = [
  { label: '7 DIAS', value: '7d' },
  { label: '30 DIAS', value: '30d' },
  { label: '90 DIAS', value: '90d' },
  { label: '1 ANO', value: '1y' },
];

const displayedItems = computed(() => {
  if (journalStore.activeTab === 'week') {
    return (journalStore as any).completedWeek || journalStore.completedToday;
  }
  return journalStore.completedToday;
});

const handleRangeChange = (range: TimeRangeOption) => {
  progressStore.fetchProgress(range);
};

const handleRefresh = () => {
  progressStore.fetchProgress(progressStore.selectedRange, true);
};

onMounted(async () => {
  await commitmentsStore.fetchAllActive();
  // Dispara a carga analítica em paralelo com o histórico de auditoria
  await Promise.all([
    progressStore.fetchProgress('30d'),
    typeof (journalStore as any).fetchAuditLogs === 'function' 
      ? (journalStore as any).fetchAuditLogs() 
      : Promise.resolve()
  ]);
});
</script>

<template>
  <div class="max-w-5xl mx-auto space-y-8 select-none pb-16">
    
    <!-- Cabeçalho Global & Botão de Shutdown -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-4 border-b border-borderbase">
      <div>
        <h1 class="text-2xl font-semibold text-content tracking-tight flex items-center gap-2.5">
          <BarChart3 v-if="mainTab === 'evolution'" class="w-6 h-6 text-content-muted" />
          <FileText v-else class="w-6 h-6 text-content-muted" />
          <span>{{ mainTab === 'evolution' ? 'Evolução & Telemetria' : 'Auditoria & Fechamento' }}</span>
        </h1>
        <p class="text-sm text-content-muted mt-1">
          {{ mainTab === 'evolution' 
            ? 'Motor de inteligência analítica combinando histórico imutável (DB) com execução em tempo real (RAM).' 
            : 'Log imutável de execução do turno e ponto de partida para o ritual de desligamento diário.' 
          }}
        </p>
      </div>

      <div class="flex items-center gap-3">
        <!-- Indicador Tático de Modo Offline / Cache -->
        <span 
          v-if="progressStore.isServingFromCache" 
          title="Sem conexão com a API. Exibindo última série salva em disco."
          class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded text-[11px] font-mono bg-status-warning-bg text-status-warning-text border border-status-warning-border"
        >
          <WifiOff class="w-3.5 h-3.5" />
          <span>OFFLINE CACHE</span>
        </span>

        <button 
          @click="journalStore.startShutdown"
          class="inline-flex items-center justify-center gap-2 px-4 py-2 rounded-tactic bg-content hover:opacity-90 text-content-invert text-xs font-semibold shadow-sm transition-all cursor-pointer flex-shrink-0"
        >
          <Power class="w-3.5 h-3.5" />
          <span>Iniciar Fechamento</span>
        </button>
      </div>
    </div>

    <!-- ABAS SUPERIORES DE NÍVEL IDE -->
    <div class="flex items-center justify-between border-b border-borderbase pb-3">
      <div class="flex items-center gap-2">
        <button 
          @click="mainTab = 'evolution'"
          class="px-3.5 py-1.5 rounded text-xs font-mono transition-all cursor-pointer flex items-center gap-2"
          :class="mainTab === 'evolution' ? 'bg-surface-active text-content border border-borderfocus font-bold shadow-sm' : 'text-content-muted hover:text-content'"
        >
          <BarChart3 class="w-3.5 h-3.5" />
          <span>[PAINEL DE EVOLUÇÃO]</span>
        </button>
        
        <button 
          @click="mainTab = 'audit'"
          class="px-3.5 py-1.5 rounded text-xs font-mono transition-all cursor-pointer flex items-center gap-2"
          :class="mainTab === 'audit' ? 'bg-surface-active text-content border border-borderfocus font-bold shadow-sm' : 'text-content-muted hover:text-content'"
        >
          <FileText class="w-3.5 h-3.5" />
          <span>[LOG DE AUDITORIA]</span>
        </button>
      </div>

      <!-- Seletor de Período Temporal (Apenas visível no Painel de Evolução) -->
      <div v-if="mainTab === 'evolution'" class="flex items-center gap-1.5">
        <button
          v-for="range in timeRanges"
          :key="range.value"
          @click="handleRangeChange(range.value)"
          class="px-2.5 py-1 rounded text-[11px] font-mono transition-colors cursor-pointer border"
          :class="progressStore.selectedRange === range.value ? 'bg-content text-content-invert border-content font-bold' : 'bg-surface text-content-muted border-borderbase hover:text-content'"
        >
          {{ range.label }}
        </button>

        <button 
          @click="handleRefresh" 
          :disabled="progressStore.isLoading"
          class="p-1.5 rounded bg-surface hover:bg-surface-hover border border-borderbase text-content-muted hover:text-content transition-colors ml-1 cursor-pointer disabled:opacity-50"
          title="Forçar revalidação (ETag)"
        >
          <RefreshCw class="w-3.5 h-3.5" :class="{ 'animate-spin': progressStore.isLoading }" />
        </button>
      </div>
    </div>

    <!-- ================================================================= -->
    <!-- VISÃO 1: PAINEL DE EVOLUÇÃO (PROGRESS ENGINE - SEMANA 1)          -->
    <!-- ================================================================= -->
    <div v-if="mainTab === 'evolution'" class="space-y-6 animate-fade-in">
      
      <!-- 1. Grade de KPIs (Fundidos RAM + DB) -->
      <section aria-label="Indicadores Chave de Performance">
        <ProgressKpiGrid />
      </section>

      <!-- 2. Gráfico de Linhas/Barras de Execução vs Foco Profundo -->
      <section aria-label="Gráfico de Acurácia e Volume Diário">
        <EstimationAccuracyChart />
      </section>

      <!-- 3. Grade Dividida: Matriz de Procrastinação + Cronobiologia CLI -->
      <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
        <section aria-label="Heatmap de Procrastinação" class="lg:col-span-7">
          <FrictionHeatmapGrid />
        </section>
        
        <section aria-label="Cronobiologia de Foco" class="lg:col-span-5">
          <ExecutionChronology />
        </section>
      </div>

    </div>

    <!-- ================================================================= -->
    <!-- VISÃO 2: LOG DE AUDITORIA & SHUTDOWN (TRANSACIONAL INTACo)      -->
    <!-- ================================================================= -->
    <div v-else class="space-y-6 animate-fade-in">
      
      <!-- Métrica Inline de Turno -->
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 font-mono text-xs">
        <div class="p-4 rounded-lg bg-surface border border-borderbase flex items-center justify-between">
          <span class="text-content-muted">Concluídos Hoje:</span>
          <span class="text-content font-bold text-base">{{ journalStore.auditMetrics.totalCompleted }} itens</span>
        </div>
        <div class="p-4 rounded-lg bg-surface border border-borderbase flex items-center justify-between">
          <span class="text-content-muted">Deep Work Auditado:</span>
          <span class="text-content font-bold text-base">{{ journalStore.auditMetrics.deepWorkMinutes }} min</span>
        </div>
        <div class="p-4 rounded-lg bg-surface border border-borderbase flex items-center justify-between">
          <span class="text-content-muted">Tempo Total Útil:</span>
          <span class="text-content font-bold text-base">{{ journalStore.auditMetrics.totalMinutes }} min</span>
        </div>
      </div>

      <!-- Filtros em Abas Monocromáticas -->
      <div class="flex items-center justify-between border-b border-borderbase pb-3">
        <div class="flex items-center gap-2">
          <button 
            @click="journalStore.activeTab = 'today'"
            class="px-3 py-1 rounded text-xs font-mono transition-colors cursor-pointer"
            :class="journalStore.activeTab === 'today' ? 'bg-surface-active text-content border border-borderfocus font-bold' : 'text-content-muted hover:text-content'"
          >
            [Concluídos Hoje]
          </button>
          <button 
            @click="journalStore.activeTab = 'week'"
            class="px-3 py-1 rounded text-xs font-mono transition-colors cursor-pointer"
            :class="journalStore.activeTab === 'week' ? 'bg-surface-active text-content border border-borderfocus font-bold' : 'text-content-muted hover:text-content'"
          >
            [Registro da Semana]
          </button>
        </div>

        <div class="text-[11px] font-mono text-content-muted flex items-center gap-1.5">
          <Filter class="w-3.5 h-3.5" />
          <span>Modo: Somente-Leitura</span>
        </div>
      </div>

      <!-- LISTA CRONOLÓGICA DE AUDITORIA -->
      <div v-if="displayedItems && displayedItems.length > 0" class="space-y-2">
        <div 
          v-for="item in displayedItems" 
          :key="item.id"
          class="p-3.5 rounded-lg border border-borderbase bg-surface hover:bg-surface-hover transition-colors flex items-center justify-between gap-4 font-mono"
        >
          <div class="flex items-center gap-3 min-w-0 flex-1">
            <CheckCircle2 class="w-4 h-4 text-content-muted flex-shrink-0" />
            
            <div class="truncate">
              <span class="text-sm font-sans font-medium text-content line-through block truncate">
                {{ item.title }}
              </span>
              <div class="flex items-center gap-2 text-[11px] text-content-muted mt-0.5">
                <span v-if="item.projectName" class="flex items-center gap-1">
                  <Folder class="w-3 h-3" /> [{{ item.projectName }}]
                </span>
                <span class="flex items-center gap-1">
                  <Clock class="w-3 h-3" /> {{ item.estimatedDurationMinutes }}m
                </span>
                <span>| Status: COMPLETED</span>
              </div>
            </div>
          </div>

          <div class="flex items-center gap-3 flex-shrink-0">
            <EnergyIndicator :level="item.energyRequired || 2" />
            <span class="text-xs text-content-muted font-mono hidden sm:inline-block">
              #{{ item.id.slice(-6) }}
            </span>
          </div>
        </div>
      </div>

      <!-- Estado Vazio da Auditoria -->
      <div v-else class="p-12 rounded-xl border border-dashed border-borderbase bg-app/40 text-center space-y-3">
        <CheckCircle2 class="w-8 h-8 text-content-muted mx-auto" />
        <div class="space-y-1">
          <h3 class="text-sm font-semibold text-content">
            {{ journalStore.activeTab === 'today' ? 'Nenhum registro auditável hoje' : 'Nenhum registro auditável na semana' }}
          </h3>
          <p class="text-xs text-content-muted max-w-sm mx-auto">
            As tarefas e hábitos concluídos durante a execução do motor serão escritas sequencialmente neste log.
          </p>
        </div>
      </div>

    </div>

  </div>
</template>

<style scoped>
.animate-fade-in {
  animation: fadeIn 180ms cubic-bezier(0.16, 1, 0.3, 1) forwards;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(2px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>