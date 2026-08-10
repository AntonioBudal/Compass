<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { Calendar } from 'lucide-vue-next';

// Permite que inicie nulo ou indefinido vindo do banco
const modelValue = defineModel<string | null>({ required: false, default: null });

const preset = ref<'DAILY' | 'WEEKDAYS' | 'WEEKENDS' | 'CUSTOM'>('DAILY');
const time = ref('08:00');
const selectedDays = ref<number[]>([]);

const daysOfWeek = [
  { val: 1, label: 'Seg' }, { val: 2, label: 'Ter' }, { val: 3, label: 'Qua' },
  { val: 4, label: 'Qui' }, { val: 5, label: 'Sex' }, { val: 6, label: 'Sáb' }, { val: 0, label: 'Dom' }
];

// Parser Reverso (Descerialização: CRON -> UI)
const parseCron = (cron: string) => {
  if (!cron) return;
  const parts = cron.split(' ');
  if (parts.length !== 5) return; 

  time.value = `${parts[1].padStart(2, '0')}:${parts[0].padStart(2, '0')}`;
  const dayPart = parts[4];

  if (dayPart === '*' || dayPart === '0-6') {
    preset.value = 'DAILY';
    selectedDays.value = [0, 1, 2, 3, 4, 5, 6];
  } else if (dayPart === '1-5' || dayPart === '1,2,3,4,5') {
    preset.value = 'WEEKDAYS';
    selectedDays.value = [1, 2, 3, 4, 5];
  } else if (dayPart === '0,6' || dayPart === '6,0') {
    preset.value = 'WEEKENDS';
    selectedDays.value = [0, 6];
  } else {
    preset.value = 'CUSTOM';
    selectedDays.value = dayPart.split(',').map(d => parseInt(d, 10)).filter(n => !isNaN(n));
  }
};

onMounted(() => {
  if (!modelValue.value) {
    // Inicializa um CRON padrão "Diário às 08:00" na tela, sem salvar no banco ainda
    time.value = '08:00';
    preset.value = 'DAILY';
    selectedDays.value = [0, 1, 2, 3, 4, 5, 6];
  } else {
    parseCron(modelValue.value);
  }
});

// Builder Direto (Serialização: UI -> CRON)
const buildCronAndEmit = () => {
  const [hh, mm] = time.value.split(':');
  let dayPart = '*';

  if (preset.value === 'WEEKDAYS') dayPart = '1-5';
  else if (preset.value === 'WEEKENDS') dayPart = '0,6';
  else if (preset.value === 'CUSTOM') {
    dayPart = selectedDays.value.sort((a, b) => a - b).join(',') || '*';
  }

  const cronString = `${parseInt(mm, 10)} ${parseInt(hh, 10)} * * ${dayPart}`;
  modelValue.value = cronString; //  ARQ-013: Utilizando defineModel limpo sem emit manual redundante
};

const applyPreset = (p: typeof preset.value) => {
  preset.value = p;
  
  //  CORREÇÃO (BUG-011): Sincroniza a memória explicitamente ao aplicar um preset
  if (p === 'DAILY') {
    selectedDays.value = [0, 1, 2, 3, 4, 5, 6];
  } else if (p === 'WEEKDAYS') {
    selectedDays.value = [1, 2, 3, 4, 5];
  } else if (p === 'WEEKENDS') {
    selectedDays.value = [0, 6];
  }

  buildCronAndEmit();
};

const toggleDay = (d: number) => {
  preset.value = 'CUSTOM';
  if (selectedDays.value.includes(d)) {
    selectedDays.value = selectedDays.value.filter(x => x !== d);
  } else {
    selectedDays.value.push(d);
  }
  buildCronAndEmit();
};

watch(time, buildCronAndEmit);
</script>

<template>
  <div class="space-y-4 font-sans select-none">
    <div>
      <label class="block text-[10px] font-mono uppercase text-content-muted mb-2 tracking-wider">Horário do Gatilho</label>
      <div class="relative flex items-center max-w-[150px]">
        <Calendar class="w-4 h-4 text-content-muted absolute left-3" />
        <input 
          v-model="time" 
          type="time" 
          class="w-full pl-9 pr-3 py-2 bg-app border border-borderbase rounded-tactic text-sm text-content focus:border-borderfocus focus:outline-none transition-colors"
        />
      </div>
    </div>

    <div>
      <label class="block text-[10px] font-mono uppercase text-content-muted mb-2 tracking-wider">Frequência</label>
      <div class="grid grid-cols-3 gap-2 mb-3">
        <button type="button" @click="applyPreset('DAILY')" :class="preset === 'DAILY' ? 'bg-content text-content-invert border-content' : 'bg-surface border-borderbase text-content-muted hover:text-content'" class="py-1.5 text-xs font-bold rounded border transition-colors cursor-pointer">Diário</button>
        <button type="button" @click="applyPreset('WEEKDAYS')" :class="preset === 'WEEKDAYS' ? 'bg-content text-content-invert border-content' : 'bg-surface border-borderbase text-content-muted hover:text-content'" class="py-1.5 text-xs font-bold rounded border transition-colors cursor-pointer">Dias Úteis</button>
        <button type="button" @click="applyPreset('WEEKENDS')" :class="preset === 'WEEKENDS' ? 'bg-content text-content-invert border-content' : 'bg-surface border-borderbase text-content-muted hover:text-content'" class="py-1.5 text-xs font-bold rounded border transition-colors cursor-pointer">Fins de Semana</button>
      </div>

      <div class="flex items-center justify-between gap-1">
        <button 
          v-for="day in daysOfWeek" :key="day.val" type="button"
          @click="toggleDay(day.val)"
          :class="selectedDays.includes(day.val) ? 'bg-content-accent text-white border-content-accent' : 'bg-app border-borderbase text-content-muted hover:border-borderfocus'"
          class="w-8 h-8 rounded-full flex items-center justify-center text-[10px] font-bold border transition-colors cursor-pointer"
        >
          {{ day.label }}
        </button>
      </div>
    </div>
  </div>
</template>