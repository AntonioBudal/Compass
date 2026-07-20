<script setup lang="ts">
import { ref, computed, watch, nextTick } from 'vue';
import { useRouter } from 'vue-router';
import { useCommitmentsStore } from '@/stores/commitmentsStore';
import { useDecisionStore } from '@/stores/decisionStore';
import { useJournalStore } from '@/stores/journalStore';
import { useSettingsStore } from '@/stores/settingsStore';
import { isCommandBarOpen, isQuickCaptureOpen } from '@/composables/useKeyboardShortcuts';
import { 
  Search, Zap, Calendar, Folder, Target, RefreshCw, 
  PlusCircle, FileText, Sliders, Power, Download, 
  CheckCircle2, CornerDownLeft 
} from 'lucide-vue-next';

const router = useRouter();
const commitmentsStore = useCommitmentsStore();
const decisionStore = useDecisionStore();
const journalStore = useJournalStore();
const settingsStore = useSettingsStore();

const searchQuery = ref('');
const selectedIndex = ref(0);
const inputRef = ref<HTMLInputElement | null>(null);

watch(isCommandBarOpen, async (isOpen) => {
  if (isOpen) {
    searchQuery.value = '';
    selectedIndex.value = 0;
    await nextTick();
    inputRef.value?.focus();
  }
});

interface CommandItem {
  id: string;
  title: string;
  subtitle?: string;
  icon: any;
  category: 'ação' | 'navegação' | 'sistema' | 'compromisso';
  action: () => void;
}

const allItems = computed<CommandItem[]>(() => {
  const query = searchQuery.value.toLowerCase().trim();
  const list: CommandItem[] = [];

  // 1. Ações de Sistema e Fechamento
  list.push({
    id: 'act-shutdown',
    title: 'Iniciar Fechamento de Turno (Daily Shutdown)',
    subtitle: 'Auditar pendências e encerrar o dia de trabalho',
    icon: Power,
    category: 'sistema',
    action: () => {
      isCommandBarOpen.value = false;
      journalStore.startShutdown();
    }
  });

  list.push({
    id: 'act-new',
    title: 'Criar Novo Compromisso',
    subtitle: 'Captura rápida de tarefa, hábito ou evento (Tecla C)',
    icon: PlusCircle,
    category: 'ação',
    action: () => {
      isCommandBarOpen.value = false;
      isQuickCaptureOpen.value = true;
    }
  });

  list.push({
    id: 'act-recalc',
    title: 'Recalcular Motor de Decisão (Now Engine)',
    subtitle: 'Atualizar sugestão de Top Focus baseada no tempo líquido',
    icon: RefreshCw,
    category: 'ação',
    action: () => {
      isCommandBarOpen.value = false;
      decisionStore.fetchNow();
      router.push('/now');
    }
  });

  list.push({
    id: 'act-export',
    title: 'Exportar Backup Local-First (.json)',
    subtitle: 'Gerar arquivo de portabilidade com todos os dados',
    icon: Download,
    category: 'sistema',
    action: () => {
      isCommandBarOpen.value = false;
      settingsStore.exportData();
    }
  });

  // 2. Navegação
  const navs = [
    { name: 'Agora (Motor de Decisão)', path: '/now', icon: Zap },
    { name: 'Agenda & Hard Blockers', path: '/agenda', icon: Calendar },
    { name: 'Projetos Ativos', path: '/projects', icon: Folder },
    { name: 'Metas Estratégicas', path: '/goals', icon: Target },
    { name: 'Hábitos Diários', path: '/habits', icon: RefreshCw },
    { name: 'Auditoria & Fechamento', path: '/journal', icon: FileText },
    { name: 'Configurações & Portabilidade', path: '/settings', icon: Sliders },
  ];

  navs.forEach(n => {
    list.push({
      id: `nav-${n.path}`,
      title: `Ir para ${n.name}`,
      icon: n.icon,
      category: 'navegação',
      action: () => {
        isCommandBarOpen.value = false;
        router.push(n.path);
      }
    });
  });

  // 3. Compromissos Locais
  commitmentsStore.items.forEach(item => {
    if (item.status !== 'COMPLETED' && item.status !== 'ARCHIVED') {
      list.push({
        id: `com-${item.id}`,
        title: item.title,
        subtitle: `[${item.type}] | ${item.estimatedDurationMinutes}m | Status: ${item.status}`,
        icon: CheckCircle2,
        category: 'compromisso',
        action: () => {
          isCommandBarOpen.value = false;
          if (item.type === 'TASK') {
            commitmentsStore.updateStatus(item.id, 'COMPLETED');
          }
        }
      });
    }
  });

  if (!query) return list;
  return list.filter(i => 
    i.title.toLowerCase().includes(query) || 
    (i.subtitle && i.subtitle.toLowerCase().includes(query))
  );
});

watch(searchQuery, () => {
  selectedIndex.value = 0;
});

const handleKeyDown = (e: KeyboardEvent) => {
  const max = allItems.value.length;
  if (max === 0) return;

  if (e.key === 'ArrowDown') {
    e.preventDefault();
    selectedIndex.value = (selectedIndex.value + 1) % max;
  } else if (e.key === 'ArrowUp') {
    e.preventDefault();
    selectedIndex.value = (selectedIndex.value - 1 + max) % max;
  } else if (e.key === 'Enter') {
    e.preventDefault();
    const selected = allItems.value[selectedIndex.value];
    if (selected) selected.action();
  }
};
</script>

<template>
  <transition name="modal-snap">
    <div 
      v-if="isCommandBarOpen" 
      class="fixed inset-0 z-50 flex items-start justify-center pt-[15vh] px-4 bg-black/70 backdrop-blur-sm select-none"
      @click.self="isCommandBarOpen = false"
    >
      <div 
        class="w-full max-w-2xl bg-zinc-900 border border-zinc-700/80 shadow-2xl rounded-xl overflow-hidden flex flex-col max-h-[60vh]"
        @keydown="handleKeyDown"
      >
        <!-- Input de Busca -->
        <div class="relative flex items-center px-4 border-b border-zinc-800">
          <Search class="w-5 h-5 text-zinc-500 flex-shrink-0" />
          <input 
            ref="inputRef"
            v-model="searchQuery"
            type="text" 
            placeholder="Digitar comando ou buscar tarefa..." 
            class="w-full py-4 px-3 bg-transparent text-sm text-zinc-100 placeholder:text-zinc-500 focus:outline-none font-sans"
          />
          <kbd class="px-1.5 py-0.5 text-[10px] font-mono bg-zinc-800 text-zinc-400 rounded border border-zinc-700">ESC</kbd>
        </div>

        <!-- Lista -->
        <div class="flex-1 overflow-y-auto p-2 space-y-1">
          <div v-if="allItems.length === 0" class="py-12 text-center text-xs font-mono text-zinc-500">
            Nenhum comando encontrado para "[{{ searchQuery }}]".
          </div>

          <button 
            v-for="(item, idx) in allItems" 
            :key="item.id"
            @click="item.action"
            @mouseenter="selectedIndex = idx"
            class="w-full flex items-center justify-between gap-3 px-3 py-2.5 rounded-lg text-left transition-colors cursor-pointer group"
            :class="selectedIndex === idx ? 'bg-zinc-800 text-white border-l-2 border-zinc-300' : 'text-zinc-400 hover:bg-zinc-800/40'"
          >
            <div class="flex items-center gap-3 min-w-0 flex-1">
              <component :is="item.icon" class="w-4 h-4 flex-shrink-0" :class="selectedIndex === idx ? 'text-zinc-100' : 'text-zinc-500'" />
              <div class="truncate">
                <span class="text-sm font-medium block truncate">{{ item.title }}</span>
                <span v-if="item.subtitle" class="text-xs text-zinc-500 block truncate font-mono mt-0.5">{{ item.subtitle }}</span>
              </div>
            </div>

            <div class="flex items-center gap-2 flex-shrink-0">
              <span class="text-[10px] font-mono uppercase px-1.5 py-0.5 rounded bg-zinc-950 border border-zinc-800 text-zinc-400">
                {{ item.category }}
              </span>
              <CornerDownLeft v-if="selectedIndex === idx" class="w-3.5 h-3.5 text-zinc-400 ml-1" />
            </div>
          </button>
        </div>

        <!-- Rodapé Monocromático -->
        <div class="px-4 py-2 bg-zinc-950 border-t border-zinc-800 flex items-center justify-between text-[11px] font-mono text-zinc-500">
          <div class="flex items-center gap-3">
            <span><kbd class="bg-zinc-900 border border-zinc-800 px-1 rounded text-zinc-400">↑↓</kbd> Navegar</span>
            <span><kbd class="bg-zinc-900 border border-zinc-800 px-1 rounded text-zinc-400">Enter</kbd> Executar</span>
          </div>
          <span>Compass K-Menu</span>
        </div>
      </div>
    </div>
  </transition>
</template>

<style scoped>
.modal-snap-enter-active,
.modal-snap-leave-active {
  transition: opacity 150ms cubic-bezier(0.16, 1, 0.3, 1), transform 150ms cubic-bezier(0.16, 1, 0.3, 1);
}
.modal-snap-enter-from,
.modal-snap-leave-to {
  opacity: 0;
  transform: scale(0.96);
}
</style>