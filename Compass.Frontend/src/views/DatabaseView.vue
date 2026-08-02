<script setup lang="ts">
import { ref, watch, onMounted, computed } from 'vue';
import { useCommitmentsStore } from '@/stores/commitmentsStore';
import { useProjectsStore } from '@/stores/projectsStore';
import { useInspectorStore } from '@/stores/inspectorStore';
import { useToastStore } from '@/stores/toastStore';
import PageHeader from '@/components/layout/PageHeader.vue';
import { 
  Database, Search, Trash2, Eye, CircleDashed, 
  Calendar, RefreshCw, FileText, Folder, Filter
} from 'lucide-vue-next';

const commitmentsStore = useCommitmentsStore();
const projectsStore = useProjectsStore();
const inspectorStore = useInspectorStore();
const toastStore = useToastStore();

// Estado dos Filtros
const filters = ref({
  search: '',
  type: '' as '' | 'TASK' | 'EVENT' | 'HABIT' | 'NOTE',
  status: '' as '' | 'PENDING' | 'IN_PROGRESS' | 'COMPLETED' | 'ARCHIVED',
  projectId: ''
});

// Busca inicial
onMounted(async () => {
  await projectsStore.fetchCatalog();
  await commitmentsStore.fetchDatabase(1, 100, filters.value);
});

// Reatividade: Refaz a busca automaticamente ao alterar os filtros (com debounce simples visual)
let debounceTimer: ReturnType<typeof setTimeout>;
watch(() => filters.value, (newFilters) => {
  clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => {
    commitmentsStore.fetchDatabase(1, 100, newFilters);
  }, 300);
}, { deep: true });

// --- Auxiliares Visuais ---

const getTypeIcon = (type: string) => {
  if (type === 'EVENT') return Calendar;
  if (type === 'HABIT') return RefreshCw;
  if (type === 'NOTE') return FileText;
  return CircleDashed; // Default para TASK
};

const getProjectName = (projectId: string | null) => {
  if (!projectId) return 'Avulso';
  const proj = projectsStore.catalog.find(p => p.id === projectId);
  return proj ? proj.name : 'Desconhecido';
};

const formatDate = (dateString: string | null) => {
  if (!dateString) return '--';
  const date = new Date(dateString);
  return date.toLocaleDateString('pt-BR', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' });
};

// Ação Segura de Deleção
const confirmDelete = async (id: string, title: string) => {
  if (confirm(`Tem certeza que deseja excluir permanentemente o registro "${title}"?`)) {
    try {
      await commitmentsStore.deleteCommitment(id);
      toastStore.showToast('Registro destruído.', 'success');
    } catch {
      toastStore.showToast('Falha ao excluir registro.', 'error');
    }
  }
};
</script>

<template>
  <div class="max-w-6xl mx-auto space-y-6 select-none pb-12">
    
    <PageHeader 
      title="Biblioteca de Compromissos"
      description="Visão administrativa de alta densidade (All Issues). Gerencie todos os registros do banco local."
      :actionIcon="Database"
      viewName="database"
      :badgeCount="commitmentsStore.databaseTotal"
      badgeLabel="Registros"
    />

    <!-- Barra de Ferramentas / Filtros -->
    <div class="p-3 bg-surface border border-borderbase rounded-xl flex flex-wrap items-center gap-3 shadow-sm">
      
      <!-- Busca Textual -->
      <div class="relative flex-1 min-w-[200px]">
        <Search class="w-4 h-4 text-content-muted absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none" />
        <input 
          v-model="filters.search" 
          type="text" 
          placeholder="Buscar por título..." 
          class="w-full pl-9 pr-3 py-2 bg-app border border-borderbase rounded-tactic text-xs font-medium text-content focus:border-borderfocus outline-none transition-colors"
        />
      </div>

      <!-- Filtro: Tipo -->
      <select v-model="filters.type" class="px-3 py-2 bg-app border border-borderbase rounded-tactic text-xs font-mono text-content-muted focus:border-borderfocus outline-none cursor-pointer">
        <option value="">[Tipos: Todos]</option>
        <option value="TASK">Tasks</option>
        <option value="HABIT">Habits</option>
        <option value="EVENT">Events</option>
        <option value="NOTE">Notes</option>
      </select>

      <!-- Filtro: Status -->
      <select v-model="filters.status" class="px-3 py-2 bg-app border border-borderbase rounded-tactic text-xs font-mono text-content-muted focus:border-borderfocus outline-none cursor-pointer">
        <option value="">[Status: Todos]</option>
        <option value="PENDING">Pendentes</option>
        <option value="IN_PROGRESS">Em Progresso</option>
        <option value="COMPLETED">Concluídos</option>
        <option value="ARCHIVED">Arquivados</option>
      </select>

      <!-- Filtro: Projeto -->
      <select v-model="filters.projectId" class="px-3 py-2 bg-app border border-borderbase rounded-tactic text-xs font-mono text-content-muted focus:border-borderfocus outline-none cursor-pointer max-w-[200px]">
        <option value="">[Projeto: Todos]</option>
        <option v-for="proj in projectsStore.catalog" :key="proj.id" :value="proj.id">
          {{ proj.name }}
        </option>
      </select>
      
      <button 
        @click="filters = { search: '', type: '', status: '', projectId: '' }"
        class="p-2 rounded-tactic hover:bg-surface-hover border border-transparent hover:border-borderbase text-content-muted transition-colors cursor-pointer"
        title="Limpar Filtros"
      >
        <Filter class="w-4 h-4" />
      </button>
    </div>

    <!-- Tabela de Alta Densidade (DataGrid) -->
    <div class="border border-borderbase rounded-xl overflow-hidden bg-app">
      <!-- Cabeçalho da Tabela -->
      <div class="grid grid-cols-12 gap-4 px-4 py-3 bg-surface border-b border-borderbase text-[10px] font-mono font-bold text-content-muted uppercase tracking-wider">
        <div class="col-span-5">Registro (Título)</div>
        <div class="col-span-2">Status</div>
        <div class="col-span-2">Projeto</div>
        <div class="col-span-2 hidden md:block">Agenda / Deadline</div>
        <div class="col-span-1 text-right">Ações</div>
      </div>

      <!-- Estado de Carregamento -->
      <div v-if="commitmentsStore.isDatabaseLoading" class="py-12 flex justify-center">
        <span class="text-xs font-mono text-content-muted animate-pulse">Consultando Banco de Dados...</span>
      </div>

      <!-- Estado Vazio -->
      <div v-else-if="commitmentsStore.databaseItems.length === 0" class="py-16 text-center text-xs font-mono text-content-muted flex flex-col items-center gap-2 bg-app">
        <Database class="w-6 h-6 text-content-muted opacity-50" />
        <span>Nenhum registro corresponde aos filtros atuais.</span>
      </div>

      <!-- Linhas da Tabela -->
      <div v-else class="divide-y divide-borderbase">
        <div 
          v-for="item in commitmentsStore.databaseItems" 
          :key="item.id"
          class="grid grid-cols-12 gap-4 px-4 py-3.5 items-center hover:bg-surface-hover transition-colors group"
        >
          <!-- 1. Ícone e Título -->
          <div class="col-span-5 flex items-center gap-3 min-w-0">
            <component 
              :is="getTypeIcon(item.type)" 
              class="w-4 h-4 flex-shrink-0"
              :class="{
                'text-status-danger-text': item.type === 'EVENT',
                'text-content-accent': item.type === 'HABIT',
                'text-content-muted': item.type === 'NOTE',
                'text-content': item.type === 'TASK'
              }" 
            />
            <span class="text-xs font-medium text-content truncate">{{ item.title }}</span>
          </div>

          <!-- 2. Status Badge -->
          <div class="col-span-2">
            <span 
              class="px-2 py-0.5 text-[9px] font-mono uppercase rounded font-bold"
              :class="{
                'bg-status-success-bg text-status-success-text border border-status-success-border': item.status === 'COMPLETED',
                'bg-surface-active text-content border border-borderfocus': item.status === 'IN_PROGRESS',
                'bg-app border border-borderbase text-content-muted': item.status === 'PENDING',
                'bg-status-warning-bg text-status-warning-text border border-status-warning-border opacity-70': item.status === 'ARCHIVED'
              }"
            >
              {{ item.status }}
            </span>
          </div>

          <!-- 3. Projeto -->
          <div class="col-span-2 text-[10px] font-mono text-content-muted truncate flex items-center gap-1.5">
            <Folder v-if="item.projectId" class="w-3 h-3" />
            <span :class="{'italic opacity-50': !item.projectId}">{{ getProjectName(item.projectId) }}</span>
          </div>

          <!-- 4. Data -->
          <div class="col-span-2 hidden md:block text-[10px] font-mono text-content-muted">
            {{ formatDate(item.startTime || item.deadline) }}
          </div>

          <!-- 5. Ações (Reveladas no Hover) -->
          <div class="col-span-1 flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
            <button 
              @click="inspectorStore.openInspector(item, 'COMMITMENT')"
              class="p-1.5 rounded bg-surface border border-borderbase hover:border-borderfocus text-content-muted hover:text-content transition-colors cursor-pointer"
              title="Inspecionar / Editar"
            >
              <Eye class="w-3.5 h-3.5" />
            </button>
            <button 
              @click="confirmDelete(item.id, item.title)"
              class="p-1.5 rounded bg-status-danger-bg/20 border border-transparent hover:border-status-danger-border text-status-danger-text transition-colors cursor-pointer"
              title="Excluir Permanentemente"
            >
              <Trash2 class="w-3.5 h-3.5" />
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>