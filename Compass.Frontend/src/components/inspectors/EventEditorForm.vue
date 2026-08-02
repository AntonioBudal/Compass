<script setup lang="ts">
import { computed } from 'vue';
import { Calendar, Link2 } from 'lucide-vue-next';

// 🔥 CORREÇÃO (ARQ-013): Uso seguro do defineModel
const draft = defineModel<any>('draft', { required: true });

const emit = defineEmits<{ (e: 'update'): void }>();
const triggerSave = () => emit('update');

// 🔥 CORREÇÃO (BUG-015): Tratamento de Fuso Horário sem falha matemática (Extração direta)
const toLocalInputFormat = (isoString: string | null) => {
  if (!isoString) return '';
  const date = new Date(isoString);
  if (isNaN(date.getTime())) return '';
  
  const pad = (n: number) => n.toString().padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
};

const toUtcIsoFormat = (localString: string) => {
  if (!localString) return null;
  const date = new Date(localString);
  return isNaN(date.getTime()) ? null : date.toISOString();
};

// Auxiliar para a Validação Cruzada
const getDiffInMinutes = (startIso: string | null, endIso: string | null) => {
  if (!startIso || !endIso) return 60; // Duração padrão: 1 hora
  return (new Date(endIso).getTime() - new Date(startIso).getTime()) / 60000;
};

// 🔥 CORREÇÃO (UX-014): Setters Reativos com Validação Cruzada (Cross-Validation)
const localStartTime = computed({
  get: () => toLocalInputFormat(draft.value.startTime),
  set: (val) => {
    const newStartIso = toUtcIsoFormat(val);
    if (!newStartIso) return;
    
    const prevStartIso = draft.value.startTime;
    const duration = getDiffInMinutes(prevStartIso, draft.value.endTime);
    
    draft.value.startTime = newStartIso;

    // Se a data de fim for nula ou menor que a nova data de início, empurramos o fim pra frente mantendo a duração
    if (!draft.value.endTime || new Date(draft.value.endTime).getTime() <= new Date(newStartIso).getTime()) {
      const newEnd = new Date(newStartIso);
      newEnd.setMinutes(newEnd.getMinutes() + (duration > 0 ? duration : 60));
      draft.value.endTime = newEnd.toISOString();
    }
    triggerSave();
  }
});

const localEndTime = computed({
  get: () => toLocalInputFormat(draft.value.endTime),
  set: (val) => {
    const newEndIso = toUtcIsoFormat(val);
    if (!newEndIso) return;
    
    draft.value.endTime = newEndIso;

    // Se o fim for antes do início, recua o início automaticamente em 1 hora
    if (draft.value.startTime && new Date(newEndIso).getTime() <= new Date(draft.value.startTime).getTime()) {
      const newStart = new Date(newEndIso);
      newStart.setHours(newStart.getHours() - 1);
      draft.value.startTime = newStart.toISOString();
    }
    
    triggerSave();
  }
});
</script>

<template>
  <div class="space-y-5">
    <div>
      <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Título do Evento</label>
      <input 
        v-model="draft.title" 
        @input="triggerSave"
        type="text" 
        class="w-full px-4 py-2.5 bg-app border border-borderbase rounded-tactic text-sm text-content focus:border-borderfocus focus:outline-none font-semibold"
      />
    </div>

    <div class="grid grid-cols-2 gap-4">
      <div>
        <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Início</label>
        <div class="relative flex items-center">
          <Calendar class="w-4 h-4 text-content-muted absolute left-3 pointer-events-none" />
          <input 
            v-model="localStartTime" 
            type="datetime-local" 
            class="w-full pl-9 pr-2 py-2 bg-app border border-borderbase rounded-tactic text-xs font-mono text-content focus:border-borderfocus focus:outline-none"
          />
        </div>
      </div>
      <div>
        <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Término</label>
        <div class="relative flex items-center">
          <Calendar class="w-4 h-4 text-content-muted absolute left-3 pointer-events-none" />
          <input 
            v-model="localEndTime" 
            type="datetime-local" 
            class="w-full pl-9 pr-2 py-2 bg-app border border-borderbase rounded-tactic text-xs font-mono text-content focus:border-borderfocus focus:outline-none"
          />
        </div>
      </div>
    </div>

    <div>
      <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Localização ou Link da Reunião</label>
      <div class="relative flex items-center">
        <Link2 class="w-4 h-4 text-content-muted absolute left-3" />
        <input 
          v-model="draft.locationOrLink" 
          @input="triggerSave"
          type="url" 
          placeholder="https://meet.google.com/..."
          class="w-full pl-9 pr-3 py-2 bg-app border border-borderbase rounded-tactic text-sm text-content focus:border-borderfocus focus:outline-none"
        />
      </div>
    </div>
  </div>
</template>