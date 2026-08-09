<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { useCommitmentsStore, type CommitmentItem } from '@/stores/commitmentsStore';
import { useDecisionStore } from '@/stores/decisionStore';
import { useInspectorStore } from '@/stores/inspectorStore';
import { CompassApi } from '@/services/api';
import { useTimeResize } from '@/composables/useTimeResize';
import { isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';
import { useToastStore } from '@/stores/toastStore'; //  IMPORT DO TOAST
import type { HorizonOption } from '@/components/core/TacticalHorizonBar.vue';

import AgendaHeader from '@/components/agenda/AgendaHeader.vue';
import AgendaBacklog from '@/components/agenda/AgendaBacklog.vue';
import AgendaTimeGrid from '@/components/agenda/AgendaTimeGrid.vue';
import AgendaRestBlocks from '@/components/agenda/AgendaRestBlocks.vue';
import AgendaFreeBlocks from '@/components/agenda/AgendaFreeBlocks.vue';
import AgendaCommitmentBlock from '@/components/agenda/AgendaCommitmentBlock.vue';
import AgendaGhostBlock from '@/components/agenda/AgendaGhostBlock.vue';
import AgendaConflictModal from '@/components/agenda/AgendaConflictModal.vue';

const PIXELS_PER_MINUTE = 2;
const MINUTES_IN_DAY = 1440;
const GRID_INTERVAL_MINUTES = 30;

const commitmentsStore = useCommitmentsStore();
const decisionStore = useDecisionStore();
const inspectorStore = useInspectorStore();
const toastStore = useToastStore(); //  INSTÂNCIA DO TOAST

const currentHorizon = ref<HorizonOption>('today');
const currentTime = ref(new Date());
let timeTracker: number;

const canvasRef = ref<HTMLElement | null>(null);
const baseSchedule = ref({ workStart: '08:00', workEnd: '18:00', isActive: true });

const dragEnabledId = ref<string | null>(null);
const ghostState = ref<{ isActive: boolean; startMin: number; durationMin: number; title: string; type: string; }>({ isActive: false, startMin: 0, durationMin: 30, title: '', type: '' });
const conflictModal = ref<{ isOpen: boolean; item: CommitmentItem | null; dropMinutes: number; availableMinutes: number; newStartDate: Date | null; }>({ isOpen: false, item: null, dropMinutes: 0, availableMinutes: 0, newStartDate: null });

onMounted(async () => {
  timeTracker = window.setInterval(() => { currentTime.value = new Date(); }, 60000);
  const [scheduleData] = await Promise.all([
    CompassApi.fetchTodaySchedule().catch(() => baseSchedule.value),
    commitmentsStore.fetchAllActive(),
    decisionStore.fetchNow()
  ]);
  if (scheduleData) baseSchedule.value = scheduleData;

  //  UX: Aviso Transparente sobre Tarefas no Backlog
  if (agendaBuckets.value.unscheduled.length > 0) {
    toastStore.showToast(
      'Algumas tarefas estão sem horário agendado e foram movidas para o Drag & Drop. Arraste-as novamente para a Agenda para definir um horário.',
      'error'
    );
  }
});

onUnmounted(() => window.clearInterval(timeTracker));

const getMinutesFromMidnight = (date: Date) => date.getHours() * 60 + date.getMinutes();
const timeStringToMinutes = (timeStr: string) => { const [h, m] = timeStr.split(':').map(Number); return h * 60 + m; };

const nowLineTop = computed(() => getMinutesFromMidnight(currentTime.value) * PIXELS_PER_MINUTE);

const restBlocks = computed(() => {
  if (!baseSchedule.value.isActive) return [];
  const startMin = timeStringToMinutes(baseSchedule.value.workStart);
  const endMin = timeStringToMinutes(baseSchedule.value.workEnd);
  return [{ start: 0, end: startMin, label: 'Madrugada / Descanso' }, { start: endMin, end: MINUTES_IN_DAY, label: 'Noite / Descanso' }];
});

const agendaBuckets = computed(() => {
  const now = currentTime.value;
  const getHorizonBounds = (horizon: HorizonOption) => {
    const baseDate = new Date(now);
    if (horizon === 'tomorrow') baseDate.setDate(baseDate.getDate() + 1);
    else if (horizon === '3days') baseDate.setDate(baseDate.getDate() + 3);
    else if (horizon === 'week') baseDate.setDate(baseDate.getDate() + 7);
    baseDate.setHours(0, 0, 0, 0);
    return { startMs: baseDate.getTime(), endMs: baseDate.getTime() + 86399999 };
  };

  const bounds = getHorizonBounds(currentHorizon.value);
  const scheduled: (CommitmentItem & { startMin: number, endMin: number })[] = [];
  const unscheduled: CommitmentItem[] = [];
  const allKnownEntities = Object.values(commitmentsStore.entities);

  allKnownEntities.forEach(item => {
    if (item.status === 'ARCHIVED' || item.status === 'COMPLETED') return;
    if (!item.startTime) { if (currentHorizon.value === 'today') unscheduled.push(item); return; }
    
    const timeMs = new Date(item.startTime).getTime();
    if (timeMs >= bounds.startMs && timeMs <= bounds.endMs) {
      const startMin = getMinutesFromMidnight(new Date(item.startTime));
      scheduled.push({ ...item, startMin, endMin: startMin + (item.estimatedDurationMinutes || 30) });
    }
  });
  return { scheduled, unscheduled };
});

const freeBlocks = computed(() => {
  const blocks: { startMin: number, endMin: number, duration: number }[] = [];
  let currentStart = 0;
  const sortedEvents = [...agendaBuckets.value.scheduled].sort((a, b) => a.startMin - b.startMin);
  sortedEvents.forEach(evt => {
    if (evt.startMin > currentStart) blocks.push({ startMin: currentStart, endMin: evt.startMin, duration: evt.startMin - currentStart });
    if (evt.endMin > currentStart) currentStart = evt.endMin;
  });
  if (currentStart < MINUTES_IN_DAY) blocks.push({ startMin: currentStart, endMin: MINUTES_IN_DAY, duration: MINUTES_IN_DAY - currentStart });
  return blocks.filter(b => b.duration >= 15);
});

const { resizingId, previewDuration, startResize } = useTimeResize(PIXELS_PER_MINUTE, async (id, newDuration) => {
  const target = commitmentsStore.entities[id];
  if (!target) return;
  const safePayload: any = { 
    title: target.title || 'Sem Título', projectId: target.projectId || null,
    estimatedDurationMinutes: newDuration > 0 ? newDuration : 15, energyRequired: target.energyRequired > 0 ? target.energyRequired : 2,
    startTime: target.startTime 
  };
  if (target.type === 'HABIT') safePayload.cronExpression = target.cronExpression || '0 8 * * *';
  if (target.type === 'TASK') safePayload.deadline = target.deadline;
  await commitmentsStore.updateCommitment(id, safePayload);
});

const getAvailableTimeAt = (startMin: number, ignoreId: string) => {
  const subsequentEvents = agendaBuckets.value.scheduled.filter(e => e.id !== ignoreId && e.startMin >= startMin).sort((a, b) => a.startMin - b.startMin);
  if (subsequentEvents.length === 0) return MINUTES_IN_DAY - startMin;
  return subsequentEvents[0].startMin - startMin; 
};

//  FUNÇÃO NOVA: Tratamento do Drop Reverso
const handleDropToBacklog = async (itemId: string) => {
  const targetItem = commitmentsStore.entities[itemId];
  if (!targetItem || !targetItem.startTime) return; 

  const safePayload: any = {
    title: targetItem.title || 'Sem Título',
    projectId: targetItem.projectId || null,
    estimatedDurationMinutes: targetItem.estimatedDurationMinutes > 0 ? targetItem.estimatedDurationMinutes : 15,
    energyRequired: targetItem.energyRequired > 0 ? targetItem.energyRequired : 2,
    startTime: null // Reseta o horário
  };

  if (targetItem.type === 'HABIT') safePayload.cronExpression = targetItem.cronExpression || '0 8 * * *';
  if (targetItem.type === 'TASK') safePayload.deadline = targetItem.deadline;

  await commitmentsStore.updateCommitment(itemId, safePayload);
};

const handleDragStart = (event: DragEvent, item: CommitmentItem) => {
  if (event.dataTransfer) {
    event.dataTransfer.setData('text/plain', item.id);
    event.dataTransfer.effectAllowed = 'move';
    ghostState.value = { isActive: true, startMin: 0, durationMin: item.estimatedDurationMinutes > 0 ? item.estimatedDurationMinutes : 30, title: item.title, type: item.type };
    setTimeout(() => { if(event.target) (event.target as HTMLElement).style.opacity = '0.4'; }, 0);
  }
};

const handleDragOver = (event: DragEvent) => {
  if (!ghostState.value.isActive || !canvasRef.value) return;
  const canvasRect = canvasRef.value.getBoundingClientRect();
  const dropY = event.clientY - canvasRect.top; 
  let dropMinutes = Math.floor(dropY / PIXELS_PER_MINUTE);
  dropMinutes = Math.round(dropMinutes / 15) * 15; 
  if (dropMinutes < 0) dropMinutes = 0;
  if (dropMinutes + ghostState.value.durationMin > MINUTES_IN_DAY) dropMinutes = MINUTES_IN_DAY - ghostState.value.durationMin;
  ghostState.value.startMin = dropMinutes;
};

const handleDragEnd = (event: DragEvent) => {
  if(event.target) (event.target as HTMLElement).style.opacity = '1';
  ghostState.value.isActive = false; 
};

const handleDropOnCanvas = async (event: DragEvent) => {
  const itemId = event.dataTransfer?.getData('text/plain');
  ghostState.value.isActive = false; 
  if (!itemId || !canvasRef.value) return;
  const targetItem = commitmentsStore.entities[itemId];
  if (!targetItem) return;

  const canvasRect = canvasRef.value.getBoundingClientRect();
  let dropMinutes = Math.round(Math.floor((event.clientY - canvasRect.top) / PIXELS_PER_MINUTE) / 15) * 15;
  if (dropMinutes < 0) dropMinutes = 0;
  if (dropMinutes >= MINUTES_IN_DAY) dropMinutes = MINUTES_IN_DAY - 15;

  const newStartDate = new Date(currentTime.value);
  newStartDate.setHours(Math.floor(dropMinutes / 60), dropMinutes % 60, 0, 0);

  const availableMin = getAvailableTimeAt(dropMinutes, targetItem.id);
  const requestedMin = targetItem.estimatedDurationMinutes > 0 ? targetItem.estimatedDurationMinutes : 30;

  if (requestedMin > availableMin && availableMin > 0) {
    conflictModal.value = { isOpen: true, item: targetItem, dropMinutes, availableMinutes: availableMin, newStartDate };
    return;
  }
  await applyDrop(targetItem, newStartDate, requestedMin);
};

const applyDrop = async (item: CommitmentItem, startDate: Date, duration: number) => {
  const safePayload: any = {
    title: item.title || 'Sem Título', projectId: item.projectId || null,
    estimatedDurationMinutes: duration, energyRequired: item.energyRequired > 0 ? item.energyRequired : 2,
    startTime: startDate.toISOString() 
  };
  if (item.type === 'HABIT') safePayload.cronExpression = item.cronExpression || '0 8 * * *';
  if (item.type === 'TASK') safePayload.deadline = item.deadline;

  await commitmentsStore.updateCommitment(item.id, safePayload);
  conflictModal.value.isOpen = false;
};

const resolveConflict = async (resolution: 'REDUCE' | 'OVERLAP') => {
  const { item, newStartDate, availableMinutes } = conflictModal.value;
  if (!item || !newStartDate) return;
  if (resolution === 'REDUCE') await applyDrop(item, newStartDate, availableMinutes);
  else await applyDrop(item, newStartDate, item.estimatedDurationMinutes);
};
</script>

<template>
  <div class="max-w-5xl mx-auto space-y-6 select-none h-full flex flex-col relative">
    
    <!-- 1. HEADER -->
    <AgendaHeader @open-capture="isQuickCaptureOpen = true" />

    <!-- 2. BACKLOG (DRAG SOURCE & DROP ZONE) -->
    <AgendaBacklog 
      :items="agendaBuckets.unscheduled" 
      @dragstart="handleDragStart" 
      @dragend="handleDragEnd" 
      @drop-to-backlog="handleDropToBacklog"
    />

    <!-- 3. CANVAS ABSOLUTO (EIXO Y MATEMÁTICO) -->
    <div class="flex-1 overflow-y-auto pr-2 pt-8 custom-scrollbar relative border border-borderbase rounded-xl bg-app shadow-inner">
      <div 
        ref="canvasRef"
        class="relative w-full" 
        :style="{ height: `${MINUTES_IN_DAY * PIXELS_PER_MINUTE}px` }"
        @dragover.prevent="handleDragOver"
        @dragenter.prevent
        @drop="handleDropOnCanvas"
      >
        <!-- Fundos e Grids Visuais -->
        <AgendaRestBlocks :blocks="restBlocks" :pixels-per-minute="PIXELS_PER_MINUTE" />
        
        <AgendaTimeGrid 
          :pixels-per-minute="PIXELS_PER_MINUTE" 
          :grid-interval-minutes="GRID_INTERVAL_MINUTES" 
        />
        
        <AgendaFreeBlocks 
          :blocks="freeBlocks" 
          :pixels-per-minute="PIXELS_PER_MINUTE" 
          @allocate="isQuickCaptureOpen = true" 
        />

        <!-- Linha do Tempo Atual -->
        <div v-if="currentHorizon === 'today'" class="absolute w-full flex items-center z-20 pointer-events-none transition-all duration-1000" :style="{ top: `${nowLineTop}px` }">
          <div class="w-16 text-right pr-2 text-xs font-mono font-bold text-status-danger-text relative -top-2">
            {{ currentTime.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}
          </div>
          <div class="flex-1 border-t-2 border-status-danger-border relative">
            <div class="absolute -left-1 -top-1 w-2 h-2 rounded-full bg-status-danger-text shadow-[0_0_8px_rgba(239,68,68,0.8)]" />
          </div>
        </div>

        <!-- Tarefas Alocadas -->
        <AgendaCommitmentBlock 
          v-for="node in agendaBuckets.scheduled" 
          :key="node.id"
          :item="node"
          :pixels-per-minute="PIXELS_PER_MINUTE"
          :drag-enabled-id="dragEnabledId"
          :resizing-id="resizingId"
          :preview-duration="previewDuration"
          @update:dragEnabledId="dragEnabledId = $event"
          @dragstart="handleDragStart"
          @dragend="handleDragEnd"
          @resize-start="startResize"
          @open-inspector="inspectorStore.openInspector($event, 'COMMITMENT')"
        />

        <!-- Sombra Interativa do Drag & Drop -->
        <AgendaGhostBlock 
          v-bind="ghostState"
          :pixels-per-minute="PIXELS_PER_MINUTE"
        />

      </div>
    </div>

    <!-- 4. MODAL DE CONFLITO -->
    <AgendaConflictModal 
      :is-open="conflictModal.isOpen"
      :item="conflictModal.item"
      :available-minutes="conflictModal.availableMinutes"
      @resolve="resolveConflict"
      @cancel="conflictModal.isOpen = false"
    />

  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background-color: var(--color-border-base); border-radius: 10px; }
</style>