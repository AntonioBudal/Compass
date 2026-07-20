<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { useCommitmentsStore } from '@/stores/commitmentsStore';
import { useDecisionStore } from '@/stores/decisionStore';
import { isQuickCaptureOpen, isCommandBarOpen } from '@/composables/useKeyboardShortcuts';
import { 
  Calendar, Clock, PlusCircle, Lock, ArrowRight, CheckCircle2, 
  Building, Lightbulb, MapPin, Moon, Folder 
} from 'lucide-vue-next';

const commitmentsStore = useCommitmentsStore();
const decisionStore = useDecisionStore();

onMounted(async () => {
  await Promise.all([
    commitmentsStore.fetchAllActive(),
    decisionStore.fetchNow()
  ]);
});

const hardBlockers = computed(() => {
  return commitmentsStore.items.filter(item => 
    item.type === 'EVENT' && item.status !== 'ARCHIVED'
  );
});

const availableMinutes = computed(() => decisionStore.availableMinutes || 180);
</script>

<template>
  <div class="max-w-4xl mx-auto space-y-8 select-none">
    <div class="flex items-center justify-between gap-4 pb-4 border-b border-zinc-800">
      <div>
        <h1 class="text-2xl font-semibold text-zinc-100 tracking-tight">Agenda & Hard Blockers</h1>
        <p class="text-sm text-zinc-400 mt-1">Visão cronológica contínua intercalando compromissos rígidos e janelas de foco líquidas.</p>
      </div>
      <button 
        @click="isQuickCaptureOpen = true"
        class="inline-flex items-center gap-2 px-3 py-1.5 rounded-tactic bg-zinc-900 hover:bg-zinc-800 border border-zinc-800 text-xs font-medium text-zinc-300 hover:text-white transition-all shadow-sm cursor-pointer"
      >
        <PlusCircle class="w-3.5 h-3.5 text-zinc-300" />
        <span>Novo Evento</span>
        <kbd class="px-1 text-[10px] font-mono bg-zinc-950 rounded border border-zinc-800 text-zinc-500">C</kbd>
      </button>
    </div>

    <!-- LINHA DO TEMPO VERTICAL TÁTICA -->
    <div class="relative pl-16 space-y-6 before:absolute before:left-12 before:top-2 before:bottom-2 before:w-px before:bg-zinc-700/60/80">
      
      <!-- Marco 1 -->
      <div class="relative flex items-center group">
        <span class="absolute -left-16 text-xs font-mono text-zinc-500 w-12 text-right">08:00</span>
        <div class="absolute -left-4 w-2.5 h-2.5 rounded-full bg-zinc-800 border-2 border-zinc-950" />
        <div class="w-full p-3 rounded-lg bg-zinc-900/40 border border-zinc-800/60 text-xs font-mono text-zinc-400 flex items-center justify-between">
          <span class="flex items-center gap-2"><Building class="w-3.5 h-3.5" /> Turno Útil Iniciado (08:00 às 18:00)</span>
          <span class="px-2 py-0.5 rounded border border-zinc-800 bg-zinc-950 text-[10px] font-mono text-zinc-500">
          ONLINE
          </span>
        </div>
      </div>

      <!-- Marco 2 -->
      <div class="relative flex items-center">
        <span class="absolute -left-16 text-xs font-mono text-zinc-500/80 w-12 text-right">08:30</span>
        <div class="absolute -left-4 w-2.5 h-2.5 rounded-full bg-zinc-500 animate-pulse border-2 border-zinc-950" />
        
        <div class="w-full p-5 rounded-lg border border-dashed border-zinc-800 bg-zinc-950/60 hover:border-zinc-700 transition-all group flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
          <div class="space-y-1.5">
            <div class="flex items-center gap-2">
              <div class="w-1.5 h-6 rounded-full bg-zinc-200" />
              <h3 class="text-sm font-mono font-semibold text-zinc-400 tracking-tight">JANELA DE FOCO DISPONÍVEL ({{ availableMinutes }}m)</h3>
            </div>
            <p class="text-xs text-zinc-500 flex items-center gap-1.5">
              <Lightbulb class="w-3.5 h-3.5" /> Dica: Tempo suficiente para 3 a 4 tarefas de manutenção.
            </p>
          </div>

          <button 
            @click="isCommandBarOpen = true"
            class="inline-flex items-center gap-2 px-3 py-1.5 rounded bg-zinc-900 hover:bg-zinc-800 border border-zinc-800 text-xs font-medium text-zinc-300 hover:text-white transition-colors cursor-pointer flex-shrink-0"
          >
            <ArrowRight class="w-3.5 h-3.5 text-zinc-400" />
            <span>Alocar tarefa</span>
            <kbd class="px-1 text-[10px] font-mono bg-zinc-950 rounded text-zinc-500">Cmd+K</kbd>
          </button>
        </div>
      </div>

      <!-- Marco 3 -->
      <template v-if="hardBlockers.length > 0">
        <div v-for="(event, idx) in hardBlockers" :key="event.id" class="relative flex items-start">
          <span class="absolute -left-16 top-3 text-xs font-mono text-zinc-400 w-12 text-right">{{ event.startTime || '14:00' }}</span>
          <div class="absolute -left-4 top-4 w-2.5 h-2.5 rounded-full bg-rose-500 border-2 border-zinc-950" />
          
          <div class="w-full p-4 rounded-r-lg border-l-4 border-l-rose-500 bg-zinc-900 border-y border-r border-zinc-800/80 space-y-2">
            <div class="flex items-center justify-between gap-2">
              <div class="flex items-center gap-2">
                <Lock class="w-3.5 h-3.5 text-rose-300" />
                <h4 class="text-sm font-semibold text-zinc-100 truncate">{{ event.title }}</h4>
              </div>
              <span class="text-[10px] font-mono uppercase text-rose-300 border border-rose-500/40 bg-zinc-950 px-1.5 py-0.5 rounded">Hard Blocker</span>
            </div>

            <div class="flex items-center gap-4 text-xs font-mono text-zinc-400 pt-1">
              <span v-if="event.locationOrLink" class="flex items-center gap-1.5"><MapPin class="w-3 h-3" /> {{ event.locationOrLink }}</span>
              <span class="flex items-center gap-1.5"><Clock class="w-3 h-3" /> {{ event.estimatedDurationMinutes }}m</span>
              <span v-if="event.projectName" class="flex items-center gap-1.5"><Folder class="w-3 h-3" /> {{ event.projectName }}</span>
            </div>
          </div>
        </div>
      </template>

      <!-- Marco 4 -->
      <div class="relative flex items-center">
        <span class="absolute -left-16 text-xs font-mono text-zinc-500 w-12 text-right">18:00</span>
        <div class="absolute -left-4 w-2.5 h-2.5 rounded-full bg-zinc-800 border-2 border-zinc-950" />
        <div class="w-full p-3 rounded-lg bg-zinc-900/40 border border-zinc-800/60 text-xs font-mono text-zinc-500 flex items-center gap-2">
          <Moon class="w-3.5 h-3.5" /> Encerramento da Janela de Foco
        </div>
      </div>
    </div>
  </div>
</template>