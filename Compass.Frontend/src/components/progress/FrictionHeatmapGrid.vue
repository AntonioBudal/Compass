<script setup lang="ts">
import { computed } from 'vue';
import { useProgressStore } from '@/stores/progressStore';
import { AlertOctagon } from 'lucide-vue-next';

const progressStore = useProgressStore();
const heatmap = computed(() => progressStore.rawHeatmap);

const types = ['TASK', 'HABIT', 'EVENT', 'NOTE'];
const energies = [1, 2, 3];

function getBucket(type: string, energy: number) {
  return heatmap.value.find(h => h.commitmentType === type && h.energyRequired === energy);
}

// Escala cromática de opacidade baseada na porcentagem de adiamento
function getHeatmapStyle(rate: number, total: number) {
  if (total === 0) return 'bg-app/40 border-borderbase text-content-muted/40';
  if (rate === 0) return 'bg-surface border-borderbase text-content font-normal';
  if (rate <= 25) return 'bg-surface-active border-borderbase text-content font-medium';
  if (rate <= 50) return 'bg-surface-active border-borderfocus text-content font-bold shadow-sm';
  return 'bg-status-danger-bg border-status-danger-border text-status-danger-text font-bold animate-pulse';
}
</script>

<template>
  <div class="p-5 rounded-xl bg-surface border border-borderbase space-y-4 font-mono select-none">
    <!-- Cabeçalho -->
    <div class="flex items-center justify-between border-b border-borderbase pb-3">
      <div class="flex items-center gap-2 text-sm font-semibold text-content uppercase tracking-wider">
        <AlertOctagon class="w-4 h-4 text-content-muted" />
        <span>Heatmap de Procrastinação & Atrito</span>
      </div>
      <span class="text-[11px] text-content-muted">Taxa de Adiamentos por Nível de Energia</span>
    </div>

    <!-- Grade Matricial 4x3 -->
    <div class="space-y-3 pt-1">
      <div class="grid grid-cols-4 gap-2 text-[10px] text-content-muted uppercase tracking-wider text-center">
        <span>Arquétipo</span>
        <span>■□□ MAINT (!1)</span>
        <span>■■□ OPER (!2)</span>
        <span>■■■ DEEP (!3)</span>
      </div>

      <div v-for="type in types" :key="type" class="grid grid-cols-4 gap-2 items-center">
        <!-- Rótulo Linha -->
        <div class="text-xs font-bold text-content bg-app/60 p-2.5 rounded border border-borderbase text-center">
          {{ type }}
        </div>

        <!-- Células de Energia -->
        <div
          v-for="energy in energies"
          :key="energy"
          class="p-2.5 rounded border transition-all flex flex-col items-center justify-center text-center min-h-[52px]"
          :class="getHeatmapStyle(getBucket(type, energy)?.postponementRatePercentage || 0, getBucket(type, energy)?.totalCount || 0)"
        >
          <template v-if="getBucket(type, energy) && getBucket(type, energy)!.totalCount > 0">
            <span class="text-sm tracking-tight">{{ getBucket(type, energy)!.postponementRatePercentage }}%</span>
            <span class="text-[9px] opacity-75 mt-0.5">
              {{ getBucket(type, energy)!.postponedCount }}/{{ getBucket(type, energy)!.totalCount }} adiados
            </span>
          </template>
          <span v-else class="text-[10px] opacity-40">-</span>
        </div>
      </div>
    </div>
  </div>
</template>