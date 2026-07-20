<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useSettingsStore } from '@/stores/settingsStore';
import { 
  Sliders, Clock, Download, Upload, Trash2, 
  Keyboard, Check, AlertTriangle, FileJson 
} from 'lucide-vue-next';

const store = useSettingsStore();
const fileInputRef = ref<HTMLInputElement | null>(null);
const showResetConfirm = ref(false);

const localStart = ref('08:00');
const localEnd = ref('18:00');
const localEnergy = ref(2);
const localDuration = ref(30);

onMounted(async () => {
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
    <div class="pb-4 border-b border-zinc-800">
      <h1 class="text-2xl font-semibold text-zinc-100 tracking-tight flex items-center gap-2.5">
        <Sliders class="w-6 h-6 text-zinc-400" />
        <span>Configurações & Portabilidade</span>
      </h1>
      <p class="text-sm text-zinc-400 mt-1">
        Calibre as invariantes do motor de decisão, proteja seus dados locais e consulte a referência de atalhos.
      </p>
    </div>

    <!-- 1. Calibração do Algoritmo (Now Engine) -->
    <section class="space-y-4">
      <div class="flex items-center justify-between border-b border-zinc-800/60 pb-2">
        <h2 class="text-sm font-mono uppercase text-zinc-300 font-semibold tracking-wider flex items-center gap-2">
          <Clock class="w-4 h-4 text-zinc-500" /> 1. Calibração do Motor de Decisão
        </h2>
        <span class="text-[11px] font-mono text-zinc-500">Afeta o cálculo líquido de M(tempo)</span>
      </div>

      <div class="p-6 rounded-xl border border-zinc-800 bg-zinc-900/40 space-y-6">
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-6">
          <div>
            <label class="block text-xs font-mono uppercase text-zinc-400 mb-2">Início do Turno Útil</label>
            <input 
              v-model="localStart"
              type="time" 
              class="w-full px-3 py-2 bg-zinc-950 border border-zinc-800 rounded-tactic text-sm font-mono text-zinc-100 focus:border-zinc-600 outline-none"
            />
          </div>

          <div>
            <label class="block text-xs font-mono uppercase text-zinc-400 mb-2">Encerramento do Turno</label>
            <input 
              v-model="localEnd"
              type="time" 
              class="w-full px-3 py-2 bg-zinc-950 border border-zinc-800 rounded-tactic text-sm font-mono text-zinc-100 focus:border-zinc-600 outline-none"
            />
          </div>

          <div>
            <label class="block text-xs font-mono uppercase text-zinc-400 mb-2">Energia Padrão (Novas Tarefas)</label>
            <select 
              v-model.number="localEnergy"
              class="w-full px-3 py-2 bg-zinc-950 border border-zinc-800 rounded-tactic text-sm font-mono text-zinc-100 focus:border-zinc-600 outline-none"
            >
              <option :value="1">■□□ MAINT (1)</option>
              <option :value="2">■■□ OPER (2)</option>
              <option :value="3">■■■ DEEP (3)</option>
            </select>
          </div>

          <div>
            <label class="block text-xs font-mono uppercase text-zinc-400 mb-2">Duração Padrão Estimada</label>
            <div class="relative flex items-center">
              <input 
                v-model.number="localDuration"
                type="number" 
                step="15"
                class="w-full pl-3 pr-10 py-2 bg-zinc-950 border border-zinc-800 rounded-tactic text-sm font-mono text-zinc-100 focus:border-zinc-600 outline-none"
              />
              <span class="absolute right-3 text-xs font-mono text-zinc-500">min</span>
            </div>
          </div>
        </div>

        <div class="pt-2 flex justify-end">
          <button 
            @click="handleSave"
            :disabled="store.isSubmitting"
            class="inline-flex items-center gap-2 px-4 py-2 rounded-tactic bg-zinc-100 hover:bg-white text-zinc-950 text-xs font-semibold shadow-sm transition-all disabled:opacity-50 cursor-pointer"
          >
            <Check class="w-4 h-4 stroke-[2.5]" />
            <span>Salvar Calibração</span>
          </button>
        </div>
      </div>
    </section>

    <!-- 2. Portabilidade e Gerenciamento de Dados -->
    <section class="space-y-4">
      <div class="flex items-center justify-between border-b border-zinc-800/60 pb-2">
        <h2 class="text-sm font-mono uppercase text-zinc-300 font-semibold tracking-wider flex items-center gap-2">
          <FileJson class="w-4 h-4 text-zinc-500" /> 2. Portabilidade Local-First (.json)
        </h2>
        <span class="text-[11px] font-mono text-zinc-500">Single Source of Truth: .NET 10 DB</span>
      </div>

      <div class="p-6 rounded-xl border border-zinc-800 bg-zinc-900/40 space-y-6">
        <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-6 border-b border-zinc-800">
          <div class="space-y-1">
            <h3 class="text-sm font-medium text-zinc-200">Exportar Base de Compromissos</h3>
            <p class="text-xs text-zinc-500">Gera um arquivo .json completo contendo tarefas, histórico de streaks e metas vinculadas.</p>
          </div>
          <button 
            @click="store.exportData"
            class="inline-flex items-center gap-2 px-4 py-2 rounded-tactic bg-zinc-900 hover:bg-zinc-800 border border-zinc-700 text-zinc-200 text-xs font-medium transition-all cursor-pointer flex-shrink-0"
          >
            <Download class="w-3.5 h-3.5" />
            <span>Exportar Backup</span>
          </button>
        </div>

        <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-6 border-b border-zinc-800">
          <div class="space-y-1">
            <h3 class="text-sm font-medium text-zinc-200">Importar e Restaurar Backup</h3>
            <p class="text-xs text-zinc-500">Substitui ou mescla a base local do .NET utilizando um arquivo de backup previamente exportado.</p>
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
              class="inline-flex items-center gap-2 px-4 py-2 rounded-tactic bg-zinc-900 hover:bg-zinc-800 border border-zinc-700 text-zinc-200 text-xs font-medium transition-all cursor-pointer flex-shrink-0 disabled:opacity-50"
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
              <h3 class="text-sm font-medium text-rose-400">Zona de Perigo: Resetar Banco de Dados</h3>
              <p class="text-xs text-zinc-500">Exclui permanentemente todos os compromissos, hábitos e metas registrados na máquina local.</p>
            </div>
            <button 
              @click="showResetConfirm = true"
              class="inline-flex items-center gap-2 px-3 py-1.5 rounded-tactic bg-rose-500/10 hover:bg-rose-500/20 text-rose-400 border border-rose-500/30 text-xs font-medium transition-colors cursor-pointer flex-shrink-0"
            >
              <Trash2 class="w-3.5 h-3.5" />
              <span>Resetar Dados</span>
            </button>
          </div>

          <!-- Confirmação Inline de Segurança -->
          <div v-else class="p-4 rounded-lg bg-rose-950/20 border border-rose-500/40 flex flex-col sm:flex-row items-center justify-between gap-4">
            <div class="flex items-center gap-2.5 text-xs text-rose-300 font-mono">
              <AlertTriangle class="w-4 h-4 text-rose-400 flex-shrink-0" />
              <span>Confirma a destruição total da base local? Esta ação não pode ser desfeita.</span>
            </div>
            <div class="flex items-center gap-2 flex-shrink-0">
              <button 
                @click="showResetConfirm = false"
                class="px-3 py-1.5 rounded bg-zinc-900 hover:bg-zinc-800 text-zinc-300 text-xs font-medium transition-colors cursor-pointer"
              >
                Cancelar
              </button>
              <button 
                @click="handleResetConfirm"
                :disabled="store.isSubmitting"
                class="px-3 py-1.5 rounded bg-rose-600 hover:bg-rose-500 text-white text-xs font-semibold transition-colors cursor-pointer disabled:opacity-50"
              >
                Sim, Destruir Tudo
              </button>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- 3. Catálogo de Atalhos (Keyboard Reference) -->
    <section class="space-y-4">
      <div class="flex items-center justify-between border-b border-zinc-800/60 pb-2">
        <h2 class="text-sm font-mono uppercase text-zinc-300 font-semibold tracking-wider flex items-center gap-2">
          <Keyboard class="w-4 h-4 text-zinc-500" /> 3. Catálogo de Atalhos de Teclado
        </h2>
        <span class="text-[11px] font-mono text-zinc-500">Filosofia Zero-Mouse</span>
      </div>

      <div class="border border-zinc-800 rounded-xl overflow-hidden bg-zinc-900/40">
        <div class="grid grid-cols-12 gap-4 px-4 py-2.5 bg-zinc-900/80 border-b border-zinc-800 text-[11px] font-mono font-semibold text-zinc-400 uppercase tracking-wider">
          <div class="col-span-4 sm:col-span-3">Comando / Tecla</div>
          <div class="col-span-8 sm:col-span-9">Ação no Sistema</div>
        </div>

        <div class="divide-y divide-zinc-800/60">
          <div 
            v-for="shortcut in shortcuts" 
            :key="shortcut.key"
            class="grid grid-cols-12 gap-4 py-3 px-4 items-center hover:bg-zinc-900/60 transition-colors"
          >
            <div class="col-span-4 sm:col-span-3">
              <kbd class="px-2 py-1 bg-zinc-950 border border-zinc-700/80 rounded text-xs font-mono text-zinc-200 shadow-sm inline-block">
                {{ shortcut.key }}
              </kbd>
            </div>
            <div class="col-span-8 sm:col-span-9 text-xs text-zinc-400">
              {{ shortcut.description }}
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>