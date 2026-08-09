<!-- src/components/agenda/AgendaConflictModal.vue -->
<script setup lang="ts">
import { TriangleAlert } from 'lucide-vue-next';
import type { CommitmentItem } from '@/stores/commitmentsStore';

defineProps<{
  isOpen: boolean;
  item: CommitmentItem | null;
  availableMinutes: number;
}>();

defineEmits<{
  (e: 'resolve', resolution: 'REDUCE' | 'OVERLAP'): void;
  (e: 'cancel'): void;
}>();
</script>

<template>
  <div v-if="isOpen" class="absolute inset-0 z-50 flex items-center justify-center p-4 bg-app/80 backdrop-blur-sm">
    <div class="w-full max-w-md bg-surface border border-borderfocus rounded-xl shadow-2xl overflow-hidden p-6 space-y-6 animate-in zoom-in-95 duration-200">
      
      <div class="flex items-start gap-4">
        <div class="p-3 bg-status-warning-bg/30 text-status-warning rounded-full shrink-0"><TriangleAlert class="w-6 h-6" /></div>
        <div>
          <h3 class="text-lg font-bold text-content tracking-tight">Conflito Detectado</h3>
          <p class="text-sm text-content-muted mt-1 leading-relaxed">
            Você está tentando colocar uma tarefa de <strong>{{ item?.estimatedDurationMinutes }} minutos</strong> 
            em um espaço de apenas <strong>{{ availableMinutes }} minutos</strong>.
          </p>
        </div>
      </div>

      <div class="flex flex-col gap-3">
        <button @click="$emit('resolve', 'REDUCE')" class="w-full p-4 rounded-lg bg-surface-active border border-borderfocus hover:border-content-accent text-left transition-colors cursor-pointer group">
          <h4 class="text-sm font-semibold text-content group-hover:text-content-accent">Opção A: Ajustar (Recomendado)</h4>
          <p class="text-xs text-content-muted mt-1">Reduz magicamente para <strong>{{ availableMinutes }}m</strong> e encaixa perfeitamente.</p>
        </button>
        <button @click="$emit('resolve', 'OVERLAP')" class="w-full p-4 rounded-lg bg-surface border border-borderbase hover:border-content-muted text-left transition-colors cursor-pointer">
          <h4 class="text-sm font-medium text-content">Opção B: Forçar Agendamento</h4>
          <p class="text-xs text-content-muted mt-1">Mantém a duração e sobrepõe a tarefa com o próximo compromisso.</p>
        </button>
      </div>

      <div class="flex justify-end pt-2 border-t border-borderbase">
        <button @click="$emit('cancel')" class="px-4 py-2 text-sm font-medium text-content-muted hover:text-content cursor-pointer transition-colors">Cancelar</button>
      </div>

    </div>
  </div>
</template>