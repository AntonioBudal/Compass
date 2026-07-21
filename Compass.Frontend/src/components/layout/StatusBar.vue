<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue';
import { useOfflineStore } from '@/stores/offlineStore';
import { useDecisionStore } from '@/stores/decisionStore';
import { Wifi, WifiOff, RefreshCw, Clock } from 'lucide-vue-next';

const offlineStore = useOfflineStore();
const decisionStore = useDecisionStore();

onMounted(() => {
  offlineStore.initNetworkListeners();
});

onUnmounted(() => {
  offlineStore.removeListeners();
});
</script>

<template>
  <footer class="h-8 bg-zinc-950 border-t border-zinc-800/80 px-4 flex items-center justify-between font-mono text-[11px] text-zinc-500 select-none flex-shrink-0">
    
    <!-- Lado Esquerdo: Status da Conexão e Fila Offline -->
    <div class="flex items-center gap-3">
      <!-- Indicador Online / Offline -->
      <div class="flex items-center gap-1.5 font-semibold">
        <span v-if="offlineStore.isOnline" class="flex items-center gap-1 text-emerald-400">
          <Wifi class="w-3 h-3 stroke-[2.5]" />
          <span>[SYNCED]</span>
        </span>
        <span v-else class="flex items-center gap-1 text-amber-400 animate-pulse">
          <WifiOff class="w-3 h-3 stroke-[2.5]" />
          <span>[OFFLINE MODE]</span>
        </span>
      </div>

      <!-- Indicador de Itens Pendentes na Fila -->
      <div v-if="offlineStore.queue.length > 0" class="flex items-center gap-1.5 text-zinc-300">
        <RefreshCw class="w-3 h-3 text-indigo-400" :class="{ 'animate-spin': offlineStore.isSyncingQueue }" />
        <span>Fila: {{ offlineStore.queue.length }} pendente(s)</span>
      </div>
    </div>

    <!-- Lado Direito: Telemetria do Turno (M tempo) -->
    <div class="flex items-center gap-4">
      <span class="flex items-center gap-1">
        <Clock class="w-3 h-3 text-zinc-600" />
        <span>Janela Livre: <strong class="text-zinc-300">{{ decisionStore.availableMinutes }}m</strong></span>
      </span>
      <span class="hidden sm:inline text-zinc-600">|</span>
      <span class="hidden sm:inline">Compass v1.0 MVP</span>
    </div>

  </footer>
</template>