<script setup lang="ts">


defineProps<{
  isOpen: boolean;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
}>();
</script>

<template>
  <Transition
    enter-active-class="transition duration-200 ease-out"
    enter-from-class="opacity-0"
    enter-to-class="opacity-100"
    leave-active-class="transition duration-150 ease-in"
    leave-from-class="opacity-100"
    leave-to-class="opacity-0"
  >
    <div 
      v-if="isOpen"
      class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-app/80 backdrop-blur-sm select-none"
    >
      <!-- Contêiner do Modal Tático -->
      <div 
        class="max-w-xl w-full rounded-xl bg-surface border border-borderhighlight shadow-2xl overflow-hidden gpu-accelerated relative"
        @click.stop
      >
        <slot />
      </div>
    </div>
  </Transition>
</template>