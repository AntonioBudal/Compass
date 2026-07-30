<script setup lang="ts">
import { useInspectorStore, type InspectableType } from '@/stores/inspectorStore';
import { Edit2 } from 'lucide-vue-next';

const props = defineProps<{
  entity: any; 
  type: InspectableType;
}>();

const inspectorStore = useInspectorStore();

const triggerEdit = () => {
  if (!props.entity) {
    console.warn('[InspectableCard] Entidade indefinida. Impossível editar.', props.entity);
    return;
  }
  inspectorStore.openInspector(props.entity, props.type);
};
</script>

<template>
  <div 
    class="relative group outline-none focus-visible:ring-2 focus-visible:ring-content-muted rounded-xl transition-all"
    tabindex="0"
    @dblclick="triggerEdit"
    @keydown.enter.prevent.stop="triggerEdit"
  >
    <!-- O Card Original (Slot) -->
    <slot />

    <!-- O Lápis de Hover -->
    <button 
      @click.stop="triggerEdit"
      class="absolute top-2.5 right-2.5 p-1.5 rounded-md bg-content text-content-invert opacity-0 group-hover:opacity-100 focus:opacity-100 transition-opacity shadow-md z-10 cursor-pointer"
      title="Editar (Duplo Clique ou Enter)"
    >
      <Edit2 class="w-3.5 h-3.5" />
    </button>
  </div>
</template>