<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useCommitmentsStore, type CommitmentItem } from '@/stores/commitmentsStore';
import { isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';
import { RefreshCw, Flame, Check, PlusCircle, Zap, Clock, Calendar, Trophy } from 'lucide-vue-next';

const store = useCommitmentsStore();
const pulsingHabitId = ref<string | null>(null);

onMounted(() => {
  store.fetchAllActive();
});

const habits = computed(() => store.habitsToday);

const openNewHabitModal = () => {
  isQuickCaptureOpen.value = true;
};

// Conclusão Otimista com animação tática no ícone de fogo (Cap. 7.2)
const handleCompleteHabit = (item: CommitmentItem) => {
  if (item.status === 'COMPLETED') return;

  pulsingHabitId.value = item.id;
  store.updateStatus(item.id, 'COMPLETED');

  setTimeout(() => {
    pulsingHabitId.value = null;
  }, 300);
};

// Rastreio de calor de consistência (Cap. 5.5)
const getStreakVariant = (streak: number) => {
  if (streak > 21) {
    return 'bg-zinc-100 text-zinc-950 border-zinc-300 font-semibold';
  }

  if (streak >= 8) {
    return 'bg-amber-500/10 text-amber-300 border-amber-500/30';
  }

  return 'bg-zinc-900 text-zinc-400 border-zinc-800';
};
</script>

<template>
  <div class="max-w-4xl mx-auto space-y-8 select-none">
    <!-- Cabeçalho de Hábitos -->
    <div class="flex items-center justify-between gap-4 pb-4 border-b border-zinc-800">
      <div>
        <h1 class="text-2xl font-semibold text-zinc-100 tracking-tight flex items-center gap-2.5">
          <span>Hábitos Diários</span>
          <span class="text-xs font-mono bg-zinc-900 text-zinc-400 px-2 py-0.5 rounded border border-zinc-800">
            {{ habits.length }} Ativos
          </span>
        </h1>
        <p class="text-sm text-zinc-400 mt-1">
          Manutenção de consistência diária (streaks) integrada diretamente às invariantes de energia do motor.
        </p>
      </div>

      <button 
        @click="openNewHabitModal"
        class="inline-flex items-center gap-2 px-3 py-1.5 rounded-tactic bg-zinc-900 hover:bg-zinc-800 border border-zinc-800 text-xs font-medium text-zinc-300 hover:text-white transition-all shadow-sm"
      >
        <PlusCircle class="w-3.5 h-3.5 text-zinc-300" />
        <span>Novo Hábito</span>
        <kbd class="px-1 text-[10px] font-mono bg-zinc-950 rounded border border-zinc-800 text-zinc-500">C</kbd>
      </button>
    </div>

    <!-- ESTADO VAZIO -->
    <div v-if="habits.length === 0" class="p-12 rounded-xl border border-dashed border-zinc-800 bg-zinc-950/40 text-center space-y-4">
      <div class="w-12 h-12 rounded-full bg-zinc-900 border border-zinc-700 flex items-center justify-center mx-auto text-zinc-200">
        <RefreshCw class="w-6 h-6" />
      </div>
      <div class="max-w-sm mx-auto space-y-1">
        <h3 class="text-base font-semibold text-zinc-200">Nenhum hábito diário monitorado</h3>
        <p class="text-xs text-zinc-500 leading-relaxed">
          Hábitos criam disciplina mecânica. Cadastre sua primeira rotina diária para iniciar o rastreamento de calor.
        </p>
      </div>
      <button 
        @click="openNewHabitModal"
        class="inline-flex items-center gap-2 px-4 py-2 rounded-tactic bg-zinc-100 hover:bg-white text-zinc-950 text-xs font-medium transition-all"
      >
        <PlusCircle class="w-4 h-4" />
        <span>Criar Hábito Agora</span>
      </button>
    </div>

    <!-- LISTA DE HÁBITOS DE ALTA DENSIDADE (Cap. 5.5) -->
    <div v-else class="space-y-3">
      <div 
        v-for="item in habits" 
        :key="item.id"
        class="group relative flex items-start justify-between gap-4 p-4 rounded-lg border border-zinc-800/80 bg-zinc-900/40 hover:bg-zinc-900/90 transition-all duration-tactic"
        :class="{ 'opacity-60 bg-zinc-950/40': item.status === 'COMPLETED' }"
      >
        <!-- Checkbox de Streak Diário -->
        <div class="flex items-start gap-3 min-w-0 flex-1">
          <button 
            type="button"
            @click="handleCompleteHabit(item)"
            class="mt-0.5 w-5 h-5 rounded border border-zinc-700 bg-zinc-950 flex items-center justify-center transition-all focus-visible:ring-2 focus-visible:ring-zinc-300"
            :class="item.status === 'COMPLETED'
            ? 'bg-zinc-100 border-zinc-200 text-zinc-950'
            : 'hover:border-zinc-500'"
            :disabled="item.status === 'COMPLETED'"
            title="Concluir Hábito Hoje"
          >
            <Check v-if="item.status === 'COMPLETED'" class="w-3.5 h-3.5 stroke-[3]" />
          </button>

          <!-- Centro: Hierarquia de Dados -->
          <div class="min-w-0 flex-1 space-y-1.5">
            <p 
              class="text-sm font-medium text-zinc-100 group-hover:text-white transition-colors truncate"
              :class="{ 'line-through text-zinc-500': item.status === 'COMPLETED' }"
            >
              {{ item.title }}
            </p>

            <div class="flex flex-wrap items-center gap-3 text-xs font-mono text-zinc-500">
              <span class="text-zinc-400">
                {{ item.energyRequired === 3 ? 'Alta' : item.energyRequired === 1 ? 'Baixa' : 'Média' }}
              </span>
              <span>•</span>
              <span>CRON: {{ item.cronExpression || 'Todos os dias' }}</span>
              <span>•</span>
              <span class="flex items-center gap-1 text-zinc-400">
                <Trophy class="w-3 h-3 text-amber-500" /> Recorde: {{ item.bestStreak || item.currentStreak }}d
              </span>
            </div>
          </div>
        </div>

        <!-- Lado Direito: O Badge de Rastreio de Calor (Streak Heat Badge) -->
        <div class="flex items-center gap-3 flex-shrink-0">
          <span 
            class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md text-xs font-mono border transition-transform duration-tactic"
            :class="[
              getStreakVariant(item.currentStreak),
              { 'scale-125 shadow-[0_0_14px_rgba(255,255,255,0.18)]': pulsingHabitId === item.id }
            ]"
          >
            <Flame class="w-3.5 h-3.5 inline text-amber-500" :class="{ 'animate-bounce': pulsingHabitId === item.id }" />
            <span>{{ item.currentStreak }}d</span>
          </span>
        </div>
      </div>
    </div>
  </div>
</template>