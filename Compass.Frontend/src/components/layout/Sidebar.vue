<script setup lang="ts">
import { ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useJournalStore } from '@/stores/journalStore';
import { useOnboardingStore } from '@/stores/onboardingStore';
import { isCommandBarOpen } from '@/composables/useKeyboardShortcuts';
import { 
  Zap, Calendar, Folder, Target, RefreshCw, 
  FileText, Sliders, PanelLeftClose, PanelLeftOpen, 
  Search, Power, Terminal, BookOpen //  ARQ-00: BookOpen importado para a Library
} from 'lucide-vue-next';

const isCollapsed = ref(false);
const route = useRoute();
const router = useRouter();
const journalStore = useJournalStore();
const onboardingStore = useOnboardingStore();

const toggleSidebar = () => {
  isCollapsed.value = !isCollapsed.value;
};

const openSearch = () => {
  isCommandBarOpen.value = true;
};

const handleSandboxClick = () => {
  if (onboardingStore.isSandboxActive) {
    onboardingStore.finishOnboarding();
  } else {
    onboardingStore.activateRichSandbox();
  }
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
            :class="route.path === '/now' ? 'bg-surface-active text-content border-l-2 border-borderhighlight shadow-sm font-bold' : 'text-content-muted hover:text-content hover:bg-surface-hover'"
          >
            <Zap class="w-4 h-4 flex-shrink-0" :class="route.path === '/now' ? 'text-content' : 'text-content-muted'" />
            <span v-if="!isCollapsed" class="truncate flex-1">Agora</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-content-muted bg-surface px-1.5 py-0.5 rounded border border-borderbase">G N</span>
          </router-link>

          <router-link 
            to="/agenda" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/agenda' ? 'bg-surface-active text-content border-l-2 border-borderhighlight shadow-sm font-bold' : 'text-content-muted hover:text-content hover:bg-surface-hover'"
          >
            <Calendar class="w-4 h-4 flex-shrink-0" :class="route.path === '/agenda' ? 'text-content' : 'text-content-muted'" />
            <span v-if="!isCollapsed" class="truncate flex-1">Agenda</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-content-muted bg-surface px-1.5 py-0.5 rounded border border-borderbase">G A</span>
          </router-link>

          <router-link 
            to="/habits" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/habits' ? 'bg-surface-active text-content border-l-2 border-borderhighlight shadow-sm font-bold' : 'text-content-muted hover:text-content hover:bg-surface-hover'"
          >
            <RefreshCw class="w-4 h-4 flex-shrink-0" :class="route.path === '/habits' ? 'text-content' : 'text-content-muted'" />
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
            :class="route.path === '/projects' ? 'bg-surface-active text-content border-l-2 border-borderhighlight shadow-sm font-bold' : 'text-content-muted hover:text-content hover:bg-surface-hover'"
          >
            <Folder class="w-4 h-4 flex-shrink-0" :class="route.path === '/projects' ? 'text-content' : 'text-content-muted'" />
            <span v-if="!isCollapsed" class="truncate flex-1">Projetos</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-content-muted bg-surface px-1.5 py-0.5 rounded border border-borderbase">G P</span>
          </router-link>

          <router-link 
            to="/goals" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/goals' ? 'bg-surface-active text-content border-l-2 border-borderhighlight shadow-sm font-bold' : 'text-content-muted hover:text-content hover:bg-surface-hover'"
          >
            <Target class="w-4 h-4 flex-shrink-0" :class="route.path === '/goals' ? 'text-content' : 'text-content-muted'" />
            <span v-if="!isCollapsed" class="truncate flex-1">Metas</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-content-muted bg-surface px-1.5 py-0.5 rounded border border-borderbase">G G</span>
          </router-link>

          <!-- Botões do Sandbox/Tutorial Mantidos... -->
          <button 
            @click="handleSandboxClick" 
            type="button"
            class="w-full flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all group cursor-pointer text-left"
            :class="onboardingStore.isSandboxActive ? 'bg-surface-active text-content border-l-2 border-borderhighlight font-bold shadow-sm' : 'text-content-muted hover:text-content hover:bg-surface-hover'"
            :title="isCollapsed ? (onboardingStore.isSandboxActive ? 'Sair do Sandbox' : 'Ativar RAM Sandbox') : ''"
          >
            <span 
              class="p-0.5 rounded border flex-shrink-0 transition-colors"
              :class="onboardingStore.isSandboxActive ? 'bg-content text-content-invert border-content' : 'bg-app border-borderbase group-hover:border-borderfocus text-content-accent'"
            >
              <Terminal class="w-3 h-3" />
            </span>
            <div v-if="!isCollapsed" class="flex flex-col truncate flex-1">
              <span class="truncate leading-tight">{{ onboardingStore.isSandboxActive ? '[SAIR DO SANDBOX]' : '[RAM SANDBOX]' }}</span>
              <span class="text-[9px] font-mono" :class="onboardingStore.isSandboxActive ? 'text-content font-bold' : 'text-content-muted'">
                {{ onboardingStore.isSandboxActive ? '● Memória Ativa' : 'Simulador E2E' }}
              </span>
            </div>
          </button>
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
            :class="route.path === '/journal' ? 'bg-surface-active text-content border-l-2 border-borderhighlight shadow-sm font-bold' : 'text-content-muted hover:text-content hover:bg-surface-hover'"
          >
            <FileText class="w-4 h-4 flex-shrink-0" :class="route.path === '/journal' ? 'text-content' : 'text-content-muted'" />
            <span v-if="!isCollapsed" class="truncate flex-1">Auditoria</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-content-muted bg-surface px-1.5 py-0.5 rounded border border-borderbase">G J</span>
          </router-link>
          

          <!--  A NOVA TELA DE BIBLIOTECA (Documentation Hub) -->
          <router-link 
            to="/library" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/library' ? 'bg-surface-active text-content border-l-2 border-borderhighlight shadow-sm font-bold' : 'text-content-muted hover:text-content hover:bg-surface-hover'"
          >
            <BookOpen class="w-4 h-4 flex-shrink-0" :class="route.path === '/library' ? 'text-content' : 'text-content-muted'" />
            <span v-if="!isCollapsed" class="truncate flex-1">Biblioteca</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-content-muted bg-surface px-1.5 py-0.5 rounded border border-borderbase">G B</span>
          </router-link>

          <router-link 
            to="/settings" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/settings' ? 'bg-surface-active text-content border-l-2 border-borderhighlight shadow-sm font-bold' : 'text-content-muted hover:text-content hover:bg-surface-hover'"
          >
            <Sliders class="w-4 h-4 flex-shrink-0" :class="route.path === '/settings' ? 'text-content' : 'text-content-muted'" />
            <span v-if="!isCollapsed" class="truncate flex-1">Configurações</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-content-muted bg-surface px-1.5 py-0.5 rounded border border-borderbase">G S</span>
          </router-link>

          <router-link 
            to="/database" 
            class="flex items-center gap-3 px-2.5 py-2 text-sm font-medium rounded-tactic transition-all"
            :class="route.path === '/database' ? 'bg-surface-active text-content border-l-2 border-borderhighlight shadow-sm font-bold' : 'text-content-muted hover:text-content hover:bg-surface-hover'"
          >
            <Database class="w-4 h-4 flex-shrink-0" :class="route.path === '/database' ? 'text-content' : 'text-content-muted'" />
            <span v-if="!isCollapsed" class="truncate flex-1">Database</span>
            <span v-if="!isCollapsed" class="text-[10px] font-mono text-content-muted bg-surface px-1.5 py-0.5 rounded border border-borderbase">G D</span>
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