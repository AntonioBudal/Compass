<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useSettingsStore } from '@/stores/settingsStore';
import { useThemeStore, THEME_OPTIONS } from '@/stores/themeStore';
import { 
  Sliders, Clock, Download, Upload, Trash2, 
  Keyboard, Check, AlertTriangle, FileJson, Palette 
} from 'lucide-vue-next';

const store = useSettingsStore();
const themeStore = useThemeStore();
const fileInputRef = ref<HTMLInputElement | null>(null);
const showResetConfirm = ref(false);

const localStart = ref('08:00');
const localEnd = ref('18:00');
const localEnergy = ref(2);
const localDuration = ref(30);

onMounted(async () => {
  themeStore.initTheme();
  await store.fetchSettings();
  localStart.value = store.settings.workDayStart;
  localEnd.value = store.settings.workDayEnd;
  localEnergy.value = store.settings.defaultEnergy;
  localDuration.value = store.settings.defaultDurationMinutes;
});

const handleSave = async () => {
  await store.saveSettings({
    workDayStart: localStart.value,
    workDayEnd: localEnd.value,
    defaultEnergy: Number(localEnergy.value),
    defaultDurationMinutes: Number(localDuration.value)
  });
};

const triggerFileUpload = () => {
  fileInputRef.value?.click();
};

const handleFileChange = async (event: Event) => {
  const target = event.target as HTMLInputElement;
  if (target.files && target.files.length > 0) {
    await store.importData(target.files[0]);
    target.value = '';
  }
};

const handleResetConfirm = async () => {
  showResetConfirm.value = false;
  await store.resetAllData();
};

const shortcuts = [
  { key: 'Cmd + K / Ctrl + K', description: 'Abrir barra de comando global (K-Menu)' },
  { key: 'C', description: 'Abrir modal de captura rápida de compromissos' },
  { key: 'E', description: 'Concluir imediatamente o Top Focus atual' },
  { key: 'S', description: 'Adiar o Top Focus para o final da fila' },
  { key: 'Cmd + Z / Ctrl + Z', description: 'Desfazer a última mutação de estado ou exclusão' },
  { key: 'G -> N', description: 'Navegar para a tela Agora (Now Engine)' },
  { key: 'G -> A', description: 'Navegar para a Agenda e Hard Blockers' },
  { key: 'G -> P', description: 'Navegar para a grade de Projetos Ativos' },
  { key: 'G -> G', description: 'Navegar para as Metas Estratégicas' },
  { key: 'G -> H', description: 'Navegar para o monitor de Hábitos Diários' },
];
</script>

<template>
  <div class="max-w-4xl mx-auto space-y-10 select-none pb-12">
    <!-- Cabeçalho -->
    <div class="pb-4 border-b border-borderbase">
      <h1 class="text-2xl font-semibold text-content tracking-tight flex items-center gap-2.5">
        <Sliders class="w-6 h-6 text-content-muted" />
        <span>Configurações & Portabilidade</span>
      </h1>
      <p class="text-sm text-content-muted mt-1">
        Calibre as invariantes do motor de decisão, proteja seus dados locais e consulte a referência de atalhos.
      </p>
    </div>

    <!-- 1. Arquitetura de Interface & Temas -->
    <section class="space-y-4">
      <div class="flex items-center justify-between border-b border-borderbase pb-2">
        <h2 class="text-sm font-mono uppercase text-content font-semibold tracking-wider flex items-center gap-2">
          <Palette class="w-4 h-4 text-content-muted" /> 1. Arquitetura de Interface & Temas
        </h2>
        <span class="text-[11px] font-mono text-content-muted">Design Tokens Dinâmicos</span>
      </div>

      <p class="text-xs text-content-muted leading-relaxed">
        Selecione o motor de renderização cromática do ecossistema. Alterações são aplicadas em latência zero via Design Tokens.
      </p>

      <!-- Grid de Seletor de Temas -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div
          v-for="theme in THEME_OPTIONS"
          :key="theme.id"
          @click="themeStore.setTheme(theme.id)"
          class="p-4 rounded-lg border transition-all cursor-pointer flex flex-col justify-between group relative overflow-hidden"
          :class="themeStore.currentTheme === theme.id ? 'bg-surface-active border-borderhighlight shadow-lg' : 'bg-surface border-borderbase hover:border-borderfocus hover:bg-surface-hover'"
        >
          <!-- Topo do Card: Informações do Tema -->
          <div class="flex items-start justify-between gap-4">
            <div>
              <div class="flex items-center gap-2">
                <span class="text-sm font-bold text-content tracking-wide">{{ theme.name }}</span>
                <span 
                  v-if="themeStore.currentTheme === theme.id" 
                  class="px-1.5 py-0.5 rounded text-[9px] font-mono font-bold bg-status-success-bg text-status-success-text border border-status-success-border uppercase"
                >
                  [ATIVO]
                </span>
              </div>
              <p class="text-xs text-content-muted mt-1.5 leading-relaxed">{{ theme.description }}</p>
            </div>

            <!-- Ícone de Check Tático -->
            <div 
              class="w-5 h-5 rounded-full flex items-center justify-center border transition-colors flex-shrink-0"
              :class="themeStore.currentTheme === theme.id ? 'border-borderhighlight text-content' : 'border-borderbase text-transparent group-hover:border-borderfocus'"
            >
              <Check class="w-3 h-3 stroke-[3]" />
            </div>
          </div>

          <!-- Rodapé do Card: Paleta de Amostra Visual (Mini-Preview) -->
          <div class="mt-6 pt-3 border-t border-borderbase/50 flex items-center justify-between">
            <span class="text-[10px] font-mono text-content-muted uppercase tracking-wider">Design Tokens:</span>
            
            <div class="flex items-center gap-1.5 p-1 rounded bg-app border border-borderbase">
              <!-- Amostra BG -->
              <div class="w-4 h-4 rounded-sm border border-borderbase" :style="{ backgroundColor: theme.preview.bg }" title="Fundo (--bg-app)" />
              <!-- Amostra Surface -->
              <div class="w-4 h-4 rounded-sm border border-borderbase" :style="{ backgroundColor: theme.preview.surface }" title="Superfície (--bg-surface)" />
              <!-- Amostra Accent -->
              <div class="w-4 h-4 rounded-sm border border-borderbase" :style="{ backgroundColor: theme.preview.accent }" title="Acento (--text-accent)" />
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- 2. Calibração do Algoritmo (Now Engine) -->
    <section class="space-y-4">
      <div class="flex items-center justify-between border-b border-borderbase pb-2">
        <h2 class="text-sm font-mono uppercase text-content font-semibold tracking-wider flex items-center gap-2">
          <Clock class="w-4 h-4 text-content-muted" /> 2. Calibração do Motor de Decisão
        </h2>
        <span class="text-[11px] font-mono text-content-muted">Afeta o cálculo líquido de M(tempo)</span>
      </div>

      <div class="p-6 rounded-xl border border-borderbase bg-surface space-y-6">
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-6">
          <div>
            <label class="block text-xs font-mono uppercase text-content-muted mb-2">Início do Turno Útil</label>
            <input 
              v-model="localStart"
              type="time" 
              class="w-full px-3 py-2 bg-app border border-borderbase rounded-tactic text-sm font-mono text-content focus:border-borderfocus outline-none"
            />
          </div>

          <div>
            <label class="block text-xs font-mono uppercase text-content-muted mb-2">Encerramento do Turno</label>
            <input 
              v-model="localEnd"
              type="time" 
              class="w-full px-3 py-2 bg-app border border-borderbase rounded-tactic text-sm font-mono text-content focus:border-borderfocus outline-none"
            />
          </div>

          <div>
            <label class="block text-xs font-mono uppercase text-content-muted mb-2">Energia Padrão (Novas Tarefas)</label>
            <select 
              v-model.number="localEnergy"
              class="w-full px-3 py-2 bg-app border border-borderbase rounded-tactic text-sm font-mono text-content focus:border-borderfocus outline-none"
            >
              <option :value="1">■□□ MAINT (1)</option>
              <option :value="2">■■□ OPER (2)</option>
              <option :value="3">■■■ DEEP (3)</option>
            </select>
          </div>

          <div>
            <label class="block text-xs font-mono uppercase text-content-muted mb-2">Duração Padrão Estimada</label>
            <div class="relative flex items-center">
              <input 
                v-model.number="localDuration"
                type="number" 
                step="15"
                class="w-full pl-3 pr-10 py-2 bg-app border border-borderbase rounded-tactic text-sm font-mono text-content focus:border-borderfocus outline-none"
              />
              <span class="absolute right-3 text-xs font-mono text-content-muted">min</span>
            </div>
          </div>
        </div>

        <div class="pt-2 flex justify-end">
          <button 
            @click="handleSave"
            :disabled="store.isSubmitting"
            class="inline-flex items-center gap-2 px-4 py-2 rounded-tactic bg-content hover:bg-content-accent text-content-invert text-xs font-semibold shadow-sm transition-all disabled:opacity-50 cursor-pointer"
          >
            <Check class="w-4 h-4 stroke-[2.5]" />
            <span>Salvar Calibração</span>
          </button>
        </div>
      </div>
    </section>

    <!-- 3. Portabilidade e Gerenciamento de Dados -->
    <section class="space-y-4">
      <div class="flex items-center justify-between border-b border-borderbase pb-2">
        <h2 class="text-sm font-mono uppercase text-content font-semibold tracking-wider flex items-center gap-2">
          <FileJson class="w-4 h-4 text-content-muted" /> 3. Portabilidade Local-First (.json)
        </h2>
        <span class="text-[11px] font-mono text-content-muted">Single Source of Truth: .NET 10 DB</span>
      </div>

      <div class="p-6 rounded-xl border border-borderbase bg-surface space-y-6">
        <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-6 border-b border-borderbase">
          <div class="space-y-1">
            <h3 class="text-sm font-medium text-content">Exportar Base de Compromissos</h3>
            <p class="text-xs text-content-muted">Gera um arquivo .json completo contendo tarefas, histórico de streaks e metas vinculadas.</p>
          </div>
          <button 
            @click="store.exportData"
            class="inline-flex items-center gap-2 px-4 py-2 rounded-tactic bg-surface hover:bg-surface-hover border border-borderbase text-content text-xs font-medium transition-all cursor-pointer flex-shrink-0"
          >
            <Download class="w-3.5 h-3.5" />
            <span>Exportar Backup</span>
          </button>
        </div>

        <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-6 border-b border-borderbase">
          <div class="space-y-1">
            <h3 class="text-sm font-medium text-content">Importar e Restaurar Backup</h3>
            <p class="text-xs text-content-muted">Substitui ou mescla a base local do .NET utilizando um arquivo de backup previamente exportado.</p>
          </div>
          <div>
            <input 
              ref="fileInputRef" 
              type="file" 
              accept=".json" 
              class="hidden" 
              @change="handleFileChange" 
            />
            <button 
              @click="triggerFileUpload"
              :disabled="store.isSubmitting"
              class="inline-flex items-center gap-2 px-4 py-2 rounded-tactic bg-surface hover:bg-surface-hover border border-borderbase text-content text-xs font-medium transition-all cursor-pointer flex-shrink-0 disabled:opacity-50"
            >
              <Upload class="w-3.5 h-3.5" />
              <span>Importar Arquivo .json</span>
            </button>
          </div>
        </div>

        <!-- Zona de Perigo -->
        <div class="pt-2">
          <div v-if="!showResetConfirm" class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
            <div class="space-y-1">
              <h3 class="text-sm font-medium text-status-danger-text">Zona de Perigo: Resetar Banco de Dados</h3>
              <p class="text-xs text-content-muted">Exclui permanentemente todos os compromissos, hábitos e metas registrados na máquina local.</p>
            </div>
            <button 
              @click="showResetConfirm = true"
              class="inline-flex items-center gap-2 px-3 py-1.5 rounded-tactic bg-status-danger-bg hover:opacity-80 text-status-danger-text border border-status-danger-border text-xs font-medium transition-colors cursor-pointer flex-shrink-0"
            >
              <Trash2 class="w-3.5 h-3.5" />
              <span>Resetar Dados</span>
            </button>
          </div>

          <!-- Confirmação Inline de Segurança -->
          <div v-else class="p-4 rounded-lg bg-status-danger-bg border border-status-danger-border flex flex-col sm:flex-row items-center justify-between gap-4">
            <div class="flex items-center gap-2.5 text-xs text-status-danger-text font-mono">
              <AlertTriangle class="w-4 h-4 text-status-danger-text flex-shrink-0" />
              <span>Confirma a destruição total da base local? Esta ação não pode ser desfeita.</span>
            </div>
            <div class="flex items-center gap-2 flex-shrink-0">
              <button 
                @click="showResetConfirm = false"
                class="px-3 py-1.5 rounded bg-surface hover:bg-surface-hover text-content-muted hover:text-content text-xs font-medium transition-colors cursor-pointer"
              >
                Cancelar
              </button>
              <button 
                @click="handleResetConfirm"
                :disabled="store.isSubmitting"
                class="px-3 py-1.5 rounded bg-status-danger-bg hover:opacity-80 text-status-danger-text border border-status-danger-border text-xs font-semibold transition-colors cursor-pointer disabled:opacity-50"
              >
                Sim, Destruir Tudo
              </button>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- 4. Catálogo de Atalhos (Keyboard Reference) -->
    <section class="space-y-4">
      <div class="flex items-center justify-between border-b border-borderbase pb-2">
        <h2 class="text-sm font-mono uppercase text-content font-semibold tracking-wider flex items-center gap-2">
          <Keyboard class="w-4 h-4 text-content-muted" /> 4. Catálogo de Atalhos de Teclado
        </h2>
        <span class="text-[11px] font-mono text-content-muted">Filosofia Zero-Mouse</span>
      </div>

      <div class="border border-borderbase rounded-xl overflow-hidden bg-surface">
        <div class="grid grid-cols-12 gap-4 px-4 py-2.5 bg-surface border-b border-borderbase text-[11px] font-mono font-semibold text-content-muted uppercase tracking-wider">
          <div class="col-span-4 sm:col-span-3">Comando / Tecla</div>
          <div class="col-span-8 sm:col-span-9">Ação no Sistema</div>
        </div>

        <div class="divide-y divide-borderbase">
          <div 
            v-for="shortcut in shortcuts" 
            :key="shortcut.key"
            class="grid grid-cols-12 gap-4 py-3 px-4 items-center hover:bg-surface-hover transition-colors"
          >
            <div class="col-span-4 sm:col-span-3">
              <kbd class="px-2 py-1 bg-app border border-borderbase rounded text-xs font-mono text-content shadow-sm inline-block">
                {{ shortcut.key }}
              </kbd>
            </div>
            <div class="col-span-8 sm:col-span-9 text-xs text-content-muted">
              {{ shortcut.description }}
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>