<script setup lang="ts">
import { computed } from 'vue';
import { Calendar, Link2 } from 'lucide-vue-next';

const props = defineProps<{ draft: any }>();
const emit = defineEmits<{ (e: 'update'): void }>();

// Helpers de Conversão (UTC ISO <-> Local YYYY-MM-DDThh:mm)
const toLocalInputFormat = (isoString: string | null) => {
  if (!isoString) return '';
  const date = new Date(isoString);
  const offset = date.getTimezoneOffset() * 60000;
  return new Date(date.getTime() - offset).toISOString().slice(0, 16);
};

const toUtcIsoFormat = (localString: string) => {
  if (!localString) return null;
  return new Date(localString).toISOString();
};

const localStartTime = computed({
  get: () => toLocalInputFormat(props.draft.startTime),
  set: (val) => { props.draft.startTime = toUtcIsoFormat(val); emit('update'); }
});

const localEndTime = computed({
  get: () => toLocalInputFormat(props.draft.endTime),
  set: (val) => { props.draft.endTime = toUtcIsoFormat(val); emit('update'); }
});
</script>

<template>
  <div class="space-y-5">
    <div>
      <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Título do Evento</label>
      <input 
        v-model="props.draft.title" @input="emit('update')"
        type="text" class="w-full px-4 py-2.5 bg-app border border-borderbase rounded-tactic text-sm text-content focus:border-borderfocus focus:outline-none font-semibold"
      />
    </div>

    <div class="grid grid-cols-2 gap-4">
      <div>
        <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Início</label>
        <div class="relative flex items-center">
          <Calendar class="w-4 h-4 text-content-muted absolute left-3 pointer-events-none" />
          <input 
            v-model="localStartTime" type="datetime-local" 
            class="w-full pl-9 pr-2 py-2 bg-app border border-borderbase rounded-tactic text-xs font-mono text-content focus:border-borderfocus focus:outline-none"
          />
        </div>
      </div>
      <div>
        <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Término</label>
        <div class="relative flex items-center">
          <Calendar class="w-4 h-4 text-content-muted absolute left-3 pointer-events-none" />
          <input 
            v-model="localEndTime" type="datetime-local" 
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
          v-model="props.draft.locationOrLink" @input="emit('update')"
          type="url" placeholder="https://meet.google.com/..."
          class="w-full pl-9 pr-3 py-2 bg-app border border-borderbase rounded-tactic text-sm text-content focus:border-borderfocus focus:outline-none"
        />
      </div>
    </div>
  </div>
</template>