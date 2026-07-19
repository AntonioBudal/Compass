<script setup lang="ts">
import { ref } from 'vue';
import { Target, PlusCircle, ChevronRight, CheckCircle2, Clock, Zap } from 'lucide-vue-next';
import { isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';

// Controle reativo dos objetivos abertos na sanfona (Accordion UI)
const expandedGoalIds = ref<string[]>(['goal-1']); // Abre o primeiro objetivo por padrão

const toggleGoal = (id: string) => {
  const index = expandedGoalIds.value.indexOf(id);
  if (index === -1) {
    expandedGoalIds.value.push(id);
  } else {
    expandedGoalIds.value.splice(index, 1);
  }
};

// Dados de Estratégia TPT (Metas de Longo Prazo)
const goals = ref([
  {
    id: 'goal-1',
    title: '🚀 Lançamento do Compass MVP (Q3 2026)',
    why: 'Provar a viabilidade e latência zero de um software de produtividade local-first em .NET 10 e Vue 3.',
    targetDate: '30 de Setembro de 2026',
    progressPercentage: 65,
    status: 'Active',
    projectsCount: 3,
    children: [
      { name: '⚡ Compass Backend Core (.NET 10 REST API)', status: 'IN_PROGRESS', progress: '80%' },
      { name: '🔐 Auth & Identity JWT com Validação Fluent', status: 'PENDING', progress: '50%' },
      { name: '🎨 Design System UI & Tailwind CSS Tokens', status: 'COMPLETED', progress: '100%' },
    ]
  },
  {
    id: 'goal-2',
    title: '🎯 Excelência em Engenharia e Arquitetura Limpa',
    why: 'Dominar os padrões DDD, Clean Architecture e CQRS em ambientes de missão crítica.',
    targetDate: '15 de Dezembro de 2026',
    progressPercentage: 40,
    status: 'Active',
    projectsCount: 2,
    children: [
      { name: '📚 Leitura Diária de Livros de Arquitetura (Hábito)', status: 'IN_PROGRESS', progress: '60%' },
      { name: '🛠️ Refatoração do Motor de Scoring com Cascatas', status: 'IN_PROGRESS', progress: '20%' },
    ]
  }
]);

const openNewGoalModal = () => {
  isQuickCaptureOpen.value = true;
};
</script>

<template>
  <div class="max-w-4xl mx-auto space-y-8 select-none">
    <!-- Cabeçalho de Metas Estratégicas -->
    <div class="flex items-center justify-between gap-4 pb-4 border-b border-zinc-800">
      <div>
        <h1 class="text-2xl font-semibold text-zinc-100 tracking-tight flex items-center gap-2.5">
          <span>Metas Estratégicas</span>
          <span class="text-xs font-mono bg-zinc-900 text-zinc-400 px-2 py-0.5 rounded border border-zinc-800">
            {{ goals.length }} Ativas
          </span>
        </h1>
        <p class="text-sm text-zinc-400 mt-1">
          Guarda-chuvas estratégicos que direcionam os multiplicadores do algoritmo de pontuação.
        </p>
      </div>

      <button 
        @click="openNewGoalModal"
        class="inline-flex items-center gap-2 px-3 py-1.5 rounded-tactic bg-zinc-900 hover:bg-zinc-800 border border-zinc-800 text-xs font-medium text-zinc-300 hover:text-white transition-all shadow-sm"
      >
        <PlusCircle class="w-3.5 h-3.5 text-indigo-400" />
        <span>Nova Meta</span>
        <kbd class="px-1 text-[10px] font-mono bg-zinc-950 rounded border border-zinc-800 text-zinc-500">G</kbd>
      </button>
    </div>

    <!-- LISTA DE CARDS SANFONA (Accordion Cards — Cap. 5.4) -->
    <div class="space-y-4">
      <div 
        v-for="goal in goals" 
        :key="goal.id"
        class="rounded-xl border border-zinc-800/80 bg-zinc-900/40 overflow-hidden transition-all duration-tactic"
        :class="{ 'border-zinc-700/80 bg-zinc-900/80 shadow-lg': expandedGoalIds.includes(goal.id) }"
      >
        <!-- Cabeçalho Acionável da Sanfona -->
        <button 
          @click="toggleGoal(goal.id)"
          class="w-full p-5 flex items-center justify-between gap-4 text-left hover:bg-zinc-900/60 transition-colors cursor-pointer"
        >
          <div class="flex items-center gap-3 min-w-0 flex-1">
            <ChevronRight 
              class="w-5 h-5 text-zinc-500 transition-transform duration-200 flex-shrink-0"
              :class="{ 'rotate-90 text-indigo-400': expandedGoalIds.includes(goal.id) }"
            />
            <div class="truncate space-y-1">
              <h2 class="text-base font-semibold text-zinc-100 truncate flex items-center gap-2">
                <span>{{ goal.title }}</span>
              </h2>
              <p class="text-xs text-zinc-400 truncate">
                Prazo Alvo: {{ goal.targetDate }} • Projetos Vinculados: {{ goal.projectsCount }}
              </p>
            </div>
          </div>

          <!-- Indicador Percentual em Monospace & Barra Linear de 4px (Cap. 5.4) -->
          <div class="flex items-center gap-4 flex-shrink-0">
            <div class="flex flex-col items-end gap-1.5">
              <span class="text-sm font-mono text-indigo-400 font-bold">
                Progresso: {{ goal.progressPercentage }}%
              </span>
              <div class="h-1 w-24 bg-zinc-800 rounded-full overflow-hidden">
                <div 
                  class="h-full bg-indigo-500 transition-all duration-500"
                  :style="{ width: `${goal.progressPercentage}%` }"
                />
              </div>
            </div>

            <span class="text-[10px] font-mono uppercase bg-indigo-500/10 text-indigo-400 border border-indigo-500/20 px-2 py-0.5 rounded">
              {{ goal.status }}
            </span>
          </div>
        </button>

        <!-- ÁREA DE EXPANSÃO: OS PROJETOS FILHOS COM DESENHO DE TERMINAL (Cap. 5.4) -->
        <div 
          v-if="expandedGoalIds.includes(goal.id)"
          class="px-5 pb-5 pt-2 border-t border-zinc-800/80 bg-zinc-950/60 space-y-4 animate-fadeIn"
        >
          <div class="p-3 rounded bg-zinc-900/60 border border-zinc-800/60 text-xs text-zinc-300">
            <strong class="text-indigo-400 font-mono">Why (Propósito):</strong> {{ goal.why }}
          </div>

          <div class="space-y-2">
            <p class="text-xs font-mono uppercase tracking-wider text-zinc-500">
              Projetos Filhos & Módulos:
            </p>

            <!-- Desenho hierárquico em fonte Monospace -->
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
                  <span 
                    class="text-[10px] px-1.5 py-0.5 rounded border"
                    :class="child.status === 'COMPLETED' 
                      ? 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20' 
                      : 'bg-zinc-800 text-zinc-400 border-zinc-700'"
                  >
                    {{ [child.status] }}
                  </span>
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

<style scoped>
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(-4px); }
  to { opacity: 1; transform: translateY(0); }
}
.animate-fadeIn {
  animation: fadeIn 150ms ease-out forwards;
}
</style>