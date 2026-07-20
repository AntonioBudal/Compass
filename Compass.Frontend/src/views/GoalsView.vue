<script setup lang="ts">
import { ref } from 'vue';
import { Target, PlusCircle, ChevronRight } from 'lucide-vue-next';
import { isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';

const expandedGoalIds = ref<string[]>(['goal-1']);

const toggleGoal = (id: string) => {
  const index = expandedGoalIds.value.indexOf(id);
  if (index === -1) expandedGoalIds.value.push(id);
  else expandedGoalIds.value.splice(index, 1);
};

const goals = ref([
  {
    id: 'goal-1',
    title: 'Lançamento do Compass MVP (Q3 2026)',
    why: 'Provar a viabilidade de um software de produtividade local-first em .NET 10 e Vue 3.',
    targetDate: '30 de Setembro de 2026',
    progressPercentage: 65,
    status: 'ACTIVE',
    projectsCount: 3,
    children: [
      { name: 'Compass Backend Core (.NET 10 REST API)', status: 'IN_PROGRESS', progress: '80%' },
      { name: 'Auth & Identity JWT', status: 'PENDING', progress: '50%' },
      { name: 'Design System UI (Zinc Monocromático)', status: 'COMPLETED', progress: '100%' },
    ]
  },
  {
    id: 'goal-2',
    title: 'Excelência em Engenharia e Arquitetura Limpa',
    why: 'Dominar os padrões DDD e CQRS em ambientes de missão crítica.',
    targetDate: '15 de Dezembro de 2026',
    progressPercentage: 40,
    status: 'ACTIVE',
    projectsCount: 2,
    children: [
      { name: 'Leitura de Arquitetura de Software (Hábito)', status: 'IN_PROGRESS', progress: '60%' },
      { name: 'Refatoração do Motor de Scoring', status: 'IN_PROGRESS', progress: '20%' },
    ]
  }
]);
</script>

<template>
  <div class="max-w-4xl mx-auto space-y-8 select-none">
    <div class="flex items-center justify-between gap-4 pb-4 border-b border-zinc-800">
      <div>
        <h1 class="text-2xl font-semibold text-zinc-100 tracking-tight flex items-center gap-2.5">
          <span>Metas Estratégicas</span>
          <span class="text-xs font-mono bg-zinc-900 text-zinc-400 px-2 py-0.5 rounded border border-zinc-800">{{ goals.length }} Ativas</span>
        </h1>
        <p class="text-sm text-zinc-400 mt-1">Guarda-chuvas estratégicos que direcionam os multiplicadores do algoritmo de pontuação.</p>
      </div>
      <button 
        @click="isQuickCaptureOpen = true"
        class="inline-flex items-center gap-2 px-3 py-1.5 rounded-tactic bg-zinc-900 hover:bg-zinc-800 border border-zinc-800 text-xs font-medium text-zinc-300 hover:text-white transition-all shadow-sm cursor-pointer"
      >
        <PlusCircle class="w-3.5 h-3.5" />
        <span>Nova Meta</span>
        <kbd class="px-1 text-[10px] font-mono bg-zinc-950 rounded border border-zinc-800 text-zinc-500">G</kbd>
      </button>
    </div>

    <!-- ACCORDIONS (Métricas Linear-style) -->
    <div class="space-y-4">
      <div 
        v-for="goal in goals" 
        :key="goal.id"
        class="rounded-xl border border-zinc-800/80 bg-zinc-900/40 overflow-hidden transition-all duration-tactic"
        :class="{ 'border-zinc-700/80 bg-zinc-900/80 shadow-lg': expandedGoalIds.includes(goal.id) }"
      >
        <button 
          @click="toggleGoal(goal.id)"
          class="w-full p-5 flex items-center justify-between gap-4 text-left hover:bg-zinc-900/60 transition-colors cursor-pointer"
        >
          <div class="flex items-center gap-3 min-w-0 flex-1">
            <ChevronRight 
              class="w-5 h-5 text-zinc-500 transition-transform duration-200 flex-shrink-0"
              :class="{ 'rotate-90 text-zinc-300': expandedGoalIds.includes(goal.id) }"
            />
            <div class="truncate space-y-1">
              <h2 class="text-base font-semibold text-zinc-100 truncate flex items-center gap-2">
                <span>{{ goal.title }}</span>
              </h2>
              <p class="text-xs text-zinc-500 truncate font-mono">
                Prazo: {{ goal.targetDate }} | Vinculados: {{ goal.projectsCount }}
              </p>
            </div>
          </div>

          <div class="flex items-center gap-4 flex-shrink-0">
            <div class="flex flex-col items-end gap-1.5">
              <span class="text-sm font-mono text-zinc-300">Progresso: {{ goal.progressPercentage }}%</span>
              <div class="h-1 w-24 bg-zinc-800 rounded-full overflow-hidden">
                <div class="h-full bg-zinc-400 transition-all duration-500" :style="{ width: `${goal.progressPercentage}%` }" />
              </div>
            </div>
            <span class="text-[10px] font-mono uppercase bg-zinc-800 text-zinc-300 border border-zinc-700 px-2 py-0.5 rounded">{{ goal.status }}</span>
          </div>
        </button>

        <!-- ÁRVORE DE PROJETOS -->
        <div v-if="expandedGoalIds.includes(goal.id)" class="px-5 pb-5 pt-2 border-t border-zinc-800/80 bg-zinc-950/60 space-y-4">
          <div class="p-3 rounded bg-zinc-900/60 border border-zinc-800/60 text-xs text-zinc-400">
            <strong class="text-zinc-200 font-mono">Propósito:</strong> {{ goal.why }}
          </div>

          <div class="space-y-2">
            <p class="text-xs font-mono uppercase tracking-wider text-zinc-500">Módulos de Execução:</p>
            <div class="space-y-1.5 font-mono text-xs pl-2">
              <div 
                v-for="(child, idx) in goal.children" 
                :key="child.name"
                class="flex items-center justify-between gap-4 text-zinc-400 hover:text-zinc-200 transition-colors py-1 px-2 rounded hover:bg-zinc-900/40"
              >
                <div class="flex items-center gap-2 truncate">
                  <span class="text-zinc-600 select-none">{{ idx === goal.children.length - 1 ? '└──' : '├──' }}</span>
                  <span class="truncate">{{ child.name }}</span>
                </div>
                <div class="flex items-center gap-3 flex-shrink-0">
                  <span class="text-[10px] px-1.5 py-0.5 rounded border border-zinc-700 bg-zinc-800 text-zinc-400">{{ child.status }}</span>
                  <span class="text-zinc-500 w-8 text-right">{{ child.progress }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>