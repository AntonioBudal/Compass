<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useDecisionStore } from '@/stores/decisionStore';
import { useCommitmentsStore } from '@/stores/commitmentsStore';
import { useSettingsStore } from '@/stores/settingsStore';
import TopFocusCard from '@/components/core/TopFocusCard.vue';
import CommitmentCard from '@/components/core/CommitmentCard.vue';
import TacticalHorizonBar, { type HorizonOption } from '@/components/core/TacticalHorizonBar.vue';
import PageHeader from '@/components/layout/PageHeader.vue';
import InspectableCard from '@/components/core/InspectableCard.vue';
import { isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';
import { RefreshCw, Sparkles, PlusCircle, Sunrise, ArrowRight } from 'lucide-vue-next';

const decisionStore = useDecisionStore();
const commitmentsStore = useCommitmentsStore();
const settingsStore = useSettingsStore();

const currentHorizon = ref<HorizonOption>('today');

// Reatividade de Densidade (Compacto vs Detalhado)
const viewDensity = computed(() => settingsStore.getViewDensity('now'));

onMounted(async () => {
  await Promise.all([
    commitmentsStore.fetchAllActive(),
    decisionStore.fetchNow()
  ]);
});

const handleRefresh = () => {
  decisionStore.fetchNow();
};

const openCreateModal = () => {
  isQuickCaptureOpen.value = true;
};

const futureTasks = computed(() => {
  const allPending = commitmentsStore.items.filter(i => 
    i.type === 'TASK' && (i.status === 'PENDING' || i.status === 'IN_PROGRESS')
  );

  if (currentHorizon.value === 'today') return [];

  const now = new Date();
  const todayEnd = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 59, 59).getTime();
  const tomorrowEnd = todayEnd + 86400000;
  const threeDaysEnd = todayEnd + 86400000 * 3;
  const weekEnd = todayEnd + 86400000 * 7;

  return allPending.filter(item => {
    if (!item.deadline) return currentHorizon.value === 'week';
    const targetTime = new Date(item.deadline).getTime();
    if (currentHorizon.value === 'tomorrow') return targetTime > todayEnd && targetTime <= tomorrowEnd;
    if (currentHorizon.value === '3days') return targetTime > todayEnd && targetTime <= threeDaysEnd;
    if (currentHorizon.value === 'week') return targetTime > todayEnd && targetTime <= weekEnd;
    return false;
  });
});

const horizonCounts = computed(() => {
  const allPending = commitmentsStore.items.filter(i => 
    i.type === 'TASK' && (i.status === 'PENDING' || i.status === 'IN_PROGRESS')
  );
  const now = new Date();
  const todayEnd = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 59, 59).getTime();

  let tomorrowCount = 0; let threeDaysCount = 0; let weekCount = 0;

  allPending.forEach(item => {
    const targetTime = item.deadline ? new Date(item.deadline).getTime() : todayEnd + 86400000 * 5;
    if (targetTime > todayEnd && targetTime <= todayEnd + 86400000) tomorrowCount++;
    if (targetTime > todayEnd && targetTime <= todayEnd + 86400000 * 3) threeDaysCount++;
    if (targetTime > todayEnd && targetTime <= todayEnd + 86400000 * 7) weekCount++;
  });

  return {
    today: (decisionStore.topFocus ? 1 : 0) + decisionStore.alternatives.length,
    tomorrow: tomorrowCount,
    '3days': threeDaysCount,
    week: weekCount
  };
});
</script>

<template>
  <div class="max-w-4xl mx-auto space-y-6 select-none">
    
    <!-- 1. O NOVO CABEÇALHO COM O TOGGLE DE DENSIDADE -->
    <PageHeader 
      title="Motor de Decisão"
      description="O algoritmo filtrou suas opções e selecionou a ação com maior retorno tático para o seu momento."
      viewName="now"
      :showDensityToggle="true"
    >
      <template #extra-actions>
        <button 
          @click="handleRefresh" 
          :disabled="decisionStore.isLoading"
          class="inline-flex items-center gap-2 px-3 py-1.5 rounded-tactic bg-surface hover:bg-surface-hover border border-borderbase text-xs font-mono text-content-muted hover:text-content transition-all disabled:opacity-50 cursor-pointer"
          title="Recalcular Sugestão (R)"
        >
          <RefreshCw class="w-3.5 h-3.5" :class="{ 'animate-spin': decisionStore.isLoading }" />
          <span class="hidden sm:inline">Recalcular</span>
        </button>
      </template>
    </PageHeader>

    <TacticalHorizonBar v-model="currentHorizon" :counts="horizonCounts" />

    <!-- ABA: TURNOS FUTUROS -->
    <template v-if="currentHorizon !== 'today'">
      <div v-if="futureTasks.length === 0" class="p-10 rounded-xl border border-dashed border-borderbase bg-app/40 text-center space-y-4 my-6">
        <div class="w-12 h-12 rounded-full bg-surface border border-borderfocus flex items-center justify-center mx-auto text-content">
          <Sunrise class="w-6 h-6 text-content-accent" />
        </div>
        <div class="max-w-md mx-auto space-y-1">
          <h3 class="text-base font-semibold text-content">
            Nenhuma atividade agendada para {{ currentHorizon === 'tomorrow' ? 'Amanhã' : currentHorizon === '3days' ? 'os Próximos 3 Dias' : 'a Próxima Semana' }}
          </h3>
          <p class="text-xs text-content-muted leading-relaxed">As atividades que excederem o seu turno serão organizadas aqui.</p>
        </div>
        <div class="flex items-center justify-center gap-3">
          <button @click="openCreateModal" class="inline-flex items-center gap-2 px-4 py-2 rounded-tactic bg-content text-content-invert hover:opacity-90 text-xs font-semibold shadow-sm cursor-pointer">
            <PlusCircle class="w-3.5 h-3.5" /> <span>Agendar Tarefa</span>
          </button>
        </div>
      </div>

      <div v-else class="space-y-3 pt-2">
        <div class="flex items-center justify-between text-xs font-semibold text-content-muted uppercase tracking-wider px-1">
          <span>Fila de Execução</span>
          <span class="font-mono">{{ futureTasks.length }} itens planejados</span>
        </div>
        
        <transition-group name="fade-list" tag="div" class="space-y-2">
          <InspectableCard 
            v-for="task in futureTasks" 
            :key="task.id"
            :entity="task"
            type="COMMITMENT"
          >
            <!-- Injetando a prop de densidade reativa nos cards futuros -->
            <CommitmentCard 
              :density="viewDensity"
              :action="{
                commitmentId: task.id, title: task.title, type: task.type,
                nominalDurationMinutes: task.estimatedDurationMinutes || 30,
                effectiveDurationMinutes: task.estimatedDurationMinutes || 30,
                energyRequired: task.energyRequired || 2,
                scorePercentage: 0, reason: '', wasTimeAdjustedByEai: false,
                projectName: task.projectName || null
              }"
            />
          </InspectableCard>
        </transition-group>
      </div>
    </template>

    <!-- ABA: TURNO CORRENTE ("HOJE") -->
    <template v-else>
      <div v-if="decisionStore.isLoading && !decisionStore.topFocus" class="space-y-6 animate-pulse">
        <div class="p-6 rounded-xl border border-borderbase bg-surface space-y-4 h-[220px]">
          <div class="w-32 h-6 bg-surface-active rounded" />
          <div class="w-3/4 h-8 bg-surface-active rounded" />
        </div>
      </div>

      <div v-else-if="!decisionStore.topFocus" class="p-8 rounded-xl border border-dashed border-borderbase bg-app/60 text-center space-y-6 my-8">
        <div class="w-12 h-12 rounded-full bg-surface-active border border-borderfocus flex items-center justify-center mx-auto text-content">
          <Sparkles class="w-6 h-6" />
        </div>
        <div class="max-w-md mx-auto space-y-2">
          <h3 class="text-lg font-semibold text-content">Nenhuma ação acionável para hoje</h3>
          <p class="text-sm text-content-muted leading-relaxed">Sua lista está limpa. Aproveite o descanso ou planeje o amanhã.</p>
        </div>
        <div class="flex flex-wrap items-center justify-center gap-4 pt-2">
          <button @click="openCreateModal" class="inline-flex items-center gap-2 px-4 py-2 rounded-tactic bg-content text-content-invert hover:opacity-90 text-sm font-medium cursor-pointer">
            <PlusCircle class="w-4 h-4" /> <span>Criar Nova Tarefa</span>
          </button>
        </div>
      </div>

      <template v-else>
        <!-- 2. INJEÇÃO DA DENSIDADE NO TOP FOCUS -->
        <section aria-label="Ação Prioritária Recomendada">
          <InspectableCard 
            :entity="commitmentsStore.items.find(i => i.id === decisionStore.topFocus?.commitmentId)"
            type="COMMITMENT"
          >
            <TopFocusCard :item="decisionStore.topFocus" :density="viewDensity" />
          </InspectableCard>
        </section>

        <section v-if="decisionStore.alternatives.length > 0" aria-label="Alternativas" class="space-y-3 pt-4">
          <div class="flex items-center justify-between text-xs font-semibold text-content-muted uppercase tracking-wider px-1">
            <span>Alternativas</span>
            <span class="font-mono">{{ decisionStore.alternatives.length }} disponíveis</span>
          </div>

          <transition-group name="fade-list" tag="div" class="space-y-2">
            <!-- 3. INJEÇÃO DA DENSIDADE NOS CARDS ALTERNATIVOS -->
            <InspectableCard
              v-for="alt in decisionStore.alternatives" 
              :key="alt.commitmentId"
              :entity="commitmentsStore.items.find(i => i.id === alt.commitmentId)"
              type="COMMITMENT"
            >
              <CommitmentCard :action="alt" :density="viewDensity" />
            </InspectableCard>
          </transition-group>
        </section>
      </template>
    </template>
  </div>
</template>

<style scoped>
.fade-list-enter-active,
.fade-list-leave-active { transition: opacity 200ms ease, transform 200ms ease; }
.fade-list-enter-from,
.fade-list-leave-to { opacity: 0; transform: translateY(4px); }
</style>