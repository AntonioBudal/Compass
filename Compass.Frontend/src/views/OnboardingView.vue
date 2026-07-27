<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { useOnboardingStore } from '@/stores/onboardingStore';
import { useToastStore } from '@/stores/toastStore';
import { 
  Terminal, CheckCircle2, Clock, RefreshCw, 
  FileText, ArrowRight, Check, ShieldAlert, Zap, Flame 
} from 'lucide-vue-next';

const emit = defineEmits<{
  (e: 'complete'): void;
  (e: 'skip'): void;
}>();

const onboardingStore = useOnboardingStore();
const toastStore = useToastStore();

const currentStep = ref(1);
const totalSteps = 5;

// Estado interativo de teste para cada tipo
const createdTypes = ref<Record<string, boolean>>({
  TASK: false,
  EVENT: false,
  HABIT: false,
  NOTE: false
});

// Ações práticas no Sandbox (Ensinando fazendo)
const simulateTask = () => {
  onboardingStore.addSandboxItem('Refatorar módulo de autenticação (@45m !3)', 'TASK');
  createdTypes.value.TASK = true;
};

const simulateEvent = () => {
  onboardingStore.addSandboxItem('Alinhamento de Arquitetura (14:00 - 15:00)', 'EVENT');
  createdTypes.value.EVENT = true;
};

const simulateHabit = () => {
  onboardingStore.addSandboxItem('Beber 500ml de água e alongar (+1 Streak 🔥)', 'HABIT');
  createdTypes.value.HABIT = true;
};

const simulateNote = () => {
  onboardingStore.addSandboxItem('Ideia: Pesquisar sobre CRDTs para sync offline', 'NOTE');
  createdTypes.value.NOTE = true;
};

const nextStep = () => {
  if (currentStep.value < totalSteps) {
    currentStep.value++;
  } else {
    emit('complete');
  }
};

const prevStep = () => {
  if (currentStep.value > 1) currentStep.value--;
};

// Suporte a teclado Zero-Mouse
const handleKeyDown = (e: KeyboardEvent) => {
  if (e.key === 'Escape') emit('skip');
  if (e.key === 'Enter') nextStep();
};

onMounted(() => window.addEventListener('keydown', handleKeyDown));
onUnmounted(() => window.removeEventListener('keydown', handleKeyDown));
</script>

<template>
  <div class="flex flex-col h-full bg-surface text-content font-mono select-none">
    
    <!-- Cabeçalho de Progresso -->
    <div class="p-4 border-b border-borderbase flex items-center justify-between text-xs">
      <div class="flex items-center gap-2">
        <span class="px-2 py-0.5 rounded bg-content text-content-invert font-bold">
          0{{ currentStep }} / 0{{ totalSteps }}
        </span>
        <span class="text-content-muted uppercase tracking-wider">Anatomia dos Compromissos</span>
      </div>
      <button @click="emit('skip')" class="text-content-muted hover:text-content text-xs transition-colors cursor-pointer">
        [ESC] Pular Tutorial
      </button>
    </div>

    <!-- Conteúdo Pedagógico (5 Etapas) -->
    <div class="p-6 md:p-8 space-y-6 min-h-[340px] flex flex-col justify-center font-sans">
      
      <!-- PASSO 1: TAREFA (TASK) -->
      <div v-if="currentStep === 1" class="space-y-4 animate-fadeIn">
        <div class="flex items-center gap-3 text-content">
          <div class="p-2 rounded bg-app border border-borderfocus text-content">
            <CheckCircle2 class="w-6 h-6 stroke-[2.5]" />
          </div>
          <h2 class="text-xl font-bold font-mono uppercase tracking-tight">1. Tarefa (Task) — Esforço Finito</h2>
        </div>
        
        <p class="text-sm text-content-muted leading-relaxed font-sans">
          A <strong>Tarefa</strong> é a unidade fundamental de trabalho do Compass. Ela representa uma ação finita que consome <strong>Tempo Estimado (minutos)</strong> e exige um nível específico de <strong>Energia Cognitiva (!1 a !3)</strong>. Assim que concluída, ela sai da sua fila de execução.
        </p>

        <div class="p-4 rounded-lg bg-app border border-borderbase space-y-3">
          <div class="text-xs font-mono uppercase text-content-muted flex items-center justify-between">
            <span>Teste Prático no Sandbox:</span>
            <span v-if="createdTypes.TASK" class="text-status-success-text font-bold flex items-center gap-1">
              <Check class="w-3.5 h-3.5" /> Gerado em RAM
            </span>
          </div>
          <button 
            @click="simulateTask"
            class="w-full py-2 px-4 rounded bg-surface hover:bg-surface-hover border border-borderfocus font-mono text-xs font-bold transition-all flex items-center justify-center gap-2 cursor-pointer shadow-sm"
          >
            <span>+ Simular Criação de Tarefa (@45m !3)</span>
          </button>
        </div>
      </div>

      <!-- PASSO 2: EVENTO (EVENT) -->
      <div v-else-if="currentStep === 2" class="space-y-4 animate-fadeIn">
        <div class="flex items-center gap-3 text-content">
          <div class="p-2 rounded bg-app border border-borderfocus text-content">
            <Clock class="w-6 h-6 stroke-[2.5]" />
          </div>
          <h2 class="text-xl font-bold font-mono uppercase tracking-tight">2. Evento (Event) — Bloco Rígido</h2>
        </div>
        
        <p class="text-sm text-content-muted leading-relaxed font-sans">
          O <strong>Evento</strong> é um compromisso preso ao relógio (reuniões, consultas, aulas). Diferente da tarefa, ele não é movido livremente pelo algoritmo: ele <strong>corta e bloqueia a sua Janela de Foco</strong>, impedindo que o sistema agende tarefas profundas no mesmo horário.
        </p>

        <div class="p-4 rounded-lg bg-app border border-borderbase space-y-3">
          <div class="text-xs font-mono uppercase text-content-muted flex items-center justify-between">
            <span>Teste Prático no Sandbox:</span>
            <span v-if="createdTypes.EVENT" class="text-status-success-text font-bold flex items-center gap-1">
              <Check class="w-3.5 h-3.5" /> Gerado em RAM
            </span>
          </div>
          <button 
            @click="simulateEvent"
            class="w-full py-2 px-4 rounded bg-surface hover:bg-surface-hover border border-borderfocus font-mono text-xs font-bold transition-all flex items-center justify-center gap-2 cursor-pointer shadow-sm"
          >
            <span>+ Simular Evento de Agenda (14:00 - 15:00)</span>
          </button>
        </div>
      </div>

      <!-- PASSO 3: HÁBITO (HABIT) -->
      <div v-else-if="currentStep === 3" class="space-y-4 animate-fadeIn">
        <div class="flex items-center gap-3 text-content">
          <div class="p-2 rounded bg-app border border-borderfocus text-content">
            <RefreshCw class="w-6 h-6 stroke-[2.5]" />
          </div>
          <h2 class="text-xl font-bold font-mono uppercase tracking-tight">3. Hábito (Habit) — Consistência</h2>
        </div>
        
        <p class="text-sm text-content-muted leading-relaxed font-sans">
          O <strong>Hábito</strong> é uma ação recorrente orientada à disciplina, não a prazos de entrega (ex: leitura, exercícios, revisão diária). Concluir um hábito incrementa sua sequência ativa (<strong>Streak 🔥</strong>), protegendo sua consistência diária sem gerar estresse de atraso.
        </p>

        <div class="p-4 rounded-lg bg-app border border-borderbase space-y-3">
          <div class="text-xs font-mono uppercase text-content-muted flex items-center justify-between">
            <span>Teste Prático no Sandbox:</span>
            <span v-if="createdTypes.HABIT" class="text-status-success-text font-bold flex items-center gap-1">
              <Check class="w-3.5 h-3.5" /> Gerado em RAM
            </span>
          </div>
          <button 
            @click="simulateHabit"
            class="w-full py-2 px-4 rounded bg-surface hover:bg-surface-hover border border-borderfocus font-mono text-xs font-bold transition-all flex items-center justify-center gap-2 cursor-pointer shadow-sm"
          >
            <span>+ Simular Hábito Diário (+1 Streak)</span>
          </button>
        </div>
      </div>

      <!-- PASSO 4: NOTA (NOTE) -->
      <div v-else-if="currentStep === 4" class="space-y-4 animate-fadeIn">
        <div class="flex items-center gap-3 text-content">
          <div class="p-2 rounded bg-app border border-borderfocus text-content">
            <FileText class="w-6 h-6 stroke-[2.5]" />
          </div>
          <h2 class="text-xl font-bold font-mono uppercase tracking-tight">4. Nota (Note) — Captura Rápida</h2>
        </div>
        
        <p class="text-sm text-content-muted leading-relaxed font-sans">
          A <strong>Nota</strong> é uma descarga mental (Brain Dump) de atrito zero. Ela possui <strong>duração zero (0m)</strong> e não consome sua janela de foco. Use notas para capturar ideias, links, insights ou referências durante o dia para processar e triar com calma no encerramento.
        </p>

        <div class="p-4 rounded-lg bg-app border border-borderbase space-y-3">
          <div class="text-xs font-mono uppercase text-content-muted flex items-center justify-between">
            <span>Teste Prático no Sandbox:</span>
            <span v-if="createdTypes.NOTE" class="text-status-success-text font-bold flex items-center gap-1">
              <Check class="w-3.5 h-3.5" /> Gerado em RAM
            </span>
          </div>
          <button 
            @click="simulateNote"
            class="w-full py-2 px-4 rounded bg-surface hover:bg-surface-hover border border-borderfocus font-mono text-xs font-bold transition-all flex items-center justify-center gap-2 cursor-pointer shadow-sm"
          >
            <span>+ Simular Captura de Nota (0m)</span>
          </button>
        </div>
      </div>

      <!-- PASSO 5: SÍNTESE E LIBERAÇÃO -->
      <div v-else-if="currentStep === 5" class="space-y-4 animate-fadeIn text-center py-2 font-sans">
        <div class="w-12 h-12 rounded-full bg-app border border-borderhighlight flex items-center justify-center text-content mx-auto">
          <Zap class="w-6 h-6" />
        </div>
        <h2 class="text-xl font-bold font-mono uppercase tracking-tight">Anatomia Dominada com Sucesso</h2>
        <p class="text-sm text-content-muted max-w-md mx-auto leading-relaxed">
          Você testou os quatro pilares do ecossistema. O motor de decisão agora sabe como ponderar seus esforços finitos, blocos de agenda, rituais de disciplina e descargas mentais.
        </p>
        <div class="p-3 rounded bg-app border border-borderbase font-mono text-xs text-status-warning inline-block">
          ⚡ Dica: Use o atalho [C] a qualquer momento para abrir a captura rápida.
        </div>
      </div>

    </div>

    <!-- Rodapé de Navegação do Tutorial -->
    <div class="p-4 bg-app border-t border-borderbase flex items-center justify-between text-xs font-mono">
      <button 
        v-if="currentStep > 1"
        @click="prevStep"
        class="px-4 py-2 rounded border border-borderbase hover:border-borderfocus text-content-muted hover:text-content transition-all cursor-pointer"
      >
        [←] Anterior
      </button>
      <div v-else />

      <button 
        @click="nextStep"
        class="px-5 py-2 rounded bg-content text-content-invert font-bold tracking-wider uppercase transition-all shadow-md flex items-center gap-2 cursor-pointer hover:opacity-90"
      >
        <span>{{ currentStep === totalSteps ? 'Concluir Tutorial [ENTER]' : 'Próximo [ENTER]' }}</span>
        <ArrowRight class="w-3.5 h-3.5 stroke-[3]" />
      </button>
    </div>

  </div>
</template>