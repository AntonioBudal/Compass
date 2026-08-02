<script setup lang="ts">
import { computed } from 'vue';
import { useDecisionStore, type ScoredActionDto } from '@/stores/decisionStore';
import { useCommitmentsStore } from '@/stores/commitmentsStore'; // 🔥 Injetado
import ScoreBreakdownPanel from '@/components/core/ScoreBreakdownPanel.vue';
import { Zap, Clock, Terminal, ShieldAlert, CheckCircle2, CornerDownRight } from 'lucide-vue-next';

const props = withDefaults(defineProps<{
  item: ScoredActionDto | null;
  density?: 'detailed' | 'compact';
}>(), {
  density: 'detailed'
});

const decisionStore = useDecisionStore();
const commitmentsStore = useCommitmentsStore(); // 🔥 Injetado
const profile = computed(() => decisionStore.adaptiveProfile);

const emit = defineEmits<{
  (e: 'complete', id: string): void;
  (e: 'postpone', id: string): void;
}>();

// 🔥 CORREÇÃO (BUG-007): Componente Autônomo para Conclusão
const handleComplete = async (id: string) => {
  // 1. Marca como concluído no banco local/API
  await commitmentsStore.updateStatus(id, 'COMPLETED');
  
  // 2. Gira o motor de decisão para puxar a próxima melhor tarefa!
  await decisionStore.fetchNow();
  
  emit('complete', id); // Mantido por retrocompatibilidade
};

// 🔥 CORREÇÃO (BUG-007): Componente Autônomo para Adiar (+15m)
const handlePostpone = async (id: string) => {
  // 1. Busca a tarefa real no cache
  const target = commitmentsStore.items.find(i => i.id === id);
  if (target) {
    // 2. Adiciona 15 minutos ao tempo estimado
    const newTime = (target.estimatedDurationMinutes || 30) + 15;
    
    // 3. Salva a alteração (passando 'true' no final para ser silencioso sem toast)
    await commitmentsStore.updateCommitment(id, { estimatedDurationMinutes: newTime }, true);
    
    // 4. Gira o motor para recalcular a pontuação com o novo tempo
    await decisionStore.fetchNow();
  }
  
  emit('postpone', id); // Mantido por retrocompatibilidade
};
</script>

<template>
  <div 
    class="w-full bg-surface border-2 border-borderfocus rounded-xl shadow-lg relative overflow-hidden font-mono select-none transition-all duration-200"
    :class="density === 'compact' ? 'p-3.5' : 'p-5'"
  >
    
    <!-- Linha Superior: Arquétipo & Status -->
    <div v-if="density === 'detailed'" class="flex items-center justify-between text-xs text-content-muted pb-3 border-b border-borderbase/60">
      <div class="flex items-center gap-2">
        <Terminal class="w-4 h-4 text-content" />
        <span class="font-bold uppercase tracking-wider text-content">Top Focus — Recomendação Primária</span>
      </div>
      <div class="flex items-center gap-2">
        <span v-if="decisionStore.isServingFromCache" class="px-1.5 py-0.5 text-[10px] bg-surface-hover border border-borderbase rounded uppercase">
          RAM Offline
        </span>
        <span class="px-2 py-0.5 text-[11px] font-bold bg-content text-content-invert rounded uppercase tracking-wide">
          {{ item?.scorePercentage || 0 }}% Match
        </span>
      </div>
    </div>

    <!-- Conteúdo Central: Título e Projeto -->
    <div v-if="item" :class="density === 'compact' ? 'pb-2 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4' : 'py-5'">
      <div class="flex-1 min-w-0">
        <div v-if="item.projectName" class="font-bold text-content-accent uppercase tracking-wider mb-1 flex items-center gap-1.5" :class="density === 'compact' ? 'text-[10px]' : 'text-xs'">
          <CornerDownRight class="w-3.5 h-3.5" />
          <span class="truncate">#{{ item.projectName }}</span>
        </div>
        <h2 class="font-sans font-extrabold text-content leading-snug break-words truncate" :class="density === 'compact' ? 'text-lg' : 'text-xl'">
          {{ item.title }}
        </h2>
      </div>

      <!-- Ações Rápidas (Compacto) -->
      <div v-if="density === 'compact'" class="flex items-center gap-2 flex-shrink-0">
        <!-- 🔥 Binds de Eventos Corrigidos -->
        <button @click.stop="handlePostpone(item.commitmentId)" class="px-3 py-1.5 text-[11px] font-bold rounded bg-surface-hover hover:bg-surface-active text-content border border-borderbase transition-colors cursor-pointer">
          Adiar
        </button>
        <button @click.stop="handleComplete(item.commitmentId)" class="px-3 py-1.5 text-[11px] font-bold rounded bg-content text-content-invert hover:opacity-90 transition-opacity flex items-center gap-1.5 cursor-pointer shadow-sm">
          <CheckCircle2 class="w-3.5 h-3.5" />
          <span>Concluir</span>
        </button>
      </div>
    </div>

    <div v-else class="text-center text-content-muted" :class="density === 'compact' ? 'py-4' : 'py-10'">
      <p class="text-sm font-sans">Nenhuma ação compatível encontrada para sua janela de {{ decisionStore.availableWindow }}m.</p>
    </div>

    <!-- Métrica Tática & Ações Rápidas (Detalhado) -->
    <div v-if="item" class="flex flex-wrap items-center justify-between gap-4" :class="density === 'compact' ? 'pt-2 border-t border-borderbase/60' : 'pt-3 border-t border-borderbase/60'">
      <div class="flex items-center gap-4 text-xs">
        <div class="flex items-center gap-1.5">
          <Clock class="w-4 h-4 text-content-muted" />
          <span v-if="item.wasTimeAdjustedByEai && density === 'detailed'" class="line-through text-content-muted opacity-60 text-[11px]">
            {{ item.nominalDurationMinutes }}m
          </span>
          <strong class="text-content font-sans text-sm">{{ item.effectiveDurationMinutes }}m</strong>
        </div>
        <div class="flex items-center gap-1">
          <Zap class="w-4 h-4 text-content-muted" />
          <strong class="text-content font-sans text-sm">!{{ item.energyRequired }}</strong>
        </div>
      </div>

      <!-- Ações Rápidas (Detalhado) -->
      <div v-if="density === 'detailed'" class="flex items-center gap-2">
        <!-- 🔥 Binds de Eventos Corrigidos -->
        <button @click.stop="handlePostpone(item.commitmentId)" class="px-3 py-1.5 text-xs font-bold rounded bg-surface-hover hover:bg-surface-active text-content border border-borderbase transition-colors cursor-pointer">
          Adiar (+15m)
        </button>
        <button @click.stop="handleComplete(item.commitmentId)" class="px-4 py-1.5 text-xs font-bold rounded bg-content text-content-invert hover:opacity-90 transition-opacity flex items-center gap-1.5 cursor-pointer shadow-sm">
          <CheckCircle2 class="w-3.5 h-3.5" />
          <span>Concluir</span>
        </button>
      </div>
    </div>

    <!-- BADGE DE TRANSPARÊNCIA E EXPLICABILIDADE -->
    <template v-if="density === 'detailed'">
      <div class="mt-4 pt-2.5 border-t border-borderbase/40 min-h-[32px] flex items-center">
        <div v-if="item?.wasTimeAdjustedByEai" class="w-full flex items-center justify-between text-[11px] bg-surface-hover border border-borderfocus/60 px-3 py-1 rounded text-content">
          <span class="flex items-center gap-1.5 font-bold truncate">
            <ShieldAlert class="w-3.5 h-3.5 text-content flex-shrink-0" />
            <span class="truncate">EAI Calibrado ({{ profile.eaiMultiplier }}x): Tempo ajustado com base no seu histórico.</span>
          </span>
        </div>
        
        <div v-else-if="item" class="w-full text-[11px] text-content-muted flex items-center justify-between truncate">
          <span class="truncate">💡 {{ item.reason }}</span>
          <span v-if="profile.isCalibrated" class="text-[10px] opacity-60 font-mono ml-2 flex-shrink-0">[Perfil Adaptativo Ativo]</span>
        </div>
      </div>

      <ScoreBreakdownPanel v-if="item" :action="item" class="mt-3" />
    </template>

  </div>
</template>