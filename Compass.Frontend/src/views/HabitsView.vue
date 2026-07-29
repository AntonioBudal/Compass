<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useCommitmentsStore, type CommitmentItem } from '@/stores/commitmentsStore';
import { isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';
import { RefreshCw, Flame, Check, PlusCircle, Trophy, Calendar, Sparkles } from 'lucide-vue-next';
import TacticalHorizonBar, { type HorizonOption } from '@/components/core/TacticalHorizonBar.vue';

const store = useCommitmentsStore();
const pulsingHabitId = ref<string | null>(null);

// ABA ATUAL DE HORIZONTE TÁTICO
const currentHorizon = ref<HorizonOption>('today');

onMounted(() => {
  store.fetchAllActive();
});

// Filtra os hábitos visíveis conforme o horizonte selecionado no topo
const habits = computed(() => {
  const allHabits = store.items.filter(i => i.type === 'HABIT' && i.status !== 'ARCHIVED');
  
  if (currentHorizon.value === 'today') {
    return allHabits.filter(i => !i.cronExpression || i.cronExpression.includes('*'));
  }
  // Em horizontes expandidos ("tomorrow", "3days", "week"), exibe todos os hábitos cadastrados
  return allHabits;
});

// Contadores para o cabeçalho temporal
const horizonCounts = computed(() => {
  const all = store.items.filter(i => i.type === 'HABIT' && i.status !== 'ARCHIVED').length;
  return {
    today: store.habitsToday.length,
    tomorrow: all,
    '3days': all,
    week: all
  };
});

const openNewHabitModal = () => {
  isQuickCaptureOpen.value = true;
};

// Conclusão com trava defensiva (impedir dois cliques e comemorar consistência)
const handleCompleteHabit = (item: CommitmentItem) => {
  if (item.status === 'COMPLETED') return;

  pulsingHabitId.value = item.id;
  store.updateStatus(item.id, 'COMPLETED');

  setTimeout(() => {
    pulsingHabitId.value = null;
  }, 300);
};

const getStreakVariant = (streak: number) => {
  if (streak > 21) return 'bg-content text-content-invert border-borderhighlight font-semibold';
  if (streak >= 8) return 'bg-status-warning-bg text-status-warning border-status-warning-border';
  return 'bg-surface text-content-muted border-borderbase';
};
</script>

<template>
  <div class="max-w-4xl mx-auto space-y-6 select-none">
    <!-- Cabeçalho de Hábitos -->
    <div class="flex items-center justify-between gap-4 pb-4 border-b border-borderbase">
      <div>
        <h1 class="text-2xl font-semibold text-content tracking-tight flex items-center gap-2.5">
          <span>Hábitos Diários</span>
          <span class="text-xs font-mono bg-surface text-content-muted px-2 py-0.5 rounded border border-borderbase">
            {{ habits.length }} no Horizonte
          </span>
        </h1>
        <p class="text-sm text-content-muted mt-1">
          Manutenção de consistência diária (streaks) integrada diretamente às invariantes de energia do motor.
        </p>
      </div>

      <button 
        @click="openNewHabitModal"
        class="inline-flex items-center gap-2 px-3 py-1.5 rounded-tactic bg-surface hover:bg-surface-hover border border-borderbase text-xs font-medium text-content-muted hover:text-content transition-all shadow-sm cursor-pointer"
      >
        <PlusCircle class="w-3.5 h-3.5 text-content-muted" />
        <span>Novo Hábito</span>
        <kbd class="px-1 text-[10px] font-mono bg-app rounded border border-borderbase text-content-muted">C</kbd>
      </button>
    </div>

    <!-- HORIZONTE TÁTICO (Evita que itens fiquem escondidos por ciclo) -->
    <TacticalHorizonBar v-model="currentHorizon" :counts="horizonCounts" />

    <!-- ESTADO VAZIO DEFENSIVO COM EXPLICAÇÃO DO HORIZONTE -->
    <div v-if="habits.length === 0" class="p-12 rounded-xl border border-dashed border-borderbase bg-app/40 text-center space-y-4">
      <div class="w-12 h-12 rounded-full bg-surface border border-borderfocus flex items-center justify-center mx-auto text-content">
        <RefreshCw class="w-6 h-6" />
      </div>
      <div class="max-w-sm mx-auto space-y-1">
        <h3 class="text-base font-semibold text-content">
          {{ currentHorizon === 'today' ? 'Nenhum hábito programado para Hoje' : 'Nenhum hábito diário cadastrado' }}
        </h3>
        <p class="text-xs text-content-muted leading-relaxed">
          {{ currentHorizon === 'today'
            ? 'Você possui hábitos configurados em outros dias da semana. Alterne para [Amanhã] ou [Próxima Semana] para visualizá-los.'
            : 'Hábitos criam disciplina mecânica. Cadastre sua primeira rotina diária para iniciar o rastreamento de calor.' }}
        </p>
      </div>
      <div class="flex items-center justify-center gap-3">
        <button 
          @click="openNewHabitModal"
          class="inline-flex items-center gap-2 px-4 py-2 rounded-tactic bg-content hover:bg-content-accent text-content-invert text-xs font-medium transition-all cursor-pointer shadow-sm"
        >
          <PlusCircle class="w-4 h-4" />
          <span>Criar Hábito Agora</span>
        </button>

        <button
          v-if="currentHorizon === 'today' && horizonCounts.week > 0"
          @click="currentHorizon = 'week'"
          class="inline-flex items-center gap-2 px-4 py-2 rounded-tactic bg-surface hover:bg-surface-hover border border-borderbase text-content text-xs font-medium transition-all cursor-pointer"
        >
          <span>Ver Próxima Semana ({{ horizonCounts.week }})</span>
        </button>
      </div>
    </div>

    <!-- LISTA DE HÁBITOS DE ALTA DENSIDADE -->
    <div v-else class="space-y-3">
      <div 
        v-for="item in habits" 
        :key="item.id"
        class="group relative flex items-start justify-between gap-4 p-4 rounded-lg border border-borderbase bg-surface hover:bg-surface-hover transition-all duration-tactic"
        :class="{ 'opacity-60 bg-app/40': item.status === 'COMPLETED' }"
      >
        <!-- Checkbox de Streak Diário -->
        <div class="flex items-start gap-3 min-w-0 flex-1">
          <button 
            type="button"
            @click="handleCompleteHabit(item)"
            class="mt-0.5 w-5 h-5 rounded border border-borderbase bg-app flex items-center justify-center transition-all focus-visible:ring-2 focus-visible:ring-borderhighlight cursor-pointer"
            :class="item.status === 'COMPLETED'
              ? 'bg-status-success-bg border-status-success-border text-status-success-text shadow-sm'
              : 'hover:border-borderfocus'"
            :disabled="item.status === 'COMPLETED'"
            title="Concluir Hábito Hoje"
          >
            <Check v-if="item.status === 'COMPLETED'" class="w-3.5 h-3.5 stroke-[3]" />
          </button>

          <!-- Centro: Hierarquia de Dados -->
          <div class="min-w-0 flex-1 space-y-1.5">
            <p 
              class="text-sm font-medium text-content group-hover:text-content-accent transition-colors truncate"
              :class="{ 'line-through text-content-muted': item.status === 'COMPLETED' }"
            >
              {{ item.title }}
            </p>

            <div class="flex flex-wrap items-center gap-3 text-xs font-mono text-content-muted">
              <span class="text-content-muted">
                {{ item.energyRequired === 3 ? 'Alta' : item.energyRequired === 1 ? 'Baixa' : 'Média' }}
              </span>
              <span>•</span>
              <span>CRON: {{ item.cronExpression || 'Todos os dias' }}</span>
              <span>•</span>
              <span class="flex items-center gap-1 text-content-muted">
                <Trophy class="w-3 h-3 text-status-warning" /> Recorde: {{ item.bestStreak || item.currentStreak }}d
              </span>
            </div>
          </div>
        </div>

        <!-- Lado Direito: O Badge de Rastreio de Calor -->
        <div class="flex items-center gap-3 flex-shrink-0">
          <span 
            class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md text-xs font-mono border transition-transform duration-tactic"
            :class="[
              getStreakVariant(item.currentStreak),
              { 'scale-125 shadow-[0_0_14px_rgba(255,255,255,0.18)]': pulsingHabitId === item.id }
            ]"
          >
            <Flame class="w-3.5 h-3.5 inline text-status-warning" :class="{ 'animate-bounce': pulsingHabitId === item.id }" />
            <span>{{ item.currentStreak }}d</span>
          </span>
        </div>
      </div>
    </div>
  </div>
</template>