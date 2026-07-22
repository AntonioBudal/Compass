<script setup lang="ts">
import { ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useJournalStore } from '@/stores/journalStore';
import { isCommandBarOpen } from '@/composables/useKeyboardShortcuts';
import { 
  Zap, Calendar, Folder, Target, RefreshCw, 
  FileText, Sliders, PanelLeftClose, PanelLeftOpen, 
  Search, Power 
} from 'lucide-vue-next';

const isCollapsed = ref(false);
const route = useRoute();
const router = useRouter();
const journalStore = useJournalStore();

const toggleSidebar = () => {
  isCollapsed.value = !isCollapsed.value;
};

const openSearch = () => {
  isCommandBarOpen.value = true;
};
</script>

<template>
  <aside 
    class="h-full flex flex-col bg-app border-r border-borderbase transition-all duration-tactic select-none flex-shrink-0"
    :class="isCollapsed ? 'w-16' : 'w-64'"
  >
    <!-- Cabeçalho -->
    <div class="h-14 px-4 border-b border-borderbase flex items-center justify-between gap-2">
      <div v-if="!isCollapsed" class="flex items-center gap-2.5 min-w-0">
        <div class="w-6 h-6 rounded bg-content flex items-center justify-center text-content-invert font-bold text-xs font-mono">
          C
        </div>
        <div class="truncate">
          <span class="text-sm font-semibold text-content block truncate">Compass MVP</span>
          <span class="text-[10px] font-mono text-content-muted block truncate">Local-First Engine</span>
        </div>
      </div>
      <button 
        @click="toggleSidebar" 
        class="p-1.5 rounded-md text-content-muted hover:text-content hover:bg-surface-hover transition-colors ml-auto cursor-pointer"
        :title="isCollapsed ? 'Expandir Sidebar' : 'Colapsar Sidebar'"
      >
        <PanelLeftOpen v-if="isCollapsed" class="w-4 h-4" />
        <PanelLeftClose v-else class="w-4 h-4" />
      </button>
    </div>

    <!-- Navegação em Grupos -->
    <nav class="flex-1 overflow-y-auto p-2 space-y-6">
      <!-- Grupo 1: Execução -->
      <div>
        <p v-if="!isCollapsed" class="px-2 mb-2 text-[10px] font-mono font-semibold text-content-muted uppercase tracking-wider">
          Execução
        </p>
        <div class="space-y-1">
          <router-link 
            to="/now" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/now' ? 'bg-surface-active text-content border-l-2 border-borderhighlight shadow-sm' : 'text-content-muted hover:text-content hover:bg-surface-hover'"
          >
            <Zap class="w-4 h-4 flex-shrink-0" :class="route.path === '/now' ? 'text-content' : 'text-content-muted'" />
            <span v-if="!isCollapsed" class="truncate flex-1">Agora</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-content-muted bg-surface px-1.5 py-0.5 rounded border border-borderbase">G N</span>
          </router-link>

          <router-link 
            to="/agenda" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/agenda' ? 'bg-surface-active text-content border-l-2 border-borderhighlight shadow-sm' : 'text-content-muted hover:text-content hover:bg-surface-hover'"
          >
            <Calendar class="w-4 h-4 flex-shrink-0 text-content-muted" />
            <span v-if="!isCollapsed" class="truncate flex-1">Agenda</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-content-muted bg-surface px-1.5 py-0.5 rounded border border-borderbase">G A</span>
          </router-link>

          <router-link 
            to="/habits" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/habits' ? 'bg-surface-active text-content border-l-2 border-borderhighlight shadow-sm' : 'text-content-muted hover:text-content hover:bg-surface-hover'"
          >
            <RefreshCw class="w-4 h-4 flex-shrink-0 text-content-muted" />
            <span v-if="!isCollapsed" class="truncate flex-1">Hábitos</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-content-muted bg-surface px-1.5 py-0.5 rounded border border-borderbase">G H</span>
          </router-link>
        </div>
      </div>

      <!-- Grupo 2: Estratégia -->
      <div>
        <p v-if="!isCollapsed" class="px-2 mb-2 text-[10px] font-mono font-semibold text-content-muted uppercase tracking-wider">
          Estratégia
        </p>
        <div class="space-y-1">
          <router-link 
            to="/projects" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/projects' ? 'bg-surface-active text-content border-l-2 border-borderhighlight shadow-sm' : 'text-content-muted hover:text-content hover:bg-surface-hover'"
          >
            <Folder class="w-4 h-4 flex-shrink-0 text-content-muted" />
            <span v-if="!isCollapsed" class="truncate flex-1">Projetos</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-content-muted bg-surface px-1.5 py-0.5 rounded border border-borderbase">G P</span>
          </router-link>

          <router-link 
            to="/goals" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/goals' ? 'bg-surface-active text-content border-l-2 border-borderhighlight shadow-sm' : 'text-content-muted hover:text-content hover:bg-surface-hover'"
          >
            <Target class="w-4 h-4 flex-shrink-0 text-content-muted" />
            <span v-if="!isCollapsed" class="truncate flex-1">Metas</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-content-muted bg-surface px-1.5 py-0.5 rounded border border-borderbase">G G</span>
          </router-link>
        </div>
      </div>

      <!-- Grupo 3: Auditoria & Sistema -->
      <div>
        <p v-if="!isCollapsed" class="px-2 mb-2 text-[10px] font-mono font-semibold text-content-muted uppercase tracking-wider">
          Sistema
        </p>
        <div class="space-y-1">
          <router-link 
            to="/journal" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/journal' ? 'bg-surface-active text-content border-l-2 border-borderhighlight shadow-sm' : 'text-content-muted hover:text-content hover:bg-surface-hover'"
          >
            <FileText class="w-4 h-4 flex-shrink-0 text-content-muted" />
            <span v-if="!isCollapsed" class="truncate flex-1">Auditoria</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-content-muted bg-surface px-1.5 py-0.5 rounded border border-borderbase">G J</span>
          </router-link>

          <router-link 
            to="/settings" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/settings' ? 'bg-surface-active text-content border-l-2 border-borderhighlight shadow-sm' : 'text-content-muted hover:text-content hover:bg-surface-hover'"
          >
            <Sliders class="w-4 h-4 flex-shrink-0 text-content-muted" />
            <span v-if="!isCollapsed" class="truncate flex-1">Configurações</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-content-muted bg-surface px-1.5 py-0.5 rounded border border-borderbase">G S</span>
          </router-link>
        </div>
      </div>
    </nav>

    <!-- Rodapé: Botão Rápido de Shutdown & Busca -->
    <div class="p-2 border-t border-borderbase space-y-1.5">
      <button 
        @click="journalStore.startShutdown"
        class="w-full flex items-center gap-2.5 px-2.5 py-2 rounded-tactic bg-surface hover:bg-surface-hover border border-borderbase text-content-muted hover:text-content transition-all text-xs font-medium cursor-pointer"
        :title="isCollapsed ? 'Encerrar Turno' : ''"
      >
        <Power class="w-4 h-4 text-content-muted flex-shrink-0" />
        <span v-if="!isCollapsed" class="truncate flex-1 text-left">Encerrar Turno</span>
      </button>

      <button 
        @click="openSearch" 
        class="w-full flex items-center gap-2.5 px-2.5 py-2 rounded-tactic bg-app hover:bg-surface-hover border border-borderbase text-content-muted hover:text-content transition-all text-xs cursor-pointer"
      >
        <Search class="w-4 h-4 text-content-muted flex-shrink-0" />
        <span v-if="!isCollapsed" class="truncate flex-1 text-left">Comandos...</span>
        <kbd v-if="!isCollapsed" class="px-1.5 py-0.5 font-mono text-[10px] bg-surface rounded border border-borderbase text-content-muted">Cmd+K</kbd>
      </button>
    </div>
  </aside>
</template>