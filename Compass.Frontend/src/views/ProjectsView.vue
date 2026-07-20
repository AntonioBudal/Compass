<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { useCommitmentsStore } from '@/stores/commitmentsStore';
import { isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';
import { Folder, PlusCircle, Clock, CheckCircle2, AlertCircle } from 'lucide-vue-next';

const store = useCommitmentsStore();

onMounted(() => {
  store.fetchAllActive();
});

// Agregação inteligente de tarefas por Projeto (High SNR Data Processing)
interface ProjectSummary {
  id: string;
  name: string;
  linkedGoal: string;
  deadline: string;
  completedMinutes: number;
  totalMinutes: number;
  progressPercentage: number;
  status: 'IN_PROGRESS' | 'PENDING' | 'COMPLETED';
}

const projects = computed<ProjectSummary[]>(() => {
  const map = new Map<string, ProjectSummary>();

  store.items.forEach(item => {
    if (item.projectName) {
      const key = item.projectName;
      const existing = map.get(key) || {
        id: `proj-${key}`,
        name: key,
        linkedGoal: '🎯 Q3 Launch MVP', // Vínculo semântico padrão de MVP
        deadline: item.deadline || '30/08/2026',
        completedMinutes: 0,
        totalMinutes: 0,
        progressPercentage: 0,
        status: 'IN_PROGRESS'
      };

      const duration = item.estimatedDurationMinutes || 30;
      existing.totalMinutes += duration;
      if (item.status === 'COMPLETED') {
        existing.completedMinutes += duration;
      }

      existing.progressPercentage = existing.totalMinutes > 0 
        ? Math.round((existing.completedMinutes / existing.totalMinutes) * 100) 
        : 0;

      if (existing.progressPercentage === 100) {
        existing.status = 'COMPLETED';
      } else if (existing.completedMinutes === 0) {
        existing.status = 'PENDING';
      } else {
        existing.status = 'IN_PROGRESS';
      }

      map.set(key, existing);
    }
  });

  return Array.from(map.values());
});

const openNewProjectModal = () => {
  isQuickCaptureOpen.value = true;
};
</script>

<template>
  <div class="max-w-5xl mx-auto space-y-8 select-none">
    <!-- Cabeçalho de Projetos Ativos -->
    <div class="flex items-center justify-between gap-4 pb-4 border-b border-zinc-800">
      <div>
        <h1 class="text-2xl font-semibold text-zinc-100 tracking-tight flex items-center gap-2.5">
          <span>Projetos Ativos</span>
          <span class="text-xs font-mono bg-zinc-900 text-zinc-400 px-2 py-0.5 rounded border border-zinc-800">
            {{ projects.length }}
          </span>
        </h1>
        <p class="text-sm text-zinc-400 mt-1">
          Agrupadores finitos orientados a prazos com auditoria contínua de estimativas de tempo.
        </p>
      </div>

      <button 
        @click="openNewProjectModal"
        class="inline-flex items-center gap-2 px-3 py-1.5 rounded-tactic bg-zinc-900 hover:bg-zinc-800 border border-zinc-800 text-xs font-medium text-zinc-300 hover:text-white transition-all shadow-sm"
      >
        <PlusCircle class="w-3.5 h-3.5 text-zinc-300" />
        <span>Novo Projeto</span>
        <kbd class="px-1 text-[10px] font-mono bg-zinc-950 rounded border border-zinc-800 text-zinc-500">C</kbd>
      </button>
    </div>

    <!-- ESTADO VAZIO -->
    <div v-if="projects.length === 0" class="p-12 rounded-xl border border-dashed border-zinc-800 bg-zinc-950/40 text-center space-y-4">
      <div class="w-12 h-12 rounded-full bg-zinc-900 border bborder-zinc-700 flex items-center justify-center mx-auto text-zinc-400">
        <Folder class="w-6 h-6" />
      </div>
      <div class="max-w-sm mx-auto space-y-1">
        <h3 class="text-base font-semibold text-zinc-200">Nenhum projeto associado às suas tarefas</h3>
        <p class="text-xs text-zinc-500 leading-relaxed">
          Ao criar uma tarefa no K-Menu ou Captura Rápida, atribua um nome de projeto para visualizá-lo nesta grade linear.
        </p>
      </div>
      <button 
        @click="openNewProjectModal"
        class="inline-flex items-center gap-2 px-4 py-2 rounded-tactic bg-zinc-100 hover:bg-white text-zinc-950 text-xs font-medium transition-all"
      >
        <PlusCircle class="w-4 h-4" />
        <span>Criar Tarefa com Projeto</span>
      </button>
    </div>

    <!-- TABELA DE ALTA DENSIDADE (Linear/GitHub-style — Cap. 5.3) -->
    <div v-else class="border border-zinc-800/80 rounded-lg overflow-hidden bg-zinc-950">
      <!-- Cabeçalho Tabular -->
      <div class="grid grid-cols-12 gap-4 px-4 py-2.5 bg-zinc-900/80 border-b border-zinc-800 text-[11px] font-mono font-semibold text-zinc-400 uppercase tracking-wider">
        <div class="col-span-4">Título do Projeto</div>
        <div class="col-span-3 hidden sm:block">Meta Vinculada</div>
        <div class="col-span-2 hidden md:block">Prazo Alvo</div>
        <div class="col-span-3 text-right">Duração Líq.</div>
      </div>

      <!-- Linhas da Tabela (Sem Zebra Striping - Regra Cap. 5.3) -->
      <div class="divide-y divide-zinc-800/60">
        <div 
          v-for="proj in projects" 
          :key="proj.id"
          class="grid grid-cols-12 gap-4 py-3 px-4 items-center hover:bg-white/[0.03] transition-colors cursor-pointer group"
        >
          <!-- Coluna 1: Nome e Status -->
          <div class="col-span-8 sm:col-span-4 flex items-center gap-2.5 min-w-0">
            <Folder class="w-4 h-4 text-zinc-400 flex-shrink-0 group-hover:text-white transition-colors" />
            <span class="text-sm font-medium text-zinc-100 group-hover:text-white truncate">{{ proj.name }}</span>
          </div>

          <!-- Coluna 2: Meta Vinculada -->
          <div class="col-span-3 hidden sm:flex items-center text-xs text-zinc-400 truncate">
            <span>{{ proj.linkedGoal }}</span>
          </div>

          <!-- Coluna 3: Prazo em Monospace -->
          <div class="col-span-2 hidden md:flex items-center text-xs font-mono text-zinc-400">
            <span>{{ proj.deadline }}</span>
          </div>

          <!-- Coluna 4: Duração Líquida & Barra de Progresso Ultrafina (Cap. 5.3) -->
          <div class="col-span-4 sm:col-span-3 flex items-center justify-end gap-3">
            <div class="flex flex-col items-end gap-1">
              <span class="text-xs font-mono text-zinc-300">
                <strong class="text-white">{{ proj.completedMinutes }}m</strong> / {{ proj.totalMinutes }}m
              </span>
              <!-- Barra de Progresso Horizontal ultrafina (2px de altura) -->
              <div class="h-0.5 w-20 bg-zinc-800 rounded overflow-hidden">
                <div 
                  class="h-full bg-zinc-100 transition-all duration-300"
                  :style="{ width: `${proj.progressPercentage}%` }"
                />
              </div>
            </div>

            <span 
              class="hidden lg:inline-block px-1.5 py-0.5 rounded text-[10px] font-mono uppercase border"
              :class="
              proj.status === 'COMPLETED'
              ? 'bg-zinc-100 text-zinc-950 border-zinc-300 font-semibold'

              : proj.status === 'IN_PROGRESS'
              ? 'bg-zinc-900 text-zinc-200 border-zinc-700'

              : 'bg-zinc-950 text-zinc-500 border-zinc-800'
              "
            >
              {{ proj.status }}
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>