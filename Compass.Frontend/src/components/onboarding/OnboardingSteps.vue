<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { useOnboardingStore } from '@/stores/onboardingStore';
import { useToastStore } from '@/stores/toastStore';
import { Terminal, Cpu, Zap, Keyboard, ArrowRight, Check, X, ShieldAlert } from 'lucide-vue-next';

const emit = defineEmits<{
  (e: 'complete'): void;
  (e: 'skip'): void;
}>();

const onboardingStore = useOnboardingStore();
const toastStore = useToastStore();

const currentStep = ref(1);
const totalSteps = 4;

// Estado reativo para o desafio prático do Passo 2 (NLP)
const nlpInput = ref('');
const isNlpValid = computed(() => {
  const val = nlpInput.value;
  return val.includes('@') || val.includes('!') || val.includes('#');
});

const nextStep = () => {
  // Trava de segurança no Passo 2: Obriga o uso da gramática NLP!
  if (currentStep.value === 2 && !isNlpValid.value) {
    toastStore.showToast('Digite pelo menos um token (@tempo, !energia ou #projeto) para avançar!', 'urgent');
    return;
  }

  if (currentStep.value === 2 && isNlpValid.value) {
    onboardingStore.addSandboxItem(nlpInput.value, 'TASK');
    toastStore.showToast('Tarefa NLP processada e salva em RAM!', 'success');
  }

  if (currentStep.value < totalSteps) {
    currentStep.value++;
  } else {
    emit('complete');
  }
};

const prevStep = () => {
  if (currentStep.value > 1) currentStep.value--;
};

// Atalhos globais durante o treinamento
const handleKeyDown = (e: KeyboardEvent) => {
  if (e.key === 'Escape') {
    emit('skip');
  } else if (e.key === 'Enter' && currentStep.value !== 2) {
    nextStep();
  } else if (e.key === 'Enter' && currentStep.value === 2 && isNlpValid.value) {
    nextStep();
  }
};

onMounted(() => {
  window.addEventListener('keydown', handleKeyDown);
});

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeyDown);
});
</script>

<template>
  <div class="flex flex-col h-full bg-surface text-content">
    
    <!-- Cabeçalho de Progresso Tático -->
    <div class="p-4 border-b border-borderbase flex items-center justify-between font-mono text-xs">
      <div class="flex items-center gap-2">
        <span class="px-1.5 py-0.5 rounded bg-app border border-borderfocus font-bold">
          PASSO 0{{ currentStep }} / 0{{ totalSteps }}
        </span>
        <span class="text-content-muted uppercase">Treinamento Operacional</span>
      </div>
      
      <button 
        @click="emit('skip')"
        class="text-content-muted hover:text-content transition-colors flex items-center gap-1 cursor-pointer"
        title="Pular treinamento (ESC)"
      >
        <span>[ESC] Sair</span>
      </button>
    </div>

    <!-- Corpo do Tutorial (4 Estados) -->
    <div class="p-6 md:p-8 space-y-6 min-h-[300px] flex flex-col justify-center">
      
      <!-- ETAPA 1: FILOSOFIA & JANELA DE FOCO -->
      <div v-if="currentStep === 1" class="space-y-4 animate-fadeIn">
        <div class="w-10 h-10 rounded-lg bg-app border border-borderfocus flex items-center justify-center text-content-accent">
          <Cpu class="w-5 h-5" />
        </div>
        <h2 class="text-lg font-bold tracking-tight">1. O Motor de Decisão & Foco Líquido</h2>
        <p class="text-sm text-content-muted leading-relaxed">
          O Compass não é uma lista de tarefas comum. Ele opera sob o conceito de <strong>Janela de Foco (M<sub>tempo</sub>)</strong>: o algoritmo calcula exatamente quantos minutos úteis você tem no turno e filtra o que realmente cabe na sua energia atual.
        </p>
        <div class="p-3 rounded bg-app border border-borderbase font-mono text-xs text-status-warning flex items-center gap-2">
          <ShieldAlert class="w-4 h-4 flex-shrink-0" />
          <span>Regra de Ouro: Nunca agende mais de 70% da sua capacidade diária.</span>
        </div>
      </div>

      <!-- ETAPA 2: O QUICK CAPTURE E GRAMÁTICA NLP -->
      <div v-else-if="currentStep === 2" class="space-y-4 animate-fadeIn">
        <div class="w-10 h-10 rounded-lg bg-app border border-borderfocus flex items-center justify-center text-content-accent">
          <Terminal class="w-5 h-5" />
        </div>
        <h2 class="text-lg font-bold tracking-tight">2. Quick Capture & Gramática NLP</h2>
        <p class="text-sm text-content-muted leading-relaxed">
          Para criar compromissos em menos de 5 segundos, digite tudo em uma linha só. O parser extrai tempo, energia e projeto automaticamente.
        </p>
        
        <!-- Caixa de Exemplos NLP -->
        <div class="grid grid-cols-3 gap-2 font-mono text-[11px] text-center">
          <div class="p-2 rounded bg-app border border-borderbase"><strong class="text-content">@45m</strong> = Duração</div>
          <div class="p-2 rounded bg-app border border-borderbase"><strong class="text-content">!3</strong> = Energia (1 a 3)</div>
          <div class="p-2 rounded bg-app border border-borderbase"><strong class="text-content">#dev</strong> = Projeto</div>
        </div>

        <!-- Desafio Prático Interativo -->
        <div class="pt-2 space-y-2">
          <label class="block text-xs font-mono text-content uppercase tracking-wider">
            Desafio: Digite uma tarefa usando pelo menos 1 token (@, ! ou #):
          </label>
          <input 
            v-model="nlpInput"
            type="text" 
            placeholder="Ex: Refatorar API do banco @30m !3 #backend"
            class="w-full px-3 py-2.5 rounded bg-app border transition-all font-mono text-sm text-content focus:outline-none"
            :class="isNlpValid ? 'border-status-success-border text-status-success-text' : 'border-borderfocus focus:border-borderhighlight'"
            autofocus
          />
          <span v-if="isNlpValid" class="text-[11px] font-mono text-status-success-text flex items-center gap-1">
            <Check class="w-3.5 h-3.5 inline" /> Token detectado! Pressione Avançar ou [ENTER].
          </span>
        </div>
      </div>

      <!-- ETAPA 3: NAVEGAÇÃO ZERO-MOUSE -->
      <div v-else-if="currentStep === 3" class="space-y-4 animate-fadeIn">
        <div class="w-10 h-10 rounded-lg bg-app border border-borderfocus flex items-center justify-center text-content-accent">
          <Keyboard class="w-5 h-5" />
        </div>
        <h2 class="text-lg font-bold tracking-tight">3. Ergonomia Zero-Mouse</h2>
        <p class="text-sm text-content-muted leading-relaxed">
          O mouse é opcional. Mantenha as mãos no teclado para navegar pela sua lista com latência zero e acionar comandos táticos em qualquer tela.
        </p>

        <!-- Tabela Monocromática de Atalhos -->
        <div class="grid grid-cols-2 gap-2.5 font-mono text-xs">
          <div class="p-2.5 rounded bg-app border border-borderbase flex items-center justify-between">
            <span class="text-content-muted">Abrir Captura:</span>
            <kbd class="px-1.5 py-0.5 rounded bg-surface border border-borderfocus font-bold text-content">[C]</kbd>
          </div>
          <div class="p-2.5 rounded bg-app border border-borderbase flex items-center justify-between">
            <span class="text-content-muted">Command Bar:</span>
            <kbd class="px-1.5 py-0.5 rounded bg-surface border border-borderfocus font-bold text-content">[Cmd + K]</kbd>
          </div>
          <div class="p-2.5 rounded bg-app border border-borderbase flex items-center justify-between">
            <span class="text-content-muted">Concluir Item:</span>
            <kbd class="px-1.5 py-0.5 rounded bg-surface border border-borderfocus font-bold text-content">[ESPAÇO]</kbd>
          </div>
          <div class="p-2.5 rounded bg-app border border-borderbase flex items-center justify-between">
            <span class="text-content-muted">Dev Console:</span>
            <kbd class="px-1.5 py-0.5 rounded bg-surface border border-borderfocus font-bold text-content">[Cmd + Shift + D]</kbd>
          </div>
        </div>
      </div>

      <!-- ETAPA 4: PRONTIDÃO OPERACIONAL -->
      <div v-else-if="currentStep === 4" class="space-y-4 animate-fadeIn text-center py-4">
        <div class="w-12 h-12 rounded-full bg-app border border-borderhighlight flex items-center justify-center text-content mx-auto">
          <Zap class="w-6 h-6" />
        </div>
        <h2 class="text-xl font-bold tracking-tight">Sistema Pronto para Operação</h2>
        <p class="text-sm text-content-muted max-w-md mx-auto leading-relaxed">
          Você dominou os fundamentos. O ecossistema está configurado, o motor de decisão está calibrado e a memória RAM está pronta para ser liberada.
        </p>
      </div>

    </div>

    <!-- Rodapé de Controle do Stepper -->
    <div class="p-4 bg-app border-t border-borderbase flex items-center justify-between font-mono text-xs">
      <button 
        v-if="currentStep > 1"
        @click="prevStep"
        class="px-4 py-2 rounded border border-borderbase hover:border-borderfocus text-content-muted hover:text-content transition-all cursor-pointer"
      >
        [←] Anterior
      </button>
      <div v-else /> <!-- Espaçador -->

      <button 
        @click="nextStep"
        class="px-5 py-2 rounded bg-content text-content-invert font-bold tracking-wider uppercase transition-all shadow-md flex items-center gap-2 cursor-pointer hover:opacity-90"
        :class="{ 'opacity-50 cursor-not-allowed': currentStep === 2 && !isNlpValid }"
      >
        <span>{{ currentStep === totalSteps ? 'Concluir Treinamento' : 'Próximo [ENTER]' }}</span>
        <ArrowRight class="w-3.5 h-3.5 stroke-[3]" />
      </button>
    </div>

  </div>
</template>