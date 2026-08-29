<template>
  <div class="app-shell">
    <AppHeader v-if="showHeader">
      <template #actions>
        <slot name="header-actions" />
      </template>
    </AppHeader>

    <main class="app-main" :class="{ 'app-main--full-width': fullWidth }">
      <div class="app-content-container" :class="{ 'app-content-container--full-width': fullWidth }">
        <slot />
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import AppHeader from './AppHeader.vue'

withDefaults(
  defineProps<{
    showHeader?: boolean
    fullWidth?: boolean
  }>(),
  {
    showHeader: true,
    fullWidth: false,
  }
)
</script>

<style scoped>
.app-shell {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background-color: var(--color-bg-app);
}

.app-main {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.app-content-container {
  width: 100%;
  max-width: 1200px;
  margin: 0 auto;
  padding: var(--space-6) var(--space-4);
  flex: 1;
}

.app-content-container--full-width {
  max-width: 100%;
  padding: 0;
}

@media (max-width: 640px) {
  .app-content-container {
    padding: var(--space-4) var(--space-3);
  }
}
</style>
