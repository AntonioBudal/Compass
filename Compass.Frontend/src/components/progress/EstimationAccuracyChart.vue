<script setup lang="ts">
import { computed } from 'vue';
import { useProgressStore } from '@/stores/progressStore';
import { BarChart2 } from 'lucide-vue-next';

const progressStore = useProgressStore();
const series = computed(() => progressStore.mergedDailySeries);

// Descobre o teto máximo da escala para normalizar as barras entre 0% e 100% da altura do container
const maxMinutes = computed(() => {
  if (!series.value || series.value.length === 0) return 60;
  const max = Math.max(...series.value.map(s => Math.max(s.estimatedMinutes, s.deepWorkMinutes)));
  return max > 0 ? max : 60;
});
</script>

<template>
  <div class="p-5 rounded-xl bg-surface border border-borderbase space-y-4 font-mono select-none">
    <!-- Cabeçalho -->
    <div class="flex items-center justify-between border-b border-borderbase pb-3">
      <div class="flex items-center gap-2 text-sm font-semibold text-content uppercase tracking-wider">
        <BarChart2 class="w-4 h-4 text-content-muted" />
        <span>Série de Execução Diária</span>
      </div>
      <div class="flex items-center gap-4 text-[11px] text-content-muted">
        <span class="flex items-center gap-1.5 font-sans">
          <span class="w-2.5 h-2.5 rounded-sm bg-content inline-block" />
          <span>Volume Útil (m)</span>
        </span>
        <span class="flex items-center gap-1.5 font-sans">
          <span class="w-2.5 h-2.5 rounded-sm bg-surface-active border border-borderfocus inline-block" />
          <span>Deep Work (!3)</span>
        </span>
      </div>
    </div>

    <!-- Container do Gráfico SVG/CSS -->
    <div v-if="series.length > 0" class="h-44 flex items-end gap-2 pt-6 pb-2 px-1 border-b border-borderbase/60 overflow-x-auto relative">
      <div
        v-for="slice in series"
        :key="slice.dateIso"
        class="flex-1 min-w-[28px] flex flex-col items-center gap-1 h-full justify-end group relative"
      >
        <!-- Tooltip Hover Flutuante -->
        <div class="absolute -top-12 z-10 hidden group-hover:flex flex-col bg-app border border-borderfocus p-1.5 rounded shadow-xl text-[10px] whitespace-nowrap pointer-events-none">
          <span class="font-bold text-content border-b border-borderbase pb-0.5">{{ slice.dateIso }}</span>
          <span class="text-content-muted mt-0.5">Útil: <strong class="text-content">{{ slice.estimatedMinutes }}m</strong></span>
          <span class="text-content-muted">Deep: <strong class="text-content">{{ slice.deepWorkMinutes }}m</strong></span>
          <span v-if="slice.postponedCount > 0" class="text-status-danger-text">Adiadiados: {{ slice.postponedCount }}</span>
        </div>

        <!-- Colunas de Barras -->
        <div class="w-full flex items-end justify-center gap-0.5 h-full px-0.5">
          <!-- Barra de Volume Útil -->
          <div 
            class="w-full bg-content rounded-t-sm transition-all duration-tactic min-h-[4px]"
            :style="{ height: `${Math.max(4, (slice.estimatedMinutes / maxMinutes) * 100)}%` }"
          />
          <!-- Barra de Deep Work -->
          <div 
            class="w-full bg-surface-active border-t border-x border-borderfocus rounded-t-sm transition-all duration-tactic min-h-[4px]"
            :style="{ height: `${Math.max(4, (slice.deepWorkMinutes / maxMinutes) * 100)}%` }"
          />
        </div>

        <!-- Eixo X (Datas) -->
        <span class="text-[9px] text-content-muted truncate max-w-full">
          {{ slice.dateIso.slice(5) }}
        </span>
      </div>
    </div>

    <!-- Estado Vazio -->
    <div v-else class="h-44 flex flex-col items-center justify-center text-content-muted text-xs border border-dashed border-borderbase rounded-lg bg-app/30">
      <span>Nenhum histórico transacional disponível para este período.</span>
    </div>
  </div>
</template>