<script setup lang="ts">
import { computed } from 'vue';
import { type ScoredActionDto } from '@/stores/decisionStore';
import { Clock, Zap, Folder, ArrowRight, ShieldAlert } from 'lucide-vue-next';

const props = withDefaults(defineProps<{
  action: ScoredActionDto;
  density?: 'detailed' | 'compact';
}>(), {
  density: 'detailed'
});

const emit = defineEmits<{
  (e: 'select', id: string): void;
}>();

const hasEaiAdjustment = computed(() => props.action.wasTimeAdjustedByEai);
</script>

<template>
  <div 
    @click="emit('select', action.commitmentId)"
    class="w-full bg-surface border border-borderbase hover:border-borderfocus rounded-lg transition-all duration-150 flex flex-col justify-between font-mono cursor-pointer group select-none"
    :class="density === 'compact' ? 'p-2.5 gap-1.5' : 'p-3.5 gap-2.5'"
  >
    <!-- Topo: Título, Projeto e Score -->
    <div class="flex items-start justify-between gap-3">
      <div class="min-w-0 flex-1">
        <div v-if="action.projectName" class="text-[10px] font-bold text-content-accent uppercase tracking-wider mb-0.5 flex items-center gap-1">
          <Folder class="w-3 h-3 flex-shrink-0" />
          <span class="truncate">#{{ action.projectName }}</span>
        </div>
        <h3 class="font-sans font-semibold text-content group-hover:underline truncate" :class="density === 'compact' ? 'text-xs' : 'text-sm'">
          {{ action.title }}
        </h3>
      </div>
      <span class="font-bold text-content bg-surface-hover px-1.5 py-0.5 rounded border border-borderbase flex-shrink-0" :class="density === 'compact' ? 'text-[10px]' : 'text-xs'">
        {{ action.scorePercentage }}%
      </span>
    </div>

    <!-- Base: Metadados Táticos -->
    <div class="flex items-center justify-between text-content-muted" :class="density === 'compact' ? 'text-[11px] pt-1' : 'text-xs pt-2 border-t border-borderbase/40'">
      <div class="flex items-center gap-3">
        <!-- Duração -->
        <span class="flex items-center gap-1" :title="hasEaiAdjustment ? 'Tempo calibrado (EAI)' : 'Tempo estimado'">
          <Clock class="w-3.5 h-3.5" :class="hasEaiAdjustment ? 'text-content font-bold' : ''" />
          <span v-if="hasEaiAdjustment && density === 'detailed'" class="line-through opacity-50 text-[10px] mr-0.5">{{ action.nominalDurationMinutes }}m</span>
          <strong class="text-content font-sans">{{ action.effectiveDurationMinutes }}m</strong>
        </span>

        <!-- Energia Requerida -->
        <span class="flex items-center gap-1">
          <Zap class="w-3.5 h-3.5" />
          <strong class="text-content font-sans">!{{ action.energyRequired }}</strong>
        </span>

        <!-- Tag Tática EAI (Ocultada no Modo Compacto) -->
        <span 
          v-if="hasEaiAdjustment && density === 'detailed'" 
          class="flex items-center gap-1 text-[10px] font-bold bg-surface-active px-1.5 py-0.5 rounded border border-borderfocus/80 text-content"
        >
          <ShieldAlert class="w-3 h-3" />
          <span>EAI Ajustado</span>
        </span>
      </div>

      <ArrowRight class="w-4 h-4 opacity-0 group-hover:opacity-100 transition-opacity text-content transform group-hover:translate-x-0.5 duration-150" />
    </div>

    <!-- Razão Explícita em Lista (Ocultada no Modo Compacto) -->
    <div v-if="density === 'detailed' && action.reason" class="text-[11px] text-content-muted/80 truncate font-sans -mt-1">
      ↳ {{ action.reason }}
    </div>
  </div>
</template>