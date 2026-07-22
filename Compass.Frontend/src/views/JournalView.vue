<script setup lang="ts">
import { onMounted } from 'vue';
import { useJournalStore } from '@/stores/journalStore';
import { useCommitmentsStore } from '@/stores/commitmentsStore';
import EnergyIndicator from '@/components/core/EnergyIndicator.vue';
import { 
  FileText, Power, Clock, CheckCircle2, 
  Filter, Calendar, Folder 
} from 'lucide-vue-next';

const journalStore = useJournalStore();
const commitmentsStore = useCommitmentsStore();

onMounted(async () => {
  await commitmentsStore.fetchAllActive();
});
</script>

<template>
  <div class="max-w-4xl mx-auto space-y-8 select-none pb-12">
    
    <!-- Cabeçalho de Auditoria e Botão de Shutdown -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-4 border-b border-borderbase">
      <div>
        <h1 class="text-2xl font-semibold text-content tracking-tight flex items-center gap-2.5">
          <FileText class="w-6 h-6 text-content-muted" />
          <span>Auditoria & Fechamento</span>
        </h1>
        <p class="text-sm text-content-muted mt-1">
          Log imutável de execução de turno e ponto de partida para o ritual de desligamento diário.
        </p>
      </div>

      <button 
        @click="journalStore.startShutdown"
        class="inline-flex items-center justify-center gap-2 px-4 py-2 rounded-tactic bg-content hover:bg-content-accent text-content-invert text-xs font-semibold shadow-sm transition-all cursor-pointer flex-shrink-0"
      >
        <Power class="w-3.5 h-3.5" />
        <span>Iniciar Fechamento de Turno</span>
      </button>
    </div>

    <!-- Métrica Inline de Turno (Substituindo o Dashboard) -->
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
          :class="journalStore.activeTab === 'today' ? 'bg-surface-active text-content border border-borderfocus' : 'text-content-muted hover:text-content'"
        >
          [Concluídos Hoje]
        </button>
        <button 
          @click="journalStore.activeTab = 'week'"
          class="px-3 py-1 rounded text-xs font-mono transition-colors cursor-pointer"
          :class="journalStore.activeTab === 'week' ? 'bg-surface-active text-content border border-borderfocus' : 'text-content-muted hover:text-content'"
        >
          [Registro da Semana]
        </button>
      </div>

      <div class="text-[11px] font-mono text-content-muted flex items-center gap-1.5">
        <Filter class="w-3.5 h-3.5" />
        <span>Modo: Somente-Leitura</span>
      </div>
    </div>

    <!-- LISTA CRONOLÓGICA DE AUDITORIA (Estilo Git Log) -->
    <div v-if="journalStore.completedToday.length > 0" class="space-y-2">
      <div 
        v-for="item in journalStore.completedToday" 
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
                <Folder class="w-3 h-3" /> [{ item.projectName }]
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

    <!-- Estado Vazio -->
    <div v-else class="p-12 rounded-xl border border-dashed border-borderbase bg-app/40 text-center space-y-3">
      <CheckCircle2 class="w-8 h-8 text-content-muted mx-auto" />
      <div class="space-y-1">
        <h3 class="text-sm font-semibold text-content">Nenhum registro de conclusão audível no período</h3>
        <p class="text-xs text-content-muted max-w-sm mx-auto">
          As tarefas e hábitos concluídos durante a execução do motor serão escritas sequencialmente neste log.
        </p>
      </div>
    </div>

  </div>
</template>