<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useDecisionStore } from '@/stores/decisionStore';
import { useCommitmentsStore } from '@/stores/commitmentsStore';
import { useSettingsStore } from '@/stores/settingsStore';
import { useProjectsStore } from '@/stores/projectsStore';
import { useGoalsStore } from '@/stores/goalsStore';
import TopFocusCard from '@/components/core/TopFocusCard.vue';
import CommitmentCard from '@/components/core/CommitmentCard.vue';
import TacticalHorizonBar, { type HorizonOption } from '@/components/core/TacticalHorizonBar.vue';
import PageHeader from '@/components/layout/PageHeader.vue';
import InspectableCard from '@/components/core/InspectableCard.vue';
import { isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';
import { RefreshCw, Sparkles, PlusCircle, Sunrise, ListTodo } from 'lucide-vue-next';

const decisionStore = useDecisionStore();
const commitmentsStore = useCommitmentsStore();
const settingsStore = useSettingsStore();
const projectsStore = useProjectsStore();
const goalsStore = useGoalsStore();

const currentHorizon = ref<HorizonOption>('today');
const isForceRefreshing = ref(false);

const viewDensity = computed(() => settingsStore.getViewDensity('now'));

onMounted(async () => {
  
  await Promise.all([
  commitmentsStore.fetchAllActive(), 
  projectsStore.fetchCatalog(),
  goalsStore.fetchGoals()
]);
  
  await decisionStore.fetchNow(); 
});

const handleRefresh = async () => {
  isForceRefreshing.value = true;
  await commitmentsStore.fetchAllActive();
  await decisionStore.fetchNow();
  isForceRefreshing.value = false;
};

const openCreateModal = () => {
  isQuickCaptureOpen.value = true;
};


const getBreadcrumb = (projectId: string | null | undefined): string | null => {
  if (!projectId) return null;
  const project = projectsStore.entities[projectId];
  if (!project) return null;

  const goal = project.goalId ? goalsStore.entities[project.goalId] : null;
  
  // Se tiver Meta e Projeto: "Minha Meta ❯ Meu Projeto"
  // Se tiver só Projeto: "Meu Projeto"
  return goal ? `${goal.title} ❯ ${project.name}` : project.name;
};

const horizonBuckets = computed(() => {
  const allKnownEntities = Object.values(commitmentsStore.entities);
  
  const allPending = allKnownEntities.filter(i => 
    i.type === 'TASK' && (i.status === 'PENDING' || i.status === 'IN_PROGRESS')
  );

  const now = new Date();
  const todayEnd = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 59, 59).getTime();
  
  const MS_IN_A_DAY = 86400000;
  const tomorrowEnd = todayEnd + MS_IN_A_DAY;
  const threeDaysEnd = todayEnd + MS_IN_A_DAY * 3;
  const weekEnd = todayEnd + MS_IN_A_DAY * 7;

  const buckets = {
    today: [] as any[],
    tomorrow: [] as any[],
    '3days': [] as any[],
    week: [] as any[]
  };

  allPending.forEach(item => {
    const targetTime = item.deadline ? new Date(item.deadline).getTime() : todayEnd + (MS_IN_A_DAY * 5);

    if (targetTime <= todayEnd) buckets.today.push(item);
    else if (targetTime <= tomorrowEnd) buckets.tomorrow.push(item);
    else if (targetTime <= threeDaysEnd) buckets['3days'].push(item);
    else buckets.week.push(item);
  });

  return {
    counts: {
      today: buckets.today.length, 
      tomorrow: buckets.tomorrow.length,
      '3days': buckets['3days'].length,
      week: buckets.week.length
    },
    lists: {
      today: buckets.today,
      tomorrow: buckets.tomorrow,
      '3days': buckets['3days'],
      week: buckets.week
    }
  };
});

//  ENRIQUECIMENTO: Injetamos o Breadcrumb nas recomendações do Motor de Decisão
const enrichedTopFocus = computed(() => {
  if (!decisionStore.topFocus) return null;
  const base = decisionStore.topFocus;
  const task = commitmentsStore.entities[base.commitmentId];
  return {
    ...base,
    projectName: getBreadcrumb(task?.projectId) || base.projectName
  };
});

const enrichedAlternatives = computed(() => {
  return decisionStore.alternatives.map(alt => {
    const task = commitmentsStore.entities[alt.commitmentId];
    return {
      ...alt,
      projectName: getBreadcrumb(task?.projectId) || alt.projectName
    };
  });
});

const remainingTodayTasks = computed(() => {
  const suggestedIds = new Set(decisionStore.topActions.map(a => a.commitmentId));
  return horizonBuckets.value.lists.today.filter((task: any) => !suggestedIds.has(task.id));
});

const activeFutureList = computed(() => {
  if (currentHorizon.value === 'today') return [];
  return horizonBuckets.value.lists[currentHorizon.value as 'tomorrow' | '3days' | 'week'];
});
</script>

<template>
  <div class="max-w-4xl mx-auto space-y-6 select-none">
    
    <PageHeader 
      title="Motor de Decisão"
      description="O algoritmo filtrou suas opções e selecionou a ação com maior retorno tático para o seu momento."
      viewName="now"
      :showDensityToggle="true"
    >
      <template #extra-actions>
        <button 
          @click="handleRefresh" 
          :disabled="decisionStore.isLoading || isForceRefreshing"
          class="inline-flex items-center gap-2 px-3 py-1.5 rounded-tactic bg-surface hover:bg-surface-hover border border-borderbase text-xs font-mono text-content-muted hover:text-content transition-all disabled:opacity-50 cursor-pointer"
          title="Recalcular Sugestão (R)"
        >
          <RefreshCw class="w-3.5 h-3.5" :class="{ 'animate-spin': decisionStore.isLoading || isForceRefreshing }" />
          <span class="hidden sm:inline">Recalcular</span>
        </button>
      </template>
    </PageHeader>

    <TacticalHorizonBar v-model="currentHorizon" :counts="horizonBuckets.counts" />

    <!-- ABA: TURNOS FUTUROS -->
    <template v-if="currentHorizon !== 'today'">
      <div v-if="activeFutureList.length === 0" class="p-10 rounded-xl border border-dashed border-borderbase bg-app/40 text-center space-y-4 my-6">
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
          <span class="font-mono">{{ activeFutureList.length }} itens planejados</span>
        </div>
        
        <transition-group name="fade-list" tag="div" class="space-y-2">
          <InspectableCard 
            v-for="task in activeFutureList" 
            :key="task.id"
            :entity="task"
            type="COMMITMENT"
          >
            <CommitmentCard 
              :density="viewDensity"
              :action="{
                commitmentId: task.id, title: task.title, type: task.type,
                nominalDurationMinutes: task.estimatedDurationMinutes || 30,
                effectiveDurationMinutes: task.estimatedDurationMinutes || 30,
                energyRequired: task.energyRequired || 2,
                projectName: getBreadcrumb(task.projectId), /*  O(1) Breadcrumb Injetado */
                scorePercentage: 0,
                reason: 'Aguardando recálculo do motor.',
                wasTimeAdjustedByEai: false
              }"
              :isMockedAction="true" 
            />
          </InspectableCard>
        </transition-group>
      </div>
    </template>

    <!-- ABA: TURNO CORRENTE ("HOJE") -->
    <template v-else>
      <div v-if="(decisionStore.isLoading || isForceRefreshing) && !decisionStore.topFocus" class="space-y-6 animate-pulse">
        <div class="p-6 rounded-xl border border-borderbase bg-surface space-y-4 h-[220px]">
          <div class="w-32 h-6 bg-surface-active rounded" />
          <div class="w-3/4 h-8 bg-surface-active rounded" />
        </div>
      </div>

      <div v-else-if="horizonBuckets.counts.today === 0" class="p-8 rounded-xl border border-dashed border-borderbase bg-app/60 text-center space-y-6 my-8">
        <div class="w-12 h-12 rounded-full bg-surface-active border border-borderfocus flex items-center justify-center mx-auto text-content">
          <Sparkles class="w-6 h-6" />
        </div>
        <div class="max-w-md mx-auto space-y-2">
          <h3 class="text-lg font-semibold text-content">Nenhuma ação pendente para hoje</h3>
          <p class="text-sm text-content-muted leading-relaxed">Sua lista está limpa. Aproveite o descanso ou planeje o amanhã.</p>
        </div>
        <div class="flex flex-wrap items-center justify-center gap-4 pt-2">
          <button @click="openCreateModal" class="inline-flex items-center gap-2 px-4 py-2 rounded-tactic bg-content text-content-invert hover:opacity-90 text-sm font-medium cursor-pointer">
            <PlusCircle class="w-4 h-4" /> <span>Criar Nova Tarefa</span>
          </button>
        </div>
      </div>

      <template v-else>
        <!-- SEÇÃO 1: MOTOR DE DECISÃO -->
        <section v-if="enrichedTopFocus" aria-label="Ação Prioritária Recomendada">
          <InspectableCard 
            v-if="commitmentsStore.entities[enrichedTopFocus.commitmentId]"
            :entity="commitmentsStore.entities[enrichedTopFocus.commitmentId]"
            type="COMMITMENT"
          >
            <TopFocusCard :item="enrichedTopFocus" :density="viewDensity" />
          </InspectableCard>
        </section>

        <!-- SEÇÃO 2: ALTERNATIVAS -->
        <section v-if="enrichedAlternatives.length > 0" aria-label="Alternativas" class="space-y-3 pt-4">
          <div class="flex items-center justify-between text-xs font-semibold text-content-muted uppercase tracking-wider px-1">
            <span>Alternativas</span>
            <span class="font-mono">{{ enrichedAlternatives.length }} sugeridas</span>
          </div>

          <transition-group name="fade-list" tag="div" class="space-y-2">
            <template v-for="alt in enrichedAlternatives" :key="alt.commitmentId">
              <InspectableCard
                v-if="commitmentsStore.entities[alt.commitmentId]"
                :entity="commitmentsStore.entities[alt.commitmentId]"
                type="COMMITMENT"
              >
                <CommitmentCard :action="alt" :density="viewDensity" />
              </InspectableCard>
            </template>
          </transition-group>
        </section>

        <!-- SEÇÃO 3: BACKLOG -->
        <section v-if="remainingTodayTasks.length > 0" aria-label="Restante do Turno" class="space-y-3 pt-8 pb-4">
          <div class="flex items-center gap-2 text-xs font-semibold text-content-muted uppercase tracking-wider px-1 border-b border-borderbase pb-2">
            <ListTodo class="w-4 h-4" />
            <span>Restante do Turno de Hoje</span>
            <span class="ml-auto font-mono bg-surface-active px-2 py-0.5 rounded">{{ remainingTodayTasks.length }} itens</span>
          </div>

          <transition-group name="fade-list" tag="div" class="space-y-2">
            <InspectableCard 
              v-for="task in remainingTodayTasks" 
              :key="task.id"
              :entity="task"
              type="COMMITMENT"
            >
              <CommitmentCard 
                :density="viewDensity"
                :action="{
                  commitmentId: task.id, title: task.title, type: task.type,
                  nominalDurationMinutes: task.estimatedDurationMinutes || 30,
                  effectiveDurationMinutes: task.estimatedDurationMinutes || 30,
                  energyRequired: task.energyRequired || 2,
                  projectName: getBreadcrumb(task.projectId), /*  O(1) Breadcrumb Injetado */
                  scorePercentage: 0,
                  reason: 'Aguardando capacidade operacional.',
                  wasTimeAdjustedByEai: false
                }"
                :isMockedAction="true" 
              />
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