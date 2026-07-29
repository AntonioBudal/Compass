<script setup lang="ts">
import { type Component } from 'vue';
import { ShieldAlert, ArrowRight } from 'lucide-vue-next';

defineProps<{
  icon?: Component;
  title: string;
  explanation: string;
  actionLabel: string;
  secondaryActionLabel?: string;
  isWarning?: boolean;
}>();

const emit = defineEmits<{
  (e: 'action'): void;
  (e: 'secondaryAction'): void;
}>();
</script>

<template>
  <div 
    class="w-full p-8 md:p-10 rounded-xl border border-dashed bg-app/50 flex flex-col items-center justify-center text-center space-y-4 transition-all"
    :class="isWarning ? 'border-status-warning-border' : 'border-borderbase'"
  >
    <!-- Ícone Diagnóstico -->
    <div 
      class="w-12 h-12 rounded-full bg-surface border flex items-center justify-center text-content"
      :class="isWarning ? 'border-status-warning-border text-status-warning' : 'border-borderfocus text-content-accent'"
    >
      <component :is="icon || ShieldAlert" class="w-6 h-6 stroke-[1.75]" />
    </div>

    <!-- O que aconteceu & Por que aconteceu -->
    <div class="max-w-md space-y-1.5 font-sans">
      <h3 class="text-base font-semibold text-content">{{ title }}</h3>
      <p class="text-xs text-content-muted leading-relaxed font-mono">{{ explanation }}</p>
    </div>

    <!-- Como Resolver (Ações de Recuperação) -->
    <div class="flex flex-wrap items-center justify-center gap-3 pt-2">
      <button
        type="button"
        @click="emit('action')"
        class="inline-flex items-center gap-2 px-4 py-2 rounded-tactic bg-content text-content-invert hover:opacity-90 text-xs font-semibold font-mono tracking-wide transition-all shadow-sm cursor-pointer"
      >
        <span>{{ actionLabel }}</span>
        <ArrowRight class="w-3.5 h-3.5 stroke-[2.5]" />
      </button>

      <button
        v-if="secondaryActionLabel"
        type="button"
        @click="emit('secondaryAction')"
        class="inline-flex items-center gap-2 px-4 py-2 rounded-tactic bg-surface hover:bg-surface-hover border border-borderbase text-content text-xs font-semibold font-mono transition-all cursor-pointer"
      >
        <span>{{ secondaryActionLabel }}</span>
      </button>
    </div>
  </div>
</template>