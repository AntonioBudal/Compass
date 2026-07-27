<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { useDailyCycleStore } from '@/stores/dailyCycleStore';
import { useDecisionStore } from '@/stores/decisionStore';
import { Sun, Moon, CheckCircle2, AlertTriangle, Clock, Tag, Play, Square, ArrowRight, X, Sparkles } from 'lucide-vue-next';

const props = defineProps<{
  isOpen: boolean;
  initialMode?: 'morning' | 'evening';
}>();

const emit = defineEmits<{
  (e: 'close'): void;
}>();

const dailyCycleStore = useDailyCycleStore();
const decisionStore = useDecisionStore();

// --- Estado Reativo do Ritual ---
const mode = ref<'morning' | 'evening'>(props.initialMode || (new Date().getHours() < 14 ? 'morning' : 'evening'));
const completedCount = ref(0);
const postponedCount = ref(0);
const focusMinutes = ref(120);
const notes = ref('');
const selectedTags = ref<string[]>([]);

// Catálogo de Tags de Divergência Algorítmica (Mapeadas para as teclas 1 a 5)
const availableTags = [
  { id: '#underestimated', label: '1. Subestimado', desc: 'Tarefas levaram mais tempo que o previsto' },
  { id: '#interrupted', label: '2. Interrompido', desc: 'Excesso de reuniões ou demandas externas' },
  { id: '#low-energy', label: '3. Baixa Energia', desc: 'Fadiga cognitiva incompatível com o plano' },
  { id: '#flow', label: '4. Estado de Flow', desc: 'Hiperfoco e entregas acima da estimativa' },
  { id: '#scope-creep', label: '5. Escopo Expandido', desc: 'Requisitos mudaram durante a execução' }
];

// --- Ações Táticas ---
function toggleTag(tagId: string) {
  const idx = selectedTags.value.indexOf(tagId);
  if (idx > -1) {
    selectedTags.value.splice(idx, 1);
  } else {
    selectedTags.value.push(tagId);
  }
}

async function handleSubmit() {
  if (mode.value === 'morning') {
    // No briefing matinal, confirmar inicia o motor e fecha o modal
    await decisionStore.fetchDecisions(60, 2, true);
    emit('close');
  } else {
    // No encerramento, envia o payload transacional
    const success = await dailyCycleStore.executeShutdown({
      completedCount: completedCount.value,
      postponedCount: postponedCount.value,
      totalFocusMinutes: focusMinutes.value,
      notes: notes.value,
      divergenceTags: selectedTags.value
    });
    if (success) {
      emit('close');
    }
  }
}

// --- Controle 100% Zero-Mouse (Teclado Tático) ---
function handleKeyDown(e: KeyboardEvent) {
  if (!props.isOpen) return;

  // Esc fecha o modal
  if (e.key === 'Escape') {
    emit('close');
    return;
  }

  // Ctrl+Enter ou Cmd+Enter submete o ritual
  if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
    e.preventDefault();
    handleSubmit();
    return;
  }

  // Se estiver digitando no Textarea, ignora atalhos de tag
  const isTyping = document.activeElement?.tagName === 'TEXTAREA' || document.activeElement?.tagName === 'INPUT';
  
  if (!isTyping && mode.value === 'evening') {
    // Teclas 1 a 5 alternam as tags de divergência
    if (['1', '2', '3', '4', '5'].includes(e.key)) {
      e.preventDefault();
      const index = parseInt(e.key) - 1;
      if (availableTags[index]) {
        toggleTag(availableTags[index].id);
      }
    }
  }

  // Alternar modo com Tab se não estiver digitando
  if (!isTyping && (e.key === 'q' || e.key === 'Q')) {
    mode.value = mode.value === 'morning' ? 'evening' : 'morning';
  }
}

onMounted(() => {
  window.addEventListener('keydown', handleKeyDown);
  if (props.isOpen && mode.value === 'morning') {
    dailyCycleStore.fetchMorningBriefing();
  }
});

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeyDown);
});
</script>

<template>
  <Teleport to="body">
    <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-app/80 backdrop-blur-sm animate-fade-in font-mono select-none">
      
      <!-- Container Principal Monocromático -->
      <div class="w-full max-w-xl bg-surface border-2 border-borderfocus rounded-xl shadow-2xl overflow-hidden flex flex-col text-content transition-all duration-200 gpu-accelerated">
        
        <!-- Cabeçalho com Seletor de Modo -->
        <div class="flex items-center justify-between px-6 py-4 border-b border-borderbase bg-surface-hover">
          <div class="flex items-center gap-3">
            <button 
              @click="mode = 'morning'" 
              class="flex items-center gap-2 px-3 py-1 rounded text-xs font-bold uppercase tracking-wider transition-colors cursor-pointer border"
              :class="mode === 'morning' ? 'bg-content text-content-invert border-content' : 'bg-transparent text-content-muted border-transparent hover:border-borderbase'"
            >
              <Sun class="w-3.5 h-3.5" />
              <span>Morning Briefing</span>
            </button>
            <button 
              @click="mode = 'evening'" 
              class="flex items-center gap-2 px-3 py-1 rounded text-xs font-bold uppercase tracking-wider transition-colors cursor-pointer border"
              :class="mode === 'evening' ? 'bg-content text-content-invert border-content' : 'bg-transparent text-content-muted border-transparent hover:border-borderbase'"
            >
              <Moon class="w-3.5 h-3.5" />
              <span>Evening Review</span>
            </button>
          </div>
          <button @click="emit('close')" class="text-content-muted hover:text-content p-1 rounded transition-colors cursor-pointer">
            <X class="w-5 h-5" />
          </button>
        </div>

        <!-- MODO 1: MORNING BRIEFING -->
        <div v-if="mode === 'morning'" class="p-6 flex flex-col gap-6">
          <div class="flex items-start justify-between">
            <div>
              <h2 class="text-2xl font-sans font-extrabold text-content">{{ dailyCycleStore.briefing?.greetingMessage || 'Bom dia, Operador.' }}</h2>
              <p class="text-xs text-content-muted mt-1">Aqui está a projeção analítica para a sua jornada de hoje:</p>
            </div>
            <span class="px-2.5 py-1 text-xs font-bold bg-surface-active border border-borderbase rounded uppercase">
              {{ dailyCycleStore.briefing?.date || new Date().toISOString().slice(0,10) }}
            </span>
          </div>

          <!-- Grid de Métricas do Dia -->
          <div class="grid grid-cols-3 gap-3">
            <div class="p-3.5 bg-app/50 border border-borderbase rounded-lg flex flex-col justify-between">
              <span class="text-[10px] text-content-muted uppercase">Pendências</span>
              <span class="text-2xl font-bold font-sans mt-1">{{ dailyCycleStore.briefing?.pendingTasksCount || 0 }}</span>
            </div>
            <div class="p-3.5 bg-app/50 border border-borderbase rounded-lg flex flex-col justify-between" :class="{'border-borderfocus': (dailyCycleStore.briefing?.overdueTasksCount || 0) > 0}">
              <span class="text-[10px] text-content-muted uppercase flex items-center gap-1">
                <AlertTriangle v-if="(dailyCycleStore.briefing?.overdueTasksCount || 0) > 0" class="w-3 h-3" />
                <span>Atrasadas</span>
              </span>
              <span class="text-2xl font-bold font-sans mt-1">{{ dailyCycleStore.briefing?.overdueTasksCount || 0 }}</span>
            </div>
            <div class="p-3.5 bg-app/50 border border-borderbase rounded-lg flex flex-col justify-between">
              <span class="text-[10px] text-content-muted uppercase">Carga Estimada</span>
              <span class="text-xl font-bold font-sans mt-1">{{ dailyCycleStore.briefing?.totalEstimatedFocusMinutes || 0 }}m</span>
            </div>
          </div>

          <!-- Recomendação de Foco Primário -->
          <div class="p-4 bg-surface-hover border border-borderfocus/60 rounded-lg flex items-center justify-between">
            <div class="flex items-center gap-3">
              <Sparkles class="w-5 h-5 text-content flex-shrink-0" />
              <div>
                <div class="text-[10px] font-bold uppercase tracking-wider text-content-accent">Alvo Prioritário Sugerido</div>
                <div class="text-sm font-sans font-bold text-content mt-0.5">{{ dailyCycleStore.briefing?.topFocusTitle || 'Nenhuma pendência crítica' }}</div>
              </div>
            </div>
          </div>
        </div>

        <!-- MODO 2: EVENING REVIEW (SHUTDOWN) -->
        <div v-else class="p-6 flex flex-col gap-5 max-h-[70vh] overflow-y-auto">
          <div>
            <h2 class="text-xl font-sans font-extrabold text-content">Fechamento Tático do Dia</h2>
            <p class="text-xs text-content-muted mt-0.5">Mensure sua execução real para calibrar o EAI (Índice de Acurácia):</p>
          </div>

          <!-- Controles de Execução (Completed / Postponed / Focus Time) -->
          <div class="grid grid-cols-3 gap-3">
            <div class="flex flex-col gap-1">
              <label class="text-[10px] uppercase text-content-muted font-bold">Concluídas</label>
              <input type="number" v-model="completedCount" min="0" class="bg-app border border-borderbase rounded p-2 text-sm font-bold text-content focus:border-borderfocus outline-none" />
            </div>
            <div class="flex flex-col gap-1">
              <label class="text-[10px] uppercase text-content-muted font-bold">Adiados (+15m)</label>
              <input type="number" v-model="postponedCount" min="0" class="bg-app border border-borderbase rounded p-2 text-sm font-bold text-content focus:border-borderfocus outline-none" />
            </div>
            <div class="flex flex-col gap-1">
              <label class="text-[10px] uppercase text-content-muted font-bold">Foco Real (Minutos)</label>
              <input type="number" v-model="focusMinutes" min="0" step="15" class="bg-app border border-borderbase rounded p-2 text-sm font-bold text-content focus:border-borderfocus outline-none" />
            </div>
          </div>

          <!-- SELETOR DE TAGS DE DIVERGÊNCIA (Atalhos Teclado 1-5) -->
          <div class="flex flex-col gap-2">
            <div class="flex items-center justify-between text-xs">
              <span class="font-bold text-content flex items-center gap-1.5">
                <Tag class="w-3.5 h-3.5" />
                <span>Tags de Divergência Algorítmica</span>
              </span>
              <span class="text-[10px] text-content-muted font-mono">[Use teclas 1 a 5 para alternar]</span>
            </div>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-2">
              <button 
                v-for="tag in availableTags" 
                :key="tag.id"
                @click="toggleTag(tag.id)"
                type="button"
                class="flex items-center justify-between p-2 rounded text-left border transition-all duration-150 cursor-pointer text-xs"
                :class="selectedTags.includes(tag.id) ? 'bg-content text-content-invert border-content font-bold shadow-sm' : 'bg-app/50 text-content-muted border-borderbase hover:border-borderfocus/60'"
                :title="tag.desc"
              >
                <span>{{ tag.label }}</span>
                <span class="text-[10px] opacity-75 font-mono">{{ tag.id }}</span>
              </button>
            </div>
          </div>

          <!-- Área de Notas Táticas -->
          <div class="flex flex-col gap-1">
            <label class="text-[10px] uppercase text-content-muted font-bold">Notas de Fechamento / Aprendizados</label>
            <textarea 
              v-model="notes" 
              rows="3" 
              placeholder="Ex: O projeto Compass tomou 80% da tarde. Ajustar estimativa de sub-tarefas de banco..."
              class="w-full bg-app border border-borderbase rounded p-2.5 text-xs font-sans text-content focus:border-borderfocus outline-none resize-none placeholder:text-content-muted/50"
            ></textarea>
          </div>
        </div>

        <!-- Rodapé de Ação e Atalhos -->
        <div class="flex items-center justify-between px-6 py-4 border-t border-borderbase bg-surface-hover">
          <div class="text-[11px] text-content-muted flex items-center gap-3">
            <span><kbd class="px-1.5 py-0.5 bg-app border border-borderbase rounded font-mono text-[10px]">Q</kbd> Alternar Modo</span>
            <span><kbd class="px-1.5 py-0.5 bg-app border border-borderbase rounded font-mono text-[10px]">Ctrl+Enter</kbd> Confirmar</span>
          </div>

          <div class="flex items-center gap-2">
            <button @click="emit('close')" type="button" class="px-4 py-2 text-xs font-bold rounded bg-transparent hover:bg-surface-active text-content transition-colors cursor-pointer">
              Cancelar
            </button>
            <button 
              @click="handleSubmit" 
              type="button"
              :disabled="dailyCycleStore.isSubmitting"
              class="px-5 py-2 text-xs font-bold rounded bg-content text-content-invert hover:opacity-90 transition-opacity flex items-center gap-2 cursor-pointer shadow-md disabled:opacity-50"
            >
              <span>{{ mode === 'morning' ? 'Iniciar Jornada' : 'Gravar Telemetria' }}</span>
              <ArrowRight class="w-3.5 h-3.5" />
            </button>
          </div>
        </div>

      </div>
    </div>
  </Teleport>
</template>

<style scoped>
.gpu-accelerated {
  will-change: transform, opacity;
}
.animate-fade-in {
  animation: fadeIn 150ms cubic-bezier(0.16, 1, 0.3, 1) forwards;
}
@keyframes fadeIn {
  from { opacity: 0; transform: scale(0.98); }
  to { opacity: 1; transform: scale(1); }
}
</style>