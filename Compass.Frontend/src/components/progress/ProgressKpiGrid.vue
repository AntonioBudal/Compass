<script setup lang="ts">
import { computed } from 'vue';
import { useProgressStore } from '@/stores/progressStore';
import { CheckCircle2, Clock, Zap, AlertTriangle, ShieldAlert } from 'lucide-vue-next';

const progressStore = useProgressStore();
const overview = computed(() => progressStore.mergedOverview);

// Formatação visual da acurácia com base em margens operacionais seguras
const accuracyStatus = computed(() => {
  if (!overview.value) return { label: 'N/A', class: 'text-content-muted' };
  const eai = overview.value.estimationAccuracyIndex;
  
  if (overview.value.hasImputedAccuracyData && eai === 1.0) {
    return { label: 'ESTIMADO (RAM)', class: 'text-content-muted font-normal' };
  }
  if (eai >= 0.9 && eai <= 1.1) {
    return { label: 'PRECISÃO ALTA', class: 'text-status-success-text font-bold' };
  }
  if (eai > 1.1) {
    return { label: `+${Math.round((eai - 1) * 100)}% SUBESTIMADO`, class: 'text-status-danger-text font-bold' };
  }
  return { label: `${Math.round((1 - eai) * 100)}% SUPERESTIMADO`, class: 'text-content font-medium' };
});
</script>

<template>
  <div v-if="overview" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 font-mono select-none">
    
    <!-- 1. KPI: Taxa de Execução Líquida -->
    <div class="p-4 rounded-xl bg-surface border border-borderbase hover:border-borderfocus transition-all flex flex-col justify-between h-28 relative overflow-hidden group">
      <div class="flex items-center justify-between text-xs text-content-muted uppercase tracking-wider">
        <span>Eficácia de Turno</span>
        <CheckCircle2 class="w-4 h-4 text-content-muted group-hover:text-content transition-colors" />
      </div>
      <div>
        <div class="text-2xl font-sans font-bold text-content tracking-tight">
          {{ overview.completionRatePercentage }}%
        </div>
        <div class="text-[11px] text-content-muted mt-0.5">
          {{ overview.totalCompleted }} concluídos / {{ overview.totalPlanned }} planejados
        </div>
      </div>
      <!-- Barra de progresso de fundo epêmera -->
      <div 
        class="absolute bottom-0 left-0 h-1 bg-content transition-all duration-tactic"
        :style="{ width: `${overview.completionRatePercentage}%` }"
      />
    </div>

    <!-- 2. KPI: Índice de Acurácia de Estimativa (EAI) -->
    <div class="p-4 rounded-xl bg-surface border border-borderbase hover:border-borderfocus transition-all flex flex-col justify-between h-28 relative group">
      <div class="flex items-center justify-between text-xs text-content-muted uppercase tracking-wider">
        <span class="flex items-center gap-1.5">
          <span>Acurácia (EAI)</span>
          <span 
            v-if="overview.hasImputedAccuracyData" 
            title="Dados imputados em RAM por falta de medição via cronômetro"
            class="cursor-help"
          >
            <AlertTriangle class="w-3.5 h-3.5 text-content-accent" />
          </span>
        </span>
        <Clock class="w-4 h-4 text-content-muted group-hover:text-content transition-colors" />
      </div>
      <div>
        <div class="flex items-baseline gap-2">
          <span class="text-2xl font-sans font-bold text-content tracking-tight">
            {{ overview.estimationAccuracyIndex }}x
          </span>
          <span class="text-[10px]" :class="accuracyStatus.class">
            [{{ accuracyStatus.label }}]
          </span>
        </div>
        <div class="text-[11px] text-content-muted mt-0.5 truncate">
          {{ overview.totalUsefulMinutes }} min de volume útil
        </div>
      </div>
    </div>

    <!-- 3. KPI: Deep Work Liquid Volume -->
    <div class="p-4 rounded-xl bg-surface border border-borderbase hover:border-borderfocus transition-all flex flex-col justify-between h-28 relative group">
      <div class="flex items-center justify-between text-xs text-content-muted uppercase tracking-wider">
        <span>Foco Profundo (!3)</span>
        <Zap class="w-4 h-4 text-content-muted group-hover:text-content transition-colors" />
      </div>
      <div>
        <div class="text-2xl font-sans font-bold text-content tracking-tight">
          {{ overview.totalDeepWorkMinutes }} <span class="text-sm font-normal text-content-muted">min</span>
        </div>
        <div class="text-[11px] text-content-muted mt-0.5">
          {{ overview.totalUsefulMinutes > 0 ? Math.round((overview.totalDeepWorkMinutes / overview.totalUsefulMinutes) * 100) : 0 }}% do tempo total útil
        </div>
      </div>
    </div>

    <!-- 4. KPI: Taxa de Atrito e Procrastinação -->
    <div class="p-4 rounded-xl bg-surface border border-borderbase hover:border-borderfocus transition-all flex flex-col justify-between h-28 relative group">
      <div class="flex items-center justify-between text-xs text-content-muted uppercase tracking-wider">
        <span>Atrito Operacional</span>
        <ShieldAlert class="w-4 h-4 text-content-muted group-hover:text-content transition-colors" />
      </div>
      <div>
        <div class="text-2xl font-sans font-bold text-content tracking-tight">
          {{ overview.totalPostponements }} <span class="text-sm font-normal text-content-muted">adiamentos</span>
        </div>
        <div class="text-[11px] text-content-muted mt-0.5">
          Penalidade acumulada no Now Engine
        </div>
      </div>
    </div>

  </div>

  <!-- Estado de Carregamento (Skeleton Zero Layout-Shift) -->
  <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 animate-pulse">
    <div v-for="n in 4" :key="n" class="h-28 rounded-xl bg-surface border border-borderbase p-4 flex flex-col justify-between">
      <div class="w-24 h-3 bg-surface-active rounded" />
      <div class="space-y-2">
        <div class="w-16 h-6 bg-surface-active rounded" />
        <div class="w-32 h-3 bg-surface-active rounded" />
      </div>
    </div>
  </div>
</template>