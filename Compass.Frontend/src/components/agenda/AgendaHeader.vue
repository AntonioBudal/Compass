<!-- src/components/agenda/AgendaHeader.vue -->
<script setup lang="ts">
import { PlusCircle } from 'lucide-vue-next';

defineEmits<{
  (e: 'open-capture'): void;
}>();
</script>

<template>
  <div class="flex items-center justify-between gap-4 pb-4 border-b border-borderbase shrink-0">
    <div>
      <h1 class="text-2xl font-semibold text-content tracking-tight">Agenda Tática</h1>
      <p class="text-sm text-content-muted mt-1">Canvas livre. Puxe o topo para mover, puxe a base para esticar.</p>
    </div>
    <button 
      @click="$emit('open-capture')" 
      class="inline-flex items-center gap-2 px-3 py-1.5 rounded-tactic bg-surface hover:bg-surface-hover border border-borderbase text-xs font-medium text-content-muted hover:text-content transition-all shadow-sm cursor-pointer"
    >
      <PlusCircle class="w-3.5 h-3.5" /> <span>Novo Evento</span>
    </button>
  </div>
</template>