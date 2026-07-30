<script setup lang="ts">
import { onMounted } from 'vue';
import { useRouter } from 'vue-router';
import AppLayout from '@/components/layout/AppLayout.vue';
import { useKeyboardShortcuts } from '@/composables/useKeyboardShortcuts';
import { useProjectsStore } from '@/stores/projectsStore';

const projectsStore = useProjectsStore();
useKeyboardShortcuts(); // Mantém o cérebro dos atalhos ativo
const router = useRouter();

onMounted(() => {
  try {
    const isOnboarded = localStorage.getItem('compass_onboarded');
    if (!isOnboarded) {
      localStorage.setItem('compass_onboarded', 'true');
      router.push('/sandbox');
      projectsStore.fetchCatalog();
    }
  } catch (e) {}
});
</script>

<template>
  <AppLayout />
</template>

<style>
/* Estilos globais mantidos */
</style>