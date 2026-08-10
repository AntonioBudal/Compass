<script setup lang="ts">
import { computed } from 'vue';
import { Clock, Folder, CalendarClock } from 'lucide-vue-next';
import VisualCronEditor from './VisualCronEditor.vue';

const draft = defineModel<any>('draft', { required: true });

const emit = defineEmits<{ (e: 'update'): void }>();

const triggerSave = () => emit('update');

// FIX: Transforma a data ISO do Backend (2026-08-09T13:00:00Z) para "HH:MM" do Input
const startTimeHHMM = computed({
  get: () => {
    if (!draft.value.startTime) return '';
    const d = new Date(draft.value.startTime);
    const hours = String(d.getHours()).padStart(2, '0');
    const mins = String(d.getMinutes()).padStart(2, '0');
    return `${hours}:${mins}`;
  },
  set: (val: string) => {
    if (!val) {
      draft.value.startTime = null;
      triggerSave();
      return;
    }
    const [h, m] = val.split(':').map(Number);
    const d = draft.value.startTime ? new Date(draft.value.startTime) : new Date();
    d.setHours(h, m, 0, 0);
    draft.value.startTime = d.toISOString();
    triggerSave();
  }
});
</script>

<template>
  <div class="space-y-6">
    
    <!-- 1. TÍTULO DO HÁBITO -->
    <div>
      <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Título do Hábito</label>
      <input 
        v-model="draft.title" 
        @input="triggerSave"
        type="text" 
        class="w-full px-4 py-2.5 bg-app border border-borderbase rounded-tactic text-sm text-content focus:border-borderfocus focus:outline-none transition-colors font-semibold"
      />
    </div>

    <!-- 2. TRADUTOR VISUAL DE RECORRÊNCIA (CRON) -->
    <div class="p-4 bg-surface-active border border-borderbase rounded-xl shadow-sm">
      <VisualCronEditor 
        v-model="draft.cronExpression" 
        @update:modelValue="triggerSave" 
      />
    </div>

    <!-- 3. METADADOS TÁTICOS (Agora com 3 colunas para acomodar o Horário) -->
    <div class="grid grid-cols-3 gap-4">
      <div>
        <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Horário</label>
        <div class="relative flex items-center">
          <CalendarClock class="w-4 h-4 text-content-muted absolute left-3 pointer-events-none" />
          <input 
            v-model="startTimeHHMM" 
            type="time"
            class="w-full pl-9 pr-2 py-2 bg-app border border-borderbase rounded-tactic text-sm font-mono text-content focus:border-borderfocus focus:outline-none"
          />
        </div>
      </div>

      <div>
        <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Duração (m)</label>
        <div class="relative flex items-center">
          <Clock class="w-4 h-4 text-content-muted absolute left-3 pointer-events-none" />
          <input 
            v-model.number="draft.estimatedDurationMinutes" 
            @input="triggerSave"
            type="number" step="5" min="5"
            class="w-full pl-9 pr-3 py-2 bg-app border border-borderbase rounded-tactic text-sm font-mono text-content focus:border-borderfocus focus:outline-none"
          />
        </div>
      </div>

      <div>
        <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Energia</label>
        <select 
          v-model.number="draft.energyRequired" 
          @change="triggerSave"
          class="w-full px-3 py-2 bg-app border border-borderbase rounded-tactic text-sm font-mono text-content focus:border-borderfocus focus:outline-none cursor-pointer"
        >
          <option :value="1">■□□ (1)</option>
          <option :value="2">■■□ (2)</option>
          <option :value="3">■■■ (3)</option>
        </select>
      </div>
    </div>

    <!-- 4. PROJETO VINCULADO -->
    <div>
      <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Projeto Vinculado</label>
      <div class="relative flex items-center">
        <Folder class="w-4 h-4 text-content-muted absolute left-3 pointer-events-none" />
        <input 
          v-model="draft.projectName" 
          @input="triggerSave"
          type="text" placeholder="Sem projeto (avulso)"
          class="w-full pl-9 pr-3 py-2 bg-app border border-borderbase rounded-tactic text-sm text-content focus:border-borderfocus focus:outline-none"
        />
      </div>
    </div>

  </div>
</template>