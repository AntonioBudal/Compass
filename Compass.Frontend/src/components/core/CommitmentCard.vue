<script setup lang="ts">
import { computed } from 'vue';
import { type ScoredActionDto } from '@/stores/decisionStore';
import { Clock, Zap, Folder, ArrowRight, ShieldAlert } from 'lucide-vue-next';

const props = defineProps<{
  action: ScoredActionDto;
}>();

const emit = defineEmits<{
  (e: 'select', id: string): void;
}>();

const hasEaiAdjustment = computed(() => props.action.wasTimeAdjustedByEai);
</script>

<template>
  <div 
    @click="emit('select', action.commitmentId)"
    class="w-full bg-surface border border-borderbase hover:border-borderfocus rounded-lg p-3.5 transition-all duration-150 flex flex-col justify-between gap-2.5 font-mono cursor-pointer group select-none"
  >
    <!-- Topo: Título e Projeto -->
    <div class="flex items-start justify-between gap-3">
      <div class="min-w-0 flex-1">
        <div v-if="action.projectName" class="text-[10px] font-bold text-content-accent uppercase tracking-wider mb-0.5 flex items-center gap-1">
          <Folder class="w-3 h-3 flex-shrink-0" />
          <span class="truncate">#{{ action.projectName }}</span>
        </div>
        <h3 class="text-sm font-sans font-semibold text-content group-hover:underline truncate">
          {{ action.title }}
        </h3>
      </div>
      <span class="text-xs font-bold text-content bg-surface-hover px-1.5 py-0.5 rounded border border-borderbase flex-shrink-0">
        {{ action.scorePercentage }}%
      </span>
    </div>

    <!-- Base: Metadados Táticos e Badges Algorítmicos -->
    <div class="flex items-center justify-between text-xs text-content-muted pt-2 border-t border-borderbase/40">
      <div class="flex items-center gap-3">
        <!-- Duração com transparência de EAI -->
        <span class="flex items-center gap-1" :title="hasEaiAdjustment ? 'Tempo calibrado automaticamente via EAI' : 'Tempo estimado'">
          <Clock class="w-3.5 h-3.5" :class="hasEaiAdjustment ? 'text-content font-bold' : ''" />
          <span v-if="hasEaiAdjustment" class="line-through opacity-50 text-[10px] mr-0.5">{{ action.nominalDurationMinutes }}m</span>
          <strong class="text-content font-sans">{{ action.effectiveDurationMinutes }}m</strong>
        </span>

        <!-- Energia Requerida -->
        <span class="flex items-center gap-1">
          <Zap class="w-3.5 h-3.5" />
          <strong class="text-content font-sans">!{{ action.energyRequired }}</strong>
        </span>

        <!-- Tag Tática de Transparência EAI -->
        <span 
          v-if="hasEaiAdjustment" 
          class="flex items-center gap-1 text-[10px] font-bold bg-surface-active px-1.5 py-0.5 rounded border border-borderfocus/80 text-content"
          title="O algoritmo ajustou esta estimativa com base na sua velocidade real anterior"
        >
          <ShieldAlert class="w-3 h-3" />
          <span>EAI Ajustado</span>
        </span>
      </div>

      <ArrowRight class="w-4 h-4 opacity-0 group-hover:opacity-100 transition-opacity text-content transform group-hover:translate-x-0.5 duration-150" />
    </div>

    <!-- Razão Explícita em Lista -->
    <div class="text-[11px] text-content-muted/80 truncate font-sans -mt-1">
      ↳ {{ action.reason }}
    </div>
  </div>
</template>