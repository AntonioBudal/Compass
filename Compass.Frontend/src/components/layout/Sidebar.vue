<script setup lang="ts">
import { ref } from 'vue';
import { useRoute } from 'vue-router';
import { 
  Zap, Calendar, Folder, Target, RefreshCw, 
  History, Settings, PanelLeftClose, PanelLeftOpen, Search 
} from 'lucide-vue-next';

const isCollapsed = ref(false);
const route = useRoute();

const toggleSidebar = () => {
  isCollapsed.value = !isCollapsed.value;
};
</script>

<template>
  <aside 
    class="h-full flex flex-col bg-zinc-950 border-r border-zinc-800/80 transition-all duration-tactic select-none flex-shrink-0"
    :class="isCollapsed ? 'w-16' : 'w-64'"
  >
    <!-- Cabeçalho da Workspace (Cap. 4.2) -->
    <div class="h-14 px-4 border-b border-zinc-800/80 flex items-center justify-between gap-2">
      <div v-if="!isCollapsed" class="flex items-center gap-2.5 min-w-0">
        <div class="w-6 h-6 rounded bg-indigo-600 flex items-center justify-center text-white font-bold text-xs font-mono">
          C
        </div>
        <div class="truncate">
          <span class="text-sm font-semibold text-zinc-100 block truncate">Compass MVP</span>
          <span class="text-[10px] font-mono text-zinc-500 block truncate">📍 America/Sao_Paulo</span>
        </div>
      </div>
      <button 
        @click="toggleSidebar" 
        class="p-1.5 rounded-md text-zinc-400 hover:text-zinc-100 hover:bg-zinc-900 transition-colors ml-auto"
        :title="isCollapsed ? 'Expandir Sidebar (Cmd+\)' : 'Colapsar Sidebar (Cmd+\)'"
      >
        <PanelLeftOpen v-if="isCollapsed" class="w-4 h-4" />
        <PanelLeftClose v-else class="w-4 h-4" />
      </button>
    </div>

    <!-- Grupos de Navegação Principal -->
    <nav class="flex-1 overflow-y-auto p-2 space-y-6">
      <!-- Grupo 1: Motor de Execução -->
      <div>
        <p v-if="!isCollapsed" class="px-2 mb-2 text-[10px] font-semibold text-zinc-500 uppercase tracking-wider">
          Execução (Core)
        </p>
        <div class="space-y-1">
          <router-link 
            to="/now" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/now' ? 'bg-zinc-800/90 text-white border-l-2 border-indigo-500 shadow-sm' : 'text-zinc-400 hover:text-zinc-100 hover:bg-zinc-900/60'"
            :title="isCollapsed ? 'Agora (G N)' : ''"
          >
            <Zap class="w-4 h-4 text-indigo-400 flex-shrink-0" />
            <span v-if="!isCollapsed" class="truncate flex-1">Agora</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-zinc-500 bg-zinc-900 px-1.5 py-0.5 rounded border border-zinc-800">G N</span>
          </router-link>

          <router-link 
            to="/agenda" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/agenda' ? 'bg-zinc-800/90 text-white border-l-2 border-indigo-500 shadow-sm' : 'text-zinc-400 hover:text-zinc-100 hover:bg-zinc-900/60'"
            :title="isCollapsed ? 'Agenda (G A)' : ''"
          >
            <Calendar class="w-4 h-4 text-zinc-400 flex-shrink-0" />
            <span v-if="!isCollapsed" class="truncate flex-1">Agenda</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-zinc-500 bg-zinc-900 px-1.5 py-0.5 rounded border border-zinc-800">G A</span>
          </router-link>
        </div>
      </div>

      <!-- Grupo 2: Planejamento & Estratégia -->
      <div>
        <p v-if="!isCollapsed" class="px-2 mb-2 text-[10px] font-semibold text-zinc-500 uppercase tracking-wider">
          Estratégia
        </p>
        <div class="space-y-1">
          <router-link 
            to="/goals" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/goals' ? 'bg-zinc-800/90 text-white border-l-2 border-indigo-500 shadow-sm' : 'text-zinc-400 hover:text-zinc-100 hover:bg-zinc-900/60'"
          >
            <Target class="w-4 h-4 text-zinc-400 flex-shrink-0" />
            <span v-if="!isCollapsed" class="truncate flex-1">Metas</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-zinc-500 bg-zinc-900 px-1.5 py-0.5 rounded border border-zinc-800">G G</span>
          </router-link>

          <router-link 
            to="/projects" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/projects' ? 'bg-zinc-800/90 text-white border-l-2 border-indigo-500 shadow-sm' : 'text-zinc-400 hover:text-zinc-100 hover:bg-zinc-900/60'"
          >
            <Folder class="w-4 h-4 text-zinc-400 flex-shrink-0" />
            <span v-if="!isCollapsed" class="truncate flex-1">Projetos</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-zinc-500 bg-zinc-900 px-1.5 py-0.5 rounded border border-zinc-800">G P</span>
          </router-link>

          <router-link 
            to="/habits" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/habits' ? 'bg-zinc-800/90 text-white border-l-2 border-indigo-500 shadow-sm' : 'text-zinc-400 hover:text-zinc-100 hover:bg-zinc-900/60'"
          >
            <RefreshCw class="w-4 h-4 text-zinc-400 flex-shrink-0" />
            <span v-if="!isCollapsed" class="truncate flex-1">Hábitos</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-zinc-500 bg-zinc-900 px-1.5 py-0.5 rounded border border-zinc-800">G H</span>
          </router-link>
        </div>
      </div>
    </nav>

    <!-- Rodapé da Sidebar: Busca Global -->
    <div class="p-2 border-t border-zinc-800/80">
      <button 
        type="button" 
        class="w-full flex items-center gap-2.5 px-2.5 py-2 rounded-tactic bg-zinc-900/60 hover:bg-zinc-800 border border-zinc-800 text-zinc-400 hover:text-zinc-100 transition-all text-xs"
      >
        <Search class="w-4 h-4 text-zinc-400 flex-shrink-0" />
        <span v-if="!isCollapsed" class="truncate flex-1 text-left">Busca Global...</span>
        <kbd v-if="!isCollapsed" class="px-1.5 py-0.5 font-mono text-[10px] bg-zinc-950 rounded border border-zinc-800 text-zinc-500">Cmd+K</kbd>
      </button>
    </div>
  </aside>
</template>