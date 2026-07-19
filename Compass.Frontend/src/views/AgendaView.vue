<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { useCommitmentsStore } from '@/stores/commitmentsStore';
import { useDecisionStore } from '@/stores/decisionStore';
import { isQuickCaptureOpen, isCommandBarOpen } from '@/composables/useKeyboardShortcuts';
import { Calendar, Clock, PlusCircle, Lock, Sparkles, ArrowRight, CheckCircle2 } from 'lucide-vue-next';

const commitmentsStore = useCommitmentsStore();
const decisionStore = useDecisionStore();

onMounted(async () => {
  await Promise.all([
    commitmentsStore.fetchAllActive(),
    decisionStore.fetchNow()
  ]);
});

// Filtra compromissos rígidos do dia atual ou agendados
const hardBlockers = computed(() => {
  return commitmentsStore.items.filter(item => 
    item.type === 'EVENT' && item.status !== 'ARCHIVED'
  );
});

// Minutos livres calculados pelo motor .NET 10
const availableMinutes = computed(() => decisionStore.availableMinutes || 180);

const openNewEventModal = () => {
  isQuickCaptureOpen.value = true;
};

const openAllocationSearch = () => {
  isCommandBarOpen.value = true;
};
</script>

<template>
  <div class="max-w-4xl mx-auto space-y-8 select-none">
    <!-- Cabeçalho da Agenda Tática -->
    <div class="flex items-center justify-between gap-4 pb-4 border-b border-zinc-800">
      <div>
        <h1 class="text-2xl font-semibold text-zinc-100 tracking-tight">
          Agenda & Hard Blockers
        </h1>
        <p class="text-sm text-zinc-400 mt-1">
          Visão cronológica contínua intercalando compromissos rígidos e janelas de foco líquidas.
        </p>
      </div>

      <button 
        @click="openNewEventModal"
        class="inline-flex items-center gap-2 px-3 py-1.5 rounded-tactic bg-zinc-900 hover:bg-zinc-800 border border-zinc-800 text-xs font-medium text-zinc-300 hover:text-white transition-all shadow-sm"
      >
        <PlusCircle class="w-3.5 h-3.5 text-indigo-400" />
        <span>Novo Evento</span>
        <kbd class="px-1 text-[10px] font-mono bg-zinc-950 rounded border border-zinc-800 text-zinc-500">C</kbd>
      </button>
    </div>

    <!-- LINHA DO TEMPO VERTICAL TÁTICA (Cap. 5.2) -->
    <div class="relative pl-16 space-y-6 before:absolute before:left-12 before:top-2 before:bottom-2 before:w-px before:bg-zinc-800/80">
      
      <!-- Marco 1: Início do Turno -->
      <div class="relative flex items-center group">
        <span class="absolute -left-16 text-xs font-mono text-zinc-500 w-12 text-right">08:00</span>
        <div class="absolute -left-4 w-2.5 h-2.5 rounded-full bg-zinc-800 border-2 border-zinc-950" />
        <div class="w-full p-3 rounded-lg bg-zinc-900/40 border border-zinc-800/60 text-xs font-mono text-zinc-400 flex items-center justify-between">
          <span>🏢 Turno Útil Iniciado (Schedule configurado: 08:00 às 18:00)</span>
          <span class="text-zinc-600">Sync OK</span>
        </div>
      </div>

      <!-- Marco 2: Janela de Foco Disponível Principal ($M_tempo$) -->
      <div class="relative flex items-center">
        <span class="absolute -left-16 text-xs font-mono text-emerald-500/80 w-12 text-right">08:30</span>
        <div class="absolute -left-4 w-2.5 h-2.5 rounded-full bg-emerald-500 animate-pulse border-2 border-zinc-950" />
        
        <!-- Contêiner de Janela Livre com Borda Tracejada (Cap. 5.2) -->
        <div class="w-full p-5 rounded-lg border border-dashed border-zinc-800 bg-zinc-950/60 hover:border-zinc-700 transition-all group flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
          <div class="space-y-1">
            <div class="flex items-center gap-2">
              <span class="w-2 h-2 rounded-full bg-emerald-400" />
              <h3 class="text-sm font-mono font-semibold text-emerald-400 tracking-tight">
                JANELA DE FOCO DISPONÍVEL ({{ availableMinutes }}m líquidos)
              </h3>
            </div>
            <p class="text-xs text-zinc-500">
              💡 Dica do Motor: Tempo suficiente para 3 a 4 tarefas de profundidade técnica.
            </p>
          </div>

          <button 
            @click="openAllocationSearch"
            class="inline-flex items-center gap-2 px-3 py-1.5 rounded bg-zinc-900 hover:bg-zinc-800 border border-zinc-800 text-xs font-medium text-zinc-300 hover:text-white transition-colors flex-shrink-0"
          >
            <Sparkles class="w-3.5 h-3.5 text-emerald-400" />
            <span>+ Alocar tarefa aqui</span>
            <kbd class="px-1 text-[10px] font-mono bg-zinc-950 rounded text-zinc-500">Cmd+K</kbd>
          </button>
        </div>
      </div>

      <!-- Marco 3: Hard Blockers (Eventos Agendados) -->
      <template v-if="hardBlockers.length > 0">
        <div 
          v-for="(event, idx) in hardBlockers" 
          :key="event.id"
          class="relative flex items-start"
        >
          <span class="absolute -left-16 top-3 text-xs font-mono text-zinc-400 w-12 text-right">
            {{ event.startTime || '14:00' }}
          </span>
          <div class="absolute -left-4 top-4 w-2.5 h-2.5 rounded-full bg-rose-500 border-2 border-zinc-950" />
          
          <!-- Card de Hard Blocker com Borda Lateral Acentuada (Cap. 5.2) -->
          <div class="w-full p-4 rounded-r-lg border-l-4 border-l-rose-500 bg-zinc-900/90 border-y border-r border-zinc-800/80 shadow-md space-y-2">
            <div class="flex items-center justify-between gap-2">
              <div class="flex items-center gap-2">
                <Lock class="w-3.5 h-3.5 text-rose-400 flex-shrink-0" />
                <h4 class="text-sm font-semibold text-zinc-100 truncate">{{ event.title }}</h4>
              </div>
              <span class="text-[10px] font-mono uppercase bg-rose-500/10 text-rose-400 border border-rose-500/20 px-1.5 py-0.5 rounded">
                Hard Blocker
              </span>
            </div>

            <div class="flex items-center gap-4 text-xs font-mono text-zinc-400">
              <span v-if="event.locationOrLink">📍 {{ event.locationOrLink }}</span>
              <span>⏱️ Duração: {{ event.estimatedDurationMinutes }}m</span>
              <span v-if="event.projectName">📁 {{ event.projectName }}</span>
            </div>
          </div>
        </div>
      </template>

      <!-- Estado de Fallback se não houver reuniões -->
      <div v-else class="relative flex items-center">
        <span class="absolute -left-16 text-xs font-mono text-zinc-600 w-12 text-right">14:00</span>
        <div class="absolute -left-4 w-2.5 h-2.5 rounded-full bg-zinc-800 border-2 border-zinc-950" />
        <div class="w-full p-4 rounded-lg bg-zinc-900/20 border border-zinc-800/40 text-xs font-mono text-zinc-500 flex items-center justify-between">
          <span>🔒 Nenhum Hard Blocker adicional agendado para o período da tarde.</span>
          <CheckCircle2 class="w-4 h-4 text-emerald-500/60" />
        </div>
      </div>

      <!-- Marco 4: Encerramento do Turno -->
      <div class="relative flex items-center">
        <span class="absolute -left-16 text-xs font-mono text-zinc-500 w-12 text-right">18:00</span>
        <div class="absolute -left-4 w-2.5 h-2.5 rounded-full bg-zinc-800 border-2 border-zinc-950" />
        <div class="w-full p-3 rounded-lg bg-zinc-900/40 border border-zinc-800/60 text-xs font-mono text-zinc-500">
          🌙 Encerramento da Janela de Foco Recomendada
        </div>
      </div>

    </div>
  </div>
</template>