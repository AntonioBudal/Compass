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
    <div class="flex items-center justify-between gap-4 pb-4 border-b border-borderbase">
      <div>
        <h1 class="text-2xl font-semibold text-content tracking-tight">Agenda & Hard Blockers</h1>
        <p class="text-sm text-content-muted mt-1">Visão cronológica contínua intercalando compromissos rígidos e janelas de foco líquidas.</p>
      </div>
      <button 
        @click="isQuickCaptureOpen = true"
        class="inline-flex items-center gap-2 px-3 py-1.5 rounded-tactic bg-surface hover:bg-surface-hover border border-borderbase text-xs font-medium text-content-muted hover:text-content transition-all shadow-sm cursor-pointer"
      >
        <PlusCircle class="w-3.5 h-3.5 text-content-muted" />
        <span>Novo Evento</span>
        <kbd class="px-1 text-[10px] font-mono bg-app rounded border border-borderbase text-content-muted">C</kbd>
      </button>
    </div>

    <!-- LINHA DO TEMPO VERTICAL TÁTICA -->
    <div class="relative pl-16 space-y-6 before:absolute before:left-12 before:top-2 before:bottom-2 before:w-px before:bg-borderbase">
      
      <!-- Marco 1 -->
      <div class="relative flex items-center group">
        <span class="absolute -left-16 text-xs font-mono text-content-muted w-12 text-right">08:00</span>
        <div class="absolute -left-4 w-2.5 h-2.5 rounded-full bg-surface-active border-2 border-app" />
        <div class="w-full p-3 rounded-lg bg-surface border border-borderbase text-xs font-mono text-content-muted flex items-center justify-between">
          <span class="flex items-center gap-2"><Building class="w-3.5 h-3.5" /> Turno Útil Iniciado (08:00 às 18:00)</span>
          <span class="px-2 py-0.5 rounded border border-borderbase bg-app text-[10px] font-mono text-content-muted">
          ONLINE
          </span>
        </div>
      </div>

      <!-- Marco 2 -->
      <div class="relative flex items-center">
        <span class="absolute -left-16 text-xs font-mono text-content-muted w-12 text-right">08:30</span>
        <div class="absolute -left-4 w-2.5 h-2.5 rounded-full bg-content-accent animate-pulse border-2 border-app" />
        
        <div class="w-full p-5 rounded-lg border border-dashed border-borderbase bg-app hover:border-borderfocus transition-all group flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
          <div class="space-y-1.5">
            <div class="flex items-center gap-2">
              <div class="w-1.5 h-6 rounded-full bg-borderhighlight" />
              <h3 class="text-sm font-mono font-semibold text-content tracking-tight">JANELA DE FOCO DISPONÍVEL ({{ availableMinutes }}m)</h3>
            </div>
            <p class="text-xs text-content-muted flex items-center gap-1.5">
              <Lightbulb class="w-3.5 h-3.5" /> Dica: Tempo suficiente para 3 a 4 tarefas de manutenção.
            </p>
          </div>

          <button 
            @click="isCommandBarOpen = true"
            class="inline-flex items-center gap-2 px-3 py-1.5 rounded bg-surface hover:bg-surface-hover border border-borderbase text-xs font-medium text-content-muted hover:text-content transition-colors cursor-pointer flex-shrink-0"
          >
            <ArrowRight class="w-3.5 h-3.5 text-content-muted" />
            <span>Alocar tarefa</span>
            <kbd class="px-1 text-[10px] font-mono bg-app rounded text-content-muted">Cmd+K</kbd>
          </button>
        </div>
      </div>

      <!-- Marco 3 -->
      <template v-if="hardBlockers.length > 0">
        <div v-for="(event, idx) in hardBlockers" :key="event.id" class="relative flex items-start">
          <span class="absolute -left-16 top-3 text-xs font-mono text-content-muted w-12 text-right">{{ event.startTime || '14:00' }}</span>
          <div class="absolute -left-4 top-4 w-2.5 h-2.5 rounded-full bg-status-danger-text border-2 border-app" />
          
          <div class="w-full p-4 rounded-r-lg border-l-4 border-l-status-danger-border bg-surface border-y border-r border-borderbase space-y-2">
            <div class="flex items-center justify-between gap-2">
              <div class="flex items-center gap-2">
                <Lock class="w-3.5 h-3.5 text-status-danger-text" />
                <h4 class="text-sm font-semibold text-content truncate">{{ event.title }}</h4>
              </div>
              <span class="text-[10px] font-mono uppercase text-status-danger-text border border-status-danger-border bg-status-danger-bg px-1.5 py-0.5 rounded">Hard Blocker</span>
            </div>

            <div class="flex items-center gap-4 text-xs font-mono text-content-muted pt-1">
              <span v-if="event.locationOrLink" class="flex items-center gap-1.5"><MapPin class="w-3 h-3" /> {{ event.locationOrLink }}</span>
              <span class="flex items-center gap-1.5"><Clock class="w-3 h-3" /> {{ event.estimatedDurationMinutes }}m</span>
              <span v-if="event.projectName" class="flex items-center gap-1.5"><Folder class="w-3 h-3" /> {{ event.projectName }}</span>
            </div>
          </div>
        </div>
      </template>

      <!-- Marco 4 -->
      <div class="relative flex items-center">
        <span class="absolute -left-16 text-xs font-mono text-content-muted w-12 text-right">18:00</span>
        <div class="absolute -left-4 w-2.5 h-2.5 rounded-full bg-surface-active border-2 border-app" />
        <div class="w-full p-3 rounded-lg bg-surface border border-borderbase text-xs font-mono text-content-muted flex items-center gap-2">
          <Moon class="w-3.5 h-3.5" /> Encerramento da Janela de Foco
        </div>
      </div>
    </div>
  </div>
</template>