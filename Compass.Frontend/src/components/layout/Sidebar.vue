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
    class="h-full flex flex-col bg-zinc-950 border-r border-zinc-800/80 transition-all duration-tactic select-none flex-shrink-0"
    :class="isCollapsed ? 'w-16' : 'w-64'"
  >
    <!-- Cabeçalho -->
    <div class="h-14 px-4 border-b border-zinc-800/80 flex items-center justify-between gap-2">
      <div v-if="!isCollapsed" class="flex items-center gap-2.5 min-w-0">
        <div class="w-6 h-6 rounded bg-zinc-100 flex items-center justify-center text-zinc-950 font-bold text-xs font-mono">
          C
        </div>
        <div class="truncate">
          <span class="text-sm font-semibold text-zinc-100 block truncate">Compass MVP</span>
          <span class="text-[10px] font-mono text-zinc-500 block truncate">Local-First Engine</span>
        </div>
      </div>
      <button 
        @click="toggleSidebar" 
        class="p-1.5 rounded-md text-zinc-500 hover:text-zinc-200 hover:bg-zinc-900 transition-colors ml-auto cursor-pointer"
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
        <p v-if="!isCollapsed" class="px-2 mb-2 text-[10px] font-mono font-semibold text-zinc-500 uppercase tracking-wider">
          Execução
        </p>
        <div class="space-y-1">
          <router-link 
            to="/now" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/now' ? 'bg-zinc-800/90 text-white border-l-2 border-zinc-300 shadow-sm' : 'text-zinc-400 hover:text-zinc-200 hover:bg-zinc-900/60'"
          >
            <Zap class="w-4 h-4 flex-shrink-0" :class="route.path === '/now' ? 'text-zinc-100' : 'text-zinc-500'" />
            <span v-if="!isCollapsed" class="truncate flex-1">Agora</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-zinc-500 bg-zinc-900 px-1.5 py-0.5 rounded border border-zinc-800">G N</span>
          </router-link>

          <router-link 
            to="/agenda" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/agenda' ? 'bg-zinc-800/90 text-white border-l-2 border-zinc-300 shadow-sm' : 'text-zinc-400 hover:text-zinc-200 hover:bg-zinc-900/60'"
          >
            <Calendar class="w-4 h-4 flex-shrink-0 text-zinc-500" />
            <span v-if="!isCollapsed" class="truncate flex-1">Agenda</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-zinc-500 bg-zinc-900 px-1.5 py-0.5 rounded border border-zinc-800">G A</span>
          </router-link>

          <router-link 
            to="/habits" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/habits' ? 'bg-zinc-800/90 text-white border-l-2 border-zinc-300 shadow-sm' : 'text-zinc-400 hover:text-zinc-200 hover:bg-zinc-900/60'"
          >
            <RefreshCw class="w-4 h-4 flex-shrink-0 text-zinc-500" />
            <span v-if="!isCollapsed" class="truncate flex-1">Hábitos</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-zinc-500 bg-zinc-900 px-1.5 py-0.5 rounded border border-zinc-800">G H</span>
          </router-link>
        </div>
      </div>

      <!-- Grupo 2: Estratégia -->
      <div>
        <p v-if="!isCollapsed" class="px-2 mb-2 text-[10px] font-mono font-semibold text-zinc-500 uppercase tracking-wider">
          Estratégia
        </p>
        <div class="space-y-1">
          <router-link 
            to="/projects" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/projects' ? 'bg-zinc-800/90 text-white border-l-2 border-zinc-300 shadow-sm' : 'text-zinc-400 hover:text-zinc-200 hover:bg-zinc-900/60'"
          >
            <Folder class="w-4 h-4 flex-shrink-0 text-zinc-500" />
            <span v-if="!isCollapsed" class="truncate flex-1">Projetos</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-zinc-500 bg-zinc-900 px-1.5 py-0.5 rounded border border-zinc-800">G P</span>
          </router-link>

          <router-link 
            to="/goals" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/goals' ? 'bg-zinc-800/90 text-white border-l-2 border-zinc-300 shadow-sm' : 'text-zinc-400 hover:text-zinc-200 hover:bg-zinc-900/60'"
          >
            <Target class="w-4 h-4 flex-shrink-0 text-zinc-500" />
            <span v-if="!isCollapsed" class="truncate flex-1">Metas</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-zinc-500 bg-zinc-900 px-1.5 py-0.5 rounded border border-zinc-800">G G</span>
          </router-link>
        </div>
      </div>

      <!-- Grupo 3: Auditoria & Sistema -->
      <div>
        <p v-if="!isCollapsed" class="px-2 mb-2 text-[10px] font-mono font-semibold text-zinc-500 uppercase tracking-wider">
          Sistema
        </p>
        <div class="space-y-1">
          <router-link 
            to="/journal" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/journal' ? 'bg-zinc-800/90 text-white border-l-2 border-zinc-300 shadow-sm' : 'text-zinc-400 hover:text-zinc-200 hover:bg-zinc-900/60'"
          >
            <FileText class="w-4 h-4 flex-shrink-0 text-zinc-500" />
            <span v-if="!isCollapsed" class="truncate flex-1">Auditoria</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-zinc-500 bg-zinc-900 px-1.5 py-0.5 rounded border border-zinc-800">G J</span>
          </router-link>

          <router-link 
            to="/settings" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/settings' ? 'bg-zinc-800/90 text-white border-l-2 border-zinc-300 shadow-sm' : 'text-zinc-400 hover:text-zinc-200 hover:bg-zinc-900/60'"
          >
            <Sliders class="w-4 h-4 flex-shrink-0 text-zinc-500" />
            <span v-if="!isCollapsed" class="truncate flex-1">Configurações</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-zinc-500 bg-zinc-900 px-1.5 py-0.5 rounded border border-zinc-800">G S</span>
          </router-link>
        </div>
      </div>
    </nav>

    <!-- Rodapé: Botão Rápido de Shutdown & Busca -->
    <div class="p-2 border-t border-zinc-800/80 space-y-1.5">
      <button 
        @click="journalStore.startShutdown"
        class="w-full flex items-center gap-2.5 px-2.5 py-2 rounded-tactic bg-zinc-900 hover:bg-zinc-800 border border-zinc-700/80 text-zinc-200 hover:text-white transition-all text-xs font-medium cursor-pointer"
        :title="isCollapsed ? 'Encerrar Turno' : ''"
      >
        <Power class="w-4 h-4 text-zinc-400 flex-shrink-0" />
        <span v-if="!isCollapsed" class="truncate flex-1 text-left">Encerrar Turno</span>
      </button>

      <button 
        @click="openSearch" 
        class="w-full flex items-center gap-2.5 px-2.5 py-2 rounded-tactic bg-zinc-950 hover:bg-zinc-900 border border-zinc-800 text-zinc-400 hover:text-zinc-200 transition-all text-xs cursor-pointer"
      >
        <Search class="w-4 h-4 text-zinc-500 flex-shrink-0" />
        <span v-if="!isCollapsed" class="truncate flex-1 text-left">Comandos...</span>
        <kbd v-if="!isCollapsed" class="px-1.5 py-0.5 font-mono text-[10px] bg-zinc-900 rounded border border-zinc-800 text-zinc-500">Cmd+K</kbd>
      </button>
    </div>
  </aside>
</template>