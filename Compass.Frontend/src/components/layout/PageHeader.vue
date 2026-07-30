<script setup lang="ts">
import { computed } from 'vue';
import { useSettingsStore } from '@/stores/settingsStore';
import { LayoutList, LayoutGrid } from 'lucide-vue-next';

const props = defineProps<{
  title: string;
  description: string;
  badgeCount?: string | number;
  badgeLabel?: string;
  
  // Ações Principais
  actionLabel?: string;
  actionIcon?: any;
  
  // Density Toggle
  viewName?: string; // Ex: 'now', 'projects', 'goals'
  showDensityToggle?: boolean;
}>();

const emit = defineEmits<{
  (e: 'action'): void;
}>();

const settingsStore = useSettingsStore();

const currentDensity = computed(() => {
  if (!props.viewName) return 'detailed';
  return settingsStore.getViewDensity(props.viewName);
});

const handleToggle = () => {
  if (props.viewName) {
    settingsStore.toggleViewDensity(props.viewName);
  }
};
</script>

<template>
  <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-4 border-b border-borderbase select-none">
    
    <!-- Esquerda: Título, Badge e Descrição -->
    <div>
      <h1 class="text-2xl font-semibold text-content tracking-tight flex items-center gap-2.5">
        <span>{{ title }}</span>
        <span v-if="badgeCount !== undefined" class="text-xs font-mono bg-surface text-content-muted px-2 py-0.5 rounded border border-borderbase flex items-center gap-1">
          <strong>{{ badgeCount }}</strong> <span v-if="badgeLabel" class="hidden sm:inline">{{ badgeLabel }}</span>
        </span>
      </h1>
      <p class="text-sm text-content-muted mt-1 max-w-2xl">
        {{ description }}
      </p>
    </div>

    <!-- Direita: Ações e View Density Toggle -->
    <div class="flex items-center gap-3 self-start sm:self-auto">
      
      <!-- Toggle de Densidade (Modo Detalhado vs Compacto) -->
      <div v-if="showDensityToggle && viewName" class="flex items-center bg-surface border border-borderbase rounded-tactic p-0.5" title="Alternar densidade de visualização">
        <button 
          @click="currentDensity !== 'detailed' && handleToggle()"
          :class="currentDensity === 'detailed' ? 'bg-surface-active text-content shadow-sm' : 'text-content-muted hover:text-content'"
          class="p-1.5 rounded transition-all cursor-pointer"
        >
          <LayoutList class="w-4 h-4" />
        </button>
        <button 
          @click="currentDensity !== 'compact' && handleToggle()"
          :class="currentDensity === 'compact' ? 'bg-surface-active text-content shadow-sm' : 'text-content-muted hover:text-content'"
          class="p-1.5 rounded transition-all cursor-pointer"
        >
          <LayoutGrid class="w-4 h-4" />
        </button>
      </div>

      <!-- Botão de Ação Primária -->
      <button 
        v-if="actionLabel"
        @click="emit('action')"
        class="inline-flex items-center gap-2 px-3 py-1.5 rounded-tactic bg-content text-content-invert hover:opacity-90 text-xs font-semibold font-mono transition-all shadow-sm cursor-pointer"
      >
        <component :is="actionIcon" v-if="actionIcon" class="w-3.5 h-3.5" />
        <span class="whitespace-nowrap">{{ actionLabel }}</span>
      </button>

      <slot name="extra-actions" />
    </div>
  </div>
</template>