<script setup lang="ts">
import { onMounted } from 'vue';
import { useDecisionStore } from '@/stores/decisionStore';
import { useCommitmentsStore } from '@/stores/commitmentsStore';
import TopFocusCard from '@/components/core/TopFocusCard.vue';
import CommitmentCard from '@/components/core/CommitmentCard.vue';
import { isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';
import { RefreshCw, Zap, Clock, Sparkles, PlusCircle } from 'lucide-vue-next';

const decisionStore = useDecisionStore();
const commitmentsStore = useCommitmentsStore();

onMounted(async () => {
  // Sincroniza tarefas locais e consulta o motor de decisão em paralelo
  await Promise.all([
    commitmentsStore.fetchAllActive(),
    decisionStore.fetchNow()
  ]);
})
const handleRefresh = () => {
  decisionStore.fetchNow();
};

const openCreateModal = () => {
  isQuickCaptureOpen.value = true;
};
</script>

<template>
  <div class="max-w-4xl mx-auto space-y-8 select-none">
    <!-- Cabeçalho de Ação da Tela -->
    <div class="flex items-center justify-between gap-4 pb-4 border-b border-borderbase">
      <div>
        <h1 class="text-2xl font-semibold text-content tracking-tight">
          Motor de Decisão <span class="text-content-muted font-mono text-sm ml-2">v1.0</span>
        </h1>
        <p class="text-sm text-content-muted mt-1">
          O algoritmo filtrou suas opções e selecionou a ação com maior retorno tático para o seu momento.
        </p>
      </div>

      <div class="flex items-center gap-2">
        <button 
          @click="handleRefresh" 
          :disabled="decisionStore.isLoading"
          class="inline-flex items-center gap-2 px-3 py-1.5 rounded-tactic bg-surface hover:bg-surface-hover border border-borderbase text-xs font-mono text-content-muted hover:text-content transition-all disabled:opacity-50 cursor-pointer"
          title="Recalcular Sugestão (R)"
        >
          <RefreshCw class="w-3.5 h-3.5" :class="{ 'animate-spin': decisionStore.isLoading }" />
          <span>Recalcular</span>
        </button>
      </div>
    </div>

    <!-- 1. ESTADO DE CARREGAMENTO (Skeleton Zero Layout-Shift - Cap. 7.3) -->
    <div v-if="decisionStore.isLoading && !decisionStore.topFocus" class="space-y-6 animate-pulse">
      <div class="p-6 rounded-xl border border-borderbase bg-surface space-y-4 h-[220px]">
        <div class="w-32 h-6 bg-surface-active rounded" />
        <div class="w-3/4 h-8 bg-surface-active rounded" />
        <div class="flex gap-4 pt-4">
          <div class="w-24 h-5 bg-surface-active rounded" />
          <div class="w-24 h-5 bg-surface-active rounded" />
        </div>
        <div class="flex justify-between pt-4">
          <div class="w-36 h-9 bg-surface-active rounded" />
          <div class="w-24 h-9 bg-surface-active rounded" />
        </div>
      </div>
      <div class="space-y-2">
        <div class="w-1/3 h-4 bg-surface-active rounded" />
        <div class="w-full h-14 bg-surface rounded-lg border border-borderbase" />
        <div class="w-full h-14 bg-surface rounded-lg border border-borderbase" />
      </div>
    </div>

    <!-- 2. ESTADO VAZIO TÁTICO (Action-Oriented Empty State - Cap. 5.6) -->
    <div v-else-if="!decisionStore.topFocus" class="p-8 rounded-xl border border-dashed border-borderbase bg-app/60 text-center space-y-6 my-12">
      <div class="w-12 h-12 rounded-full bg-surface-active border border-borderfocus flex items-center justify-center mx-auto text-content">
        <Sparkles class="w-6 h-6" />
      </div>

      <div class="max-w-md mx-auto space-y-2">
        <h3 class="text-lg font-semibold text-content">Nenhuma ação acionável no momento</h3>
        <p class="text-sm text-content-muted leading-relaxed">
          Sua lista está limpa ou as tarefas pendentes excedem o tempo disponível na sua janela de 
          <strong class="text-content font-mono">{{ decisionStore.availableMinutes }}m livres</strong>.
        </p>
      </div>

      <div class="flex flex-wrap items-center justify-center gap-4 pt-2">
        <button 
          @click="openCreateModal"
          class="inline-flex items-center gap-2 px-4 py-2 rounded-tactic bg-content text-content-invert hover:bg-content-accent text-sm font-medium transition-colors cursor-pointer"
        >
          <PlusCircle class="w-4 h-4" />
          <span>Criar Nova Tarefa</span>
          <kbd class="ml-1 rounded border border-borderbase bg-surface px-1.5 py-0.5 text-[10px] font-mono font-semibold text-content shadow-[inset_0_-1px_0_rgba(0,0,0,.08)]">
            C
          </kbd>
        </button>

        <router-link 
          to="/agenda"
          class="inline-flex items-center gap-2 px-4 py-2 rounded-tactic bg-surface hover:bg-surface-hover border border-borderbase text-content-muted hover:text-content text-sm transition-all"
        >
          <Clock class="w-4 h-4 text-content-muted" />
          <span>Ver Agenda Completa</span>
        </router-link>
      </div>
    </div>

    <!-- 3. ESTADO COM DADOS (Hero Top Focus + Alternativas - Cap. 5.1) -->
    <template v-else>
      <!-- Hero Section: O Card Top Focus -->
      <section aria-label="Ação Prioritária Recomendada">
        <TopFocusCard :item="decisionStore.topFocus" />
      </section>

      <!-- Seção Secundária: Alternativas (Máximo 2) -->
      <section v-if="decisionStore.alternatives.length > 0" aria-label="Alternativas " class="space-y-3 pt-4">
        <div class="flex items-center justify-between text-xs font-semibold text-content-muted uppercase tracking-wider px-1">
          <span>Alternativas (Caso não seja o momento ideal)</span>
          <span class="font-mono">{{ decisionStore.alternatives.length }} disponíveis</span>
        </div>

        <div class="space-y-2">
          <!-- Convertendo ScoredActionDto em CommitmentItem compatível com o CommitmentCard -->
          <CommitmentCard 
            v-for="alt in decisionStore.alternatives" 
            :key="alt.id"
            :item="{
              id: alt.id,
              title: alt.title,
              type: alt.type,
              status: 'PENDING',
              estimatedDurationMinutes: alt.estimatedDurationMinutes,
              energyRequired: alt.energyRequired,
              deadline: null,
              startTime: null,
              endTime: null,
              locationOrLink: null,
              cronExpression: null,
              currentStreak: 0,
              bestStreak: 0,
              postponedCount: 0,
              content: null,
              projectId: null,
              projectName: alt.projectName
            }"
          />
        </div>
      </section>
    </template>
  </div>
</template>