<script setup lang="ts">
import { computed } from 'vue';
import { useDecisionStore } from '@/stores/decisionStore';
import { useCommitmentsStore } from '@/stores/commitmentsStore';

const decisionStore = useDecisionStore();
const commitmentsStore = useCommitmentsStore();

const energyLevel = computed(() => {
  switch (decisionStore.effectiveEnergy) {
    case 3: return 'Alta (3)';
    case 1: return 'Baixa (1)';
    case 2:
    default: return 'Média (2)';
  }
});

const isSyncing = computed(() => commitmentsStore.isLoading || decisionStore.isLoading);
</script>

<template>
  <footer class="h-8 px-4 border-t border-zinc-800/80 bg-zinc-950 flex items-center justify-between gap-4 text-[11px] font-mono text-zinc-500 flex-shrink-0 select-none overflow-hidden">
    <!-- Lado Esquerdo: Status do Banco Local-First -->
    <div class="flex items-center gap-2">
      <span class="w-2 h-2 rounded-full" :class="isSyncing ? 'bg-amber-400 animate-ping' : 'bg-emerald-500'" />
      <span class="truncate">
        {{ isSyncing ? 'Sincronizando .NET 10...' : 'Local-First DB: Sync OK' }}
      </span>
    </div>

    <!-- Centro: Energia Atual do Operador -->
    <div class="hidden sm:block text-zinc-400">
      ⚡ Energia Atual: <strong class="text-zinc-200">{{ energyLevel }}</strong>
    </div>

    <!-- Lado Direito: Atalhos Táticos -->
    <div class="flex items-center gap-3 text-zinc-500">
      <span><kbd class="text-zinc-400">Cmd+K</kbd> Busca</span>
      <span><kbd class="text-zinc-400">C</kbd> Criar</span>
      <span><kbd class="text-zinc-400">E</kbd> Concluir</span>
    </div>
  </footer>
</template>