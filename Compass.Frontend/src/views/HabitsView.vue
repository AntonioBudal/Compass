<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useCommitmentsStore, type CommitmentItem } from '@/stores/commitmentsStore';
import { useSettingsStore } from '@/stores/settingsStore';
import { isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';
import { RefreshCw, Flame, Check, PlusCircle, Trophy } from 'lucide-vue-next';
import TacticalHorizonBar, { type HorizonOption } from '@/components/core/TacticalHorizonBar.vue';
import PageHeader from '@/components/layout/PageHeader.vue';
import InspectableCard from '@/components/core/InspectableCard.vue';

const store = useCommitmentsStore();
const settingsStore = useSettingsStore();

const pulsingHabitId = ref<string | null>(null);
const currentHorizon = ref<HorizonOption>('today');

// Reatividade de Densidade (Compacto vs Detalhado)
const viewDensity = computed(() => settingsStore.getViewDensity('habits'));

onMounted(() => {
  store.fetchAllActive();
});

const habits = computed(() => {
  const allHabits = store.items.filter(i => i.type === 'HABIT' && i.status !== 'ARCHIVED');
  
  if (currentHorizon.value === 'today') {
    return allHabits.filter(i => !i.cronExpression || i.cronExpression.includes('*'));
  }
  return allHabits;
});

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
    
    <!-- 1. CABEÇALHO UNIVERSAL COM DENSITY TOGGLE -->
    <PageHeader 
      title="Hábitos Diários"
      :badgeCount="habits.length"
      badgeLabel="no Horizonte"
      description="Manutenção de consistência diária (streaks) integrada diretamente às invariantes de energia do motor."
      actionLabel="Novo Hábito"
      :actionIcon="PlusCircle"
      @action="openNewHabitModal"
      viewName="habits"
      :showDensityToggle="true"
    />

    <TacticalHorizonBar v-model="currentHorizon" :counts="horizonCounts" />

    <!-- ESTADO VAZIO DEFENSIVO -->
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
            ? 'Você possui hábitos configurados em outros dias. Alterne para [Amanhã] ou [Próxima Semana].'
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

    <!-- LISTA DE HÁBITOS COM DENSIDADE REATIVA -->
    <div v-else class="space-y-2">
      <InspectableCard
        v-for="item in habits" 
        :key="item.id"
        :entity="item"
        type="COMMITMENT"
      >
        <div 
          class="flex justify-between gap-4 rounded-lg border border-borderbase bg-surface hover:bg-surface-hover transition-all duration-tactic w-full"
          :class="[
            { 'opacity-60 bg-app/40': item.status === 'COMPLETED' },
            viewDensity === 'compact' ? 'p-2.5 items-center' : 'p-4 items-start'
          ]"
        >
          <div class="flex items-start gap-3 min-w-0 flex-1">
            <!-- Checkbox de Streak -->
            <button 
              type="button"
              @click.stop="handleCompleteHabit(item)"
              class="rounded border border-borderbase bg-app flex items-center justify-center transition-all focus-visible:ring-2 focus-visible:ring-borderhighlight cursor-pointer flex-shrink-0"
              :class="[
                item.status === 'COMPLETED' ? 'bg-status-success-bg border-status-success-border text-status-success-text shadow-sm' : 'hover:border-borderfocus',
                viewDensity === 'compact' ? 'w-4 h-4 mt-0' : 'w-5 h-5 mt-0.5'
              ]"
              :disabled="item.status === 'COMPLETED'"
              title="Concluir Hábito Hoje"
            >
              <Check v-if="item.status === 'COMPLETED'" class="stroke-[3]" :class="viewDensity === 'compact' ? 'w-3 h-3' : 'w-3.5 h-3.5'" />
            </button>

            <!-- Título e Dados -->
            <div class="min-w-0 flex-1" :class="viewDensity === 'compact' ? 'flex items-center justify-between gap-4' : 'space-y-1.5'">
              <p 
                class="font-medium text-content transition-colors truncate"
                :class="[
                  { 'line-through text-content-muted': item.status === 'COMPLETED' },
                  viewDensity === 'compact' ? 'text-xs' : 'text-sm'
                ]"
              >
                {{ item.title }}
              </p>

              <!-- Metadados Táticos (Ocultos no modo Compacto para limpar a tela) -->
              <div v-if="viewDensity === 'detailed'" class="flex flex-wrap items-center gap-3 text-xs font-mono text-content-muted">
                <span class="text-content-muted">{{ item.energyRequired === 3 ? 'Alta' : item.energyRequired === 1 ? 'Baixa' : 'Média' }}</span>
                <span>•</span>
                <span>CRON: {{ item.cronExpression || 'Todos os dias' }}</span>
                <span>•</span>
                <span class="flex items-center gap-1 text-content-muted">
                  <Trophy class="w-3 h-3 text-status-warning" /> Recorde: {{ item.bestStreak || item.currentStreak }}d
                </span>
              </div>
            </div>
          </div>

          <!-- Flame Badge Rastreio de Calor -->
          <div class="flex items-center gap-3 flex-shrink-0">
            <span 
              class="inline-flex items-center gap-1.5 rounded-md font-mono border transition-transform duration-tactic"
              :class="[
                getStreakVariant(item.currentStreak),
                { 'scale-125 shadow-[0_0_14px_rgba(255,255,255,0.18)]': pulsingHabitId === item.id },
                viewDensity === 'compact' ? 'px-2 py-0.5 text-[10px]' : 'px-2.5 py-1 text-xs'
              ]"
            >
              <Flame class="inline text-status-warning" :class="[{ 'animate-bounce': pulsingHabitId === item.id }, viewDensity === 'compact' ? 'w-3 h-3' : 'w-3.5 h-3.5']" />
              <span>{{ item.currentStreak }}d</span>
            </span>
          </div>
        </div>
      </InspectableCard>
    </div>
  </div>
</template>