<script setup lang="ts">
import { computed } from 'vue';
import { useProgressStore } from '@/stores/progressStore';
import { Terminal } from 'lucide-vue-next';

const progressStore = useProgressStore();
const peaks = computed(() => progressStore.rawChronology);

const labels: Record<string, { title: string; range: string }> = {
  Morning: { title: 'MANHÃ', range: '06:00 - 11:59' },
  Afternoon: { title: 'TARDE', range: '12:00 - 17:59' },
  Evening: { title: 'NOITE', range: '18:00 - 23:59' },
  Night: { title: 'MADRUGADA', range: '00:00 - 05:59' }
};

function getPeak(bucket: string) {
  return peaks.value.find(p => p.timeBucket === bucket) || { completedCount: 0, deepWorkMinutes: 0, efficiencyPercentage: 0 };
}

// Gera barra ASCII visual proporcional ao volume concluído
function getAsciiBar(count: number) {
  const max = Math.max(1, ...peaks.value.map(p => p.completedCount));
  const blocks = Math.round((count / max) * 12);
  return '█'.repeat(blocks) + '░'.repeat(12 - blocks);
}
</script>

<template>
  <div class="p-5 rounded-xl bg-surface border border-borderbase space-y-4 font-mono select-none">
    <!-- Cabeçalho -->
    <div class="flex items-center justify-between border-b border-borderbase pb-3">
      <div class="flex items-center gap-2 text-sm font-semibold text-content uppercase tracking-wider">
        <Terminal class="w-4 h-4 text-content-muted" />
        <span>Cronobiologia de Execução (CLI View)</span>
      </div>
      <span class="text-[11px] text-content-muted">Picos de Produtividade por Turno</span>
    </div>

    <!-- Lista de Turnos ASCII -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-3 pt-1">
      <div
        v-for="key in ['Morning', 'Afternoon', 'Evening', 'Night']"
        :key="key"
        class="p-3 rounded bg-app border border-borderbase flex flex-col justify-between space-y-2 hover:border-borderfocus transition-colors"
      >
        <div class="flex items-center justify-between text-xs">
          <div>
            <span class="font-bold text-content">{{ labels[key].title }}</span>
            <span class="text-content-muted ml-1.5 text-[10px]">[{{ labels[key].range }}]</span>
          </div>
          <span class="font-bold text-content-accent">{{ getPeak(key).efficiencyPercentage }}% DEEP</span>
        </div>

        <div class="flex items-center justify-between gap-2 text-xs">
          <span class="text-content-muted tracking-widest text-[11px]">{{ getAsciiBar(getPeak(key).completedCount) }}</span>
          <span class="text-content font-bold">{{ getPeak(key).completedCount }} itens</span>
        </div>
      </div>
    </div>
  </div>
</template>