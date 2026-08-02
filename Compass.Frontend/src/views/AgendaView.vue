<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { useCommitmentsStore, type CommitmentItem } from '@/stores/commitmentsStore';
import { useDecisionStore } from '@/stores/decisionStore';
import { useInspectorStore } from '@/stores/inspectorStore'; // 🔥 ARQ-00: Injeção do Inspetor
import TacticalHorizonBar, { type HorizonOption } from '@/components/core/TacticalHorizonBar.vue';
import { isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';
import { 
  Calendar, Clock, PlusCircle, Lock, ArrowRight, 
  Building, Lightbulb, MapPin, Moon, Folder, CalendarCheck, AlertCircle, RefreshCw, CircleDashed 
} from 'lucide-vue-next';
import AllocationPickerModal from '@/components/modals/AllocationPickerModal.vue';

const isAllocationModalOpen = ref(false);

const commitmentsStore = useCommitmentsStore();
const decisionStore = useDecisionStore();
const inspectorStore = useInspectorStore(); // 🔥 ARQ-00: Instância ativada

const currentHorizon = ref<HorizonOption>('today');
const currentTime = ref(new Date());
let timeTracker: number;

onMounted(async () => {
  timeTracker = window.setInterval(() => {
    currentTime.value = new Date();
  }, 60000);

  await Promise.all([
    commitmentsStore.fetchAllActive(),
    decisionStore.fetchNow()
  ]);
});

onUnmounted(() => {
  window.clearInterval(timeTracker);
});

const workHours = computed(() => {
  return { start: '08:00', focus: '08:30', end: '18:00' };
});

const agendaBuckets = computed(() => {
  const now = currentTime.value;
  const todayEndMs = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 59, 59).getTime();

  const counts = { today: 0, tomorrow: 0, '3days': 0, week: 0 };
  const validEvents: CommitmentItem[] = [];
  const unscheduled: CommitmentItem[] = [];

  commitmentsStore.items.forEach(item => {
    if (item.status === 'ARCHIVED' || item.status === 'COMPLETED') return;

    if (!item.startTime) {
      if (item.type === 'EVENT' && currentHorizon.value === 'today') {
         unscheduled.push(item);
      }
      return; 
    }

    const timeMs = new Date(item.startTime).getTime();

    if (timeMs <= todayEndMs) {
      counts.today++;
      if (currentHorizon.value === 'today') validEvents.push(item);
    } 
    else if (timeMs <= todayEndMs + 86400000) {
      counts.tomorrow++;
      if (currentHorizon.value === 'tomorrow') validEvents.push(item);
    } 
    else if (timeMs <= todayEndMs + 86400000 * 3) {
      counts['3days']++;
      if (currentHorizon.value === '3days') validEvents.push(item);
    } 
    else if (timeMs <= todayEndMs + 86400000 * 7) {
      counts.week++;
      if (currentHorizon.value === 'week') validEvents.push(item);
    }
  });

  return { counts, events: validEvents, unscheduled };
});

type TimelineNode = {
  id: string;
  timeMs: number;
  timeLabel: string;
  type: 'SHIFT_START' | 'FOCUS_WINDOW' | 'SHIFT_END' | 'COMMITMENT' | 'EMPTY_STATE';
  data?: CommitmentItem;
};

const timelineNodes = computed(() => {
  const nodes: TimelineNode[] = [];
  const events = agendaBuckets.value.events;

  events.forEach(e => {
    nodes.push({
      id: e.id,
      timeMs: new Date(e.startTime!).getTime(),
      timeLabel: new Date(e.startTime!).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
      type: 'COMMITMENT',
      data: e
    });
  });

  let focusMsAnchor = Date.now();
  
  if (currentHorizon.value === 'today' || currentHorizon.value === 'tomorrow') {
    const base = currentTime.value;
    const offset = currentHorizon.value === 'tomorrow' ? 1 : 0;
    
    const getMs = (timeStr: string) => {
      const [h, m] = timeStr.split(':').map(Number);
      return new Date(base.getFullYear(), base.getMonth(), base.getDate() + offset, h, m).getTime();
    };

    nodes.push({ id: 'shift-start', timeMs: getMs(workHours.value.start), timeLabel: workHours.value.start, type: 'SHIFT_START' });
    nodes.push({ id: 'shift-end', timeMs: getMs(workHours.value.end), timeLabel: workHours.value.end, type: 'SHIFT_END' });
    
    if (currentHorizon.value === 'today') {
      focusMsAnchor = getMs(workHours.value.focus);
      nodes.push({ id: 'focus-window', timeMs: focusMsAnchor, timeLabel: workHours.value.focus, type: 'FOCUS_WINDOW' });
    } else {
      focusMsAnchor = getMs(workHours.value.start) + 1000;
    }
  }

  if (events.length === 0) {
    nodes.push({ id: 'empty-state', timeMs: focusMsAnchor + 1000, timeLabel: '', type: 'EMPTY_STATE' });
  }

  return nodes.sort((a, b) => a.timeMs - b.timeMs);
});

const availableMinutes = computed(() => decisionStore.availableMinutes || decisionStore.availableWindow || 180);
</script>

<template>
  <div class="max-w-4xl mx-auto space-y-6 select-none relative">
    
    <div class="flex items-center justify-between gap-4 pb-4 border-b border-borderbase">
      <div>
        <h1 class="text-2xl font-semibold text-content tracking-tight">Agenda Tática</h1>
        <p class="text-sm text-content-muted mt-1">Visão cronológica de eventos rígidos e tarefas alocadas.</p>
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

    <TacticalHorizonBar v-model="currentHorizon" :counts="agendaBuckets.counts" />

    <div v-if="agendaBuckets.unscheduled.length > 0" class="p-4 rounded-lg bg-status-warning-bg/40 border border-status-warning-border space-y-3 mb-6">
      <div class="flex items-center gap-2 text-xs font-bold text-status-warning uppercase tracking-wider">
        <AlertCircle class="w-4 h-4" /> <span>Eventos Sem Horário Definido</span>
      </div>
      <div class="space-y-2">
        <!-- 🔥 ARQ-00: Eventos órfãos agora são clicáveis -->
        <div 
          v-for="orphan in agendaBuckets.unscheduled" :key="orphan.id" 
          @click="inspectorStore.openInspector(orphan, 'COMMITMENT')"
          class="flex items-center gap-3 text-sm bg-app p-2 rounded border border-borderbase cursor-pointer hover:border-borderfocus transition-colors"
        >
          <Lock class="w-3.5 h-3.5 text-content-muted" />
          <span class="font-medium text-content">{{ orphan.title }}</span>
          <span class="text-xs text-content-muted">{{ orphan.projectName ? `#${orphan.projectName}` : 'Avulso' }}</span>
        </div>
      </div>
    </div>

    <div class="relative pl-16 space-y-6 before:absolute before:left-12 before:top-2 before:bottom-2 before:w-px before:bg-borderbase">
      
      <div v-for="node in timelineNodes" :key="node.id" class="relative flex items-center group">
        
        <span v-if="node.timeLabel" class="absolute -left-16 text-xs font-mono text-content-muted w-12 text-right">
          {{ node.timeLabel }}
        </span>
        
        <div 
          class="absolute -left-4 w-2.5 h-2.5 rounded-full border-2 border-app transition-colors"
          :class="{
            'bg-surface-active': node.type === 'SHIFT_START' || node.type === 'SHIFT_END',
            'bg-content-accent animate-pulse': node.type === 'FOCUS_WINDOW',
            'bg-status-danger-text': node.type === 'COMMITMENT' && node.data?.type === 'EVENT',
            'bg-content': node.type === 'COMMITMENT' && node.data?.type !== 'EVENT',
            'bg-transparent border-none': node.type === 'EMPTY_STATE'
          }"
        />
        
        <div v-if="node.type === 'SHIFT_START'" class="w-full p-3 rounded-lg bg-surface border border-borderbase text-xs font-mono text-content-muted flex items-center justify-between">
          <span class="flex items-center gap-2">
            <Building class="w-3.5 h-3.5" /> Turno Útil {{ currentHorizon === 'today' ? 'Iniciado' : 'Programado' }} ({{ workHours.start }} às {{ workHours.end }})
          </span>
          <span class="px-2 py-0.5 rounded border border-borderbase bg-app text-[10px] font-mono text-content-muted uppercase">{{ currentHorizon }}</span>
        </div>

        <div v-else-if="node.type === 'FOCUS_WINDOW'" class="w-full p-5 rounded-lg border border-dashed border-borderbase bg-app hover:border-borderfocus transition-all group flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
          <div class="space-y-1.5">
            <div class="flex items-center gap-2">
              <div class="w-1.5 h-6 rounded-full bg-borderhighlight" />
              <h3 class="text-sm font-mono font-semibold text-content tracking-tight">JANELA DE FOCO DISPONÍVEL ({{ availableMinutes }}m)</h3>
            </div>
            <p class="text-xs text-content-muted flex items-center gap-1.5"><Lightbulb class="w-3.5 h-3.5" /> Dica: Tempo livre de reuniões para alocar tarefas ativas.</p>
          </div>
          <button @click="isAllocationModalOpen = true" class="inline-flex items-center gap-2 px-3 py-1.5 rounded bg-surface hover:bg-surface-active border border-borderbase text-xs font-medium text-content hover:text-content-accent transition-colors cursor-pointer flex-shrink-0 shadow-sm">
            <ArrowRight class="w-3.5 h-3.5" /> <span>Alocar Tarefa Pendente</span>
          </button>
        </div>

        <!-- 🔥 ARQ-00: Abertura do Inspetor Universal via Click no TimelineNode -->
        <div 
          v-else-if="node.type === 'COMMITMENT' && node.data" 
          @click="inspectorStore.openInspector(node.data, 'COMMITMENT')"
          class="w-full p-4 rounded-r-lg border-y border-r border-borderbase space-y-2 shadow-sm hover:shadow-md transition-shadow cursor-pointer"
          :class="[
            node.data.type === 'EVENT' ? 'border-l-4 border-l-status-danger-border bg-surface' : 'border-l-4 border-l-content bg-app'
          ]"
        >
          <div class="flex items-center justify-between gap-2">
            <div class="flex items-center gap-2">
              <Lock v-if="node.data.type === 'EVENT'" class="w-3.5 h-3.5 text-status-danger-text" />
              <CircleDashed v-else-if="node.data.type === 'TASK'" class="w-3.5 h-3.5 text-content" />
              <RefreshCw v-else-if="node.data.type === 'HABIT'" class="w-3.5 h-3.5 text-content" />
              <h4 class="text-sm font-semibold text-content truncate">{{ node.data.title }}</h4>
            </div>
            <span 
              class="text-[10px] font-mono uppercase px-1.5 py-0.5 rounded"
              :class="node.data.type === 'EVENT' ? 'text-status-danger-text border border-status-danger-border bg-status-danger-bg' : 'text-content border border-borderfocus bg-surface-active'"
            >
              {{ node.data.type === 'EVENT' ? 'Hard Blocker' : 'Timeblocked' }}
            </span>
          </div>
          <div class="flex items-center gap-4 text-xs font-mono text-content-muted pt-1">
            <span v-if="node.data.locationOrLink" class="flex items-center gap-1.5"><MapPin class="w-3 h-3" /> {{ node.data.locationOrLink }}</span>
            <span class="flex items-center gap-1.5"><Clock class="w-3 h-3" /> {{ node.data.estimatedDurationMinutes }}m</span>
            <span v-if="node.data.projectName" class="flex items-center gap-1.5"><Folder class="w-3 h-3" /> {{ node.data.projectName }}</span>
          </div>
        </div>

        <div v-else-if="node.type === 'EMPTY_STATE'" class="w-full p-8 rounded-lg border border-dashed border-borderbase bg-app/40 text-center space-y-3">
          <CalendarCheck class="w-8 h-8 text-content-accent mx-auto stroke-1" />
          <div class="space-y-1">
            <h4 class="text-sm font-semibold text-content">Nenhum compromisso agendado</h4>
            <p class="text-xs text-content-muted max-w-sm mx-auto">Sua linha do tempo está livre para este dia.</p>
          </div>
        </div>

        <div v-else-if="node.type === 'SHIFT_END'" class="w-full p-3 rounded-lg bg-surface border border-borderbase text-xs font-mono text-content-muted flex items-center gap-2">
          <Moon class="w-3.5 h-3.5" /> Encerramento da Janela de Foco
        </div>

      </div>
    </div>
    
    <AllocationPickerModal 
      :isOpen="isAllocationModalOpen" 
      @close="isAllocationModalOpen = false" 
    />
  </div>
</template>