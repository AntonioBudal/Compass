<template>
  <header class="app-header">
    <div class="header-container">
      <div class="header-brand">
        <RouterLink to="/" class="brand-link" aria-label="Compass Início">
          <span class="brand-name">Compass</span>
        </RouterLink>
      </div>

      <nav class="header-nav" aria-label="Navegação principal">
        <RouterLink
          to="/today"
          class="nav-link"
          :class="{ 'nav-link--active': isRouteActive('/today') }"
        >
          Hoje
        </RouterLink>
        <RouterLink
          to="/planning"
          class="nav-link"
          :class="{ 'nav-link--active': isRouteActive('/planning') }"
        >
          Planning
        </RouterLink>
      </nav>

      <div class="header-actions">
        <slot name="actions" />
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import { RouterLink, useRoute } from 'vue-router'

let route: any = null
try {
  route = useRoute()
} catch {
  // router context not present
}

function isRouteActive(path: string): boolean {
  if (!route || !route.path) return false
  return route.path === path || route.path.startsWith(path + '/')
}
</script>

<style scoped>
.app-header {
  height: 56px;
  background-color: var(--color-surface);
  border-bottom: 1px solid var(--color-border);
  display: flex;
  align-items: center;
  position: sticky;
  top: 0;
  z-index: 100;
}

.header-container {
  width: 100%;
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 var(--space-4);
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.header-brand {
  display: flex;
  align-items: center;
}

.brand-link {
  text-decoration: none;
  color: var(--color-text-primary);
  display: flex;
  align-items: center;
}

.brand-name {
  font-size: var(--font-size-base);
  font-weight: var(--font-weight-semibold);
  letter-spacing: -0.01em;
}

.header-nav {
  display: flex;
  align-items: center;
  gap: var(--space-2);
}

.nav-link {
  text-decoration: none;
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-medium);
  color: var(--color-text-secondary);
  padding: var(--space-2) var(--space-3);
  border-radius: var(--radius-sm);
  transition: color var(--transition-fast), background-color var(--transition-fast);
}

.nav-link:hover {
  color: var(--color-text-primary);
  background-color: var(--color-surface-hover);
}

.nav-link--active {
  color: var(--color-text-primary);
  background-color: var(--color-surface-subtle);
  font-weight: var(--font-weight-semibold);
  box-shadow: inset 0 -2px 0 var(--color-accent);
}

.header-actions {
  display: flex;
  align-items: center;
  gap: var(--space-2);
}

@media (max-width: 640px) {
  .header-container {
    padding: 0 var(--space-3);
  }

  .nav-link {
    padding: var(--space-1) var(--space-2);
    font-size: var(--font-size-xs);
  }
}
</style>
