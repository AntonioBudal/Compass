<script setup lang="ts">
import { computed } from 'vue';
import { useDecisionStore, type ScoredActionDto } from '@/stores/decisionStore';
import ScoreBreakdownPanel from '@/components/core/ScoreBreakdownPanel.vue';
import { Zap, Clock, Terminal, ShieldAlert, CheckCircle2, CornerDownRight } from 'lucide-vue-next';

const decisionStore = useDecisionStore();
const focus = computed<ScoredActionDto | null>(() => decisionStore.primaryFocus);
const profile = computed(() => decisionStore.adaptiveProfile);

const emit = defineEmits<{
  (e: 'complete', id: string): void;
  (e: 'postpone', id: string): void;
}>();
</script>

<template>
  <div class="w-full bg-surface border-2 border-borderfocus rounded-xl p-5 shadow-lg relative overflow-hidden font-mono select-none transition-all duration-200">
    
    <!-- Linha Superior: Arquétipo & Status do Motor -->
    <div class="flex items-center justify-between text-xs text-content-muted pb-3 border-b border-borderbase/60">
      <div class="flex items-center gap-2">
        <Terminal class="w-4 h-4 text-content" />
        <span class="font-bold uppercase tracking-wider text-content">Top Focus — Recomendação Primária</span>
      </div>
      <div class="flex items-center gap-2">
        <span v-if="decisionStore.isServingFromCache" class="px-1.5 py-0.5 text-[10px] bg-surface-hover border border-borderbase rounded uppercase">
          RAM Offline
        </span>
        <span class="px-2 py-0.5 text-[11px] font-bold bg-content text-content-invert rounded uppercase tracking-wide">
          {{ focus?.scorePercentage || 0 }}% Match
        </span>
      </div>
    </div>

    <!-- Conteúdo Central: Título e Projeto -->
    <div v-if="focus" class="py-5">
      <div v-if="focus.projectName" class="text-xs font-bold text-content-accent uppercase tracking-wider mb-1 flex items-center gap-1.5">
        <CornerDownRight class="w-3.5 h-3.5" />
        <span>#{{ focus.projectName }}</span>
      </div>
      <h2 class="text-xl font-sans font-extrabold text-content leading-snug break-words">
        {{ focus.title }}
      </h2>
    </div>

    <!-- Estado Vazio (Sem Tarefas na Janela) -->
    <div v-else class="py-10 text-center text-content-muted">
      <p class="text-sm font-sans">Nenhuma ação compatível encontrada para sua janela de {{ decisionStore.availableWindow }}m.</p>
    </div>

    <!-- Métrica Tática & Ações Rápidas -->
    <div v-if="focus" class="pt-3 border-t border-borderbase/60 flex flex-wrap items-center justify-between gap-4">
      <div class="flex items-center gap-4 text-xs">
        <!-- Duração Nominal vs Efetiva -->
        <div class="flex items-center gap-1.5" title="Duração estimada para execução">
          <Clock class="w-4 h-4 text-content-muted" />
          <span v-if="focus.wasTimeAdjustedByEai" class="line-through text-content-muted opacity-60 text-[11px]">
            {{ focus.nominalDurationMinutes }}m
          </span>
          <strong class="text-content font-sans text-sm">{{ focus.effectiveDurationMinutes }}m</strong>
        </div>

        <!-- Energia Requerida -->
        <div class="flex items-center gap-1" title="Energia cognitiva necessária">
          <Zap class="w-4 h-4 text-content-muted" />
          <strong class="text-content font-sans text-sm">!{{ focus.energyRequired }}</strong>
        </div>
      </div>

      <!-- Botões de Ação Zero-Mouse -->
      <div class="flex items-center gap-2">
        <button 
          @click="emit('postpone', focus.commitmentId)"
          class="px-3 py-1.5 text-xs font-bold rounded bg-surface-hover hover:bg-surface-active text-content border border-borderbase transition-colors cursor-pointer"
        >
          Adiar (+15m)
        </button>
        <button 
          @click="emit('complete', focus.commitmentId)"
          class="px-4 py-1.5 text-xs font-bold rounded bg-content text-content-invert hover:opacity-90 transition-opacity flex items-center gap-1.5 cursor-pointer shadow-sm"
        >
          <CheckCircle2 class="w-3.5 h-3.5" />
          <span>Concluir</span>
        </button>
      </div>
    </div>

    <!-- BADGE DE TRANSPARÊNCIA ALGORTÍMICA (Altura Minimizada = Zero Layout Shift) -->
    <div class="mt-4 pt-2.5 border-t border-borderbase/40 min-h-[32px] flex items-center">
      <div v-if="focus?.wasTimeAdjustedByEai" class="w-full flex items-center justify-between text-[11px] bg-surface-hover border border-borderfocus/60 px-3 py-1 rounded text-content">
        <span class="flex items-center gap-1.5 font-bold truncate">
          <ShieldAlert class="w-3.5 h-3.5 text-content flex-shrink-0" />
          <span class="truncate">EAI Calibrado ({{ profile.eaiMultiplier }}x): Tempo ajustado com base no seu histórico de entregas.</span>
        </span>
        <span class="font-mono text-[10px] opacity-70 flex-shrink-0 ml-2">[{{ focus.nominalDurationMinutes }}m ➔ {{ focus.effectiveDurationMinutes }}m]</span>
      </div>
      
      <div v-else-if="focus" class="w-full text-[11px] text-content-muted flex items-center justify-between truncate">
        <span class="truncate">💡 {{ focus.reason }}</span>
        <span v-if="profile.isCalibrated" class="text-[10px] opacity-60 font-mono ml-2 flex-shrink-0">[Perfil Adaptativo Ativo]</span>
      </div>
    </div>

    <!-- MICRO-PAINEL DE EXPLICABILIDADE (Matriz de Pontuação CSS Grid) -->
    <ScoreBreakdownPanel v-if="focus" :action="focus" class="mt-3" />

  </div>
</template>