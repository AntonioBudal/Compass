<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import { useOnboardingStore } from '@/stores/onboardingStore';
import InteractiveLabView from '@/views/InteractiveLabView.vue';
import { 
  Terminal, CheckCircle2, Clock, RefreshCw, 
  FileText, ArrowRight, Zap 
} from 'lucide-vue-next';

const emit = defineEmits<{
  (e: 'complete'): void;
  (e: 'skip'): void;
}>();

const onboardingStore = useOnboardingStore();

// Fases: 'THEORY' (Passo 1 ao 5) -> 'LAB' (Tela dividida)
const currentPhase = ref<'THEORY' | 'LAB'>('THEORY');
const currentStep = ref(1);
const totalSteps = 5;

const nextStep = () => {
  if (currentStep.value < totalSteps) {
    currentStep.value++;
  } else {
    // Ao final da teoria, entramos no laboratório!
    currentPhase.value = 'LAB';
  }
};

const prevStep = () => {
  if (currentStep.value > 1) currentStep.value--;
};

const finishTutorial = () => {
  onboardingStore.finishOnboarding();
  emit('complete');
};

const skipTutorial = () => {
  onboardingStore.skipOnboarding();
  emit('skip');
};

const handleKeyDown = (e: KeyboardEvent) => {
  if (e.key === 'Escape') skipTutorial();
  if (e.key === 'Enter' && currentPhase.value === 'THEORY') nextStep();
};

onMounted(() => window.addEventListener('keydown', handleKeyDown));
onUnmounted(() => window.removeEventListener('keydown', handleKeyDown));
</script>

<template>
  <div class="h-full w-full bg-surface text-content flex flex-col font-sans select-none overflow-hidden">
    
    <transition name="phase-transition" mode="out-in">
      
      <!-- ========================================== -->
      <!-- FASE 1: TEORIA RÁPIDA (THEORY)             -->
      <!-- ========================================== -->
      <div v-if="currentPhase === 'THEORY'" class="flex flex-col h-full w-full max-w-3xl mx-auto" key="theory">
        <!-- Cabeçalho de Progresso -->
        <div class="p-6 border-b border-borderbase flex items-center justify-between text-xs font-mono">
          <div class="flex items-center gap-3">
            <span class="px-2 py-0.5 rounded bg-content text-content-invert font-bold">
              0{{ currentStep }} / 0{{ totalSteps }}
            </span>
            <span class="text-content-muted uppercase tracking-wider font-bold">Anatomia do Sistema</span>
          </div>
          <button @click="skipTutorial" class="text-content-muted hover:text-content text-xs transition-colors cursor-pointer border border-borderbase px-2 py-1 rounded">
            [ESC] Pular Tudo
          </button>
        </div>

        <!-- Conteúdo Pedagógico Limpo -->
        <div class="flex-1 p-8 md:p-12 flex flex-col justify-center relative">
          <transition name="step-fade" mode="out-in">
            
            <!-- PASSO 1: TAREFA -->
            <div v-if="currentStep === 1" class="space-y-6" key="step1">
              <div class="w-14 h-14 rounded-xl bg-app border border-borderfocus flex items-center justify-center text-content shadow-sm">
                <CheckCircle2 class="w-7 h-7 stroke-[2.5]" />
              </div>
              <div class="space-y-3">
                <h2 class="text-2xl font-bold font-mono uppercase tracking-tight">1. Tarefa (Task) — Esforço Finito</h2>
                <p class="text-base text-content-muted leading-relaxed max-w-lg">
                  A <strong>Tarefa</strong> é a unidade fundamental de trabalho do Compass. Ela representa uma ação finita que consome <strong>Tempo Estimado</strong> e exige um nível específico de <strong>Energia Cognitiva (!1 a !3)</strong>. Assim que concluída, ela sai da sua fila para sempre.
                </p>
              </div>
            </div>

            <!-- PASSO 2: EVENTO -->
            <div v-else-if="currentStep === 2" class="space-y-6" key="step2">
              <div class="w-14 h-14 rounded-xl bg-app border border-borderfocus flex items-center justify-center text-content shadow-sm">
                <Clock class="w-7 h-7 stroke-[2.5]" />
              </div>
              <div class="space-y-3">
                <h2 class="text-2xl font-bold font-mono uppercase tracking-tight">2. Evento (Event) — Bloco Rígido</h2>
                <p class="text-base text-content-muted leading-relaxed max-w-lg">
                  O <strong>Evento</strong> é um compromisso preso ao relógio (ex: reuniões, consultas). Diferente da tarefa, ele não é ordenado pelo algoritmo: ele <strong>corta e bloqueia a sua Janela de Foco</strong>, impedindo conflitos temporais.
                </p>
              </div>
            </div>

            <!-- PASSO 3: HÁBITO -->
            <div v-else-if="currentStep === 3" class="space-y-6" key="step3">
              <div class="w-14 h-14 rounded-xl bg-app border border-borderfocus flex items-center justify-center text-content shadow-sm">
                <RefreshCw class="w-7 h-7 stroke-[2.5]" />
              </div>
              <div class="space-y-3">
                <h2 class="text-2xl font-bold font-mono uppercase tracking-tight">3. Hábito (Habit) — Consistência</h2>
                <p class="text-base text-content-muted leading-relaxed max-w-lg">
                  O <strong>Hábito</strong> é recorrente e orientado à disciplina. Concluí-lo incrementa sua sequência ativa (<strong>Streak</strong>). Ele foca em proteger sua consistência sem gerar estresse de atraso.
                </p>
              </div>
            </div>

            <!-- PASSO 4: NOTA -->
            <div v-else-if="currentStep === 4" class="space-y-6" key="step4">
              <div class="w-14 h-14 rounded-xl bg-app border border-borderfocus flex items-center justify-center text-content shadow-sm">
                <FileText class="w-7 h-7 stroke-[2.5]" />
              </div>
              <div class="space-y-3">
                <h2 class="text-2xl font-bold font-mono uppercase tracking-tight">4. Nota (Note) — Brain Dump</h2>
                <p class="text-base text-content-muted leading-relaxed max-w-lg">
                  A <strong>Nota</strong> é uma descarga mental de atrito zero. Possui duração nula (0m) e não consome sua janela de foco. Use para capturar ideias rápidas durante o dia para processar depois.
                </p>
              </div>
            </div>

            <!-- PASSO 5: TRANSIÇÃO -->
            <div v-else-if="currentStep === 5" class="space-y-6 text-center flex flex-col items-center justify-center" key="step5">
              <div class="w-16 h-16 rounded-full bg-app border-2 border-content flex items-center justify-center text-content shadow-lg">
                <Terminal class="w-8 h-8 stroke-[2]" />
              </div>
              <div class="space-y-3">
                <h2 class="text-3xl font-bold font-mono uppercase tracking-tight">Pronto para a Prática</h2>
                <p class="text-base text-content-muted leading-relaxed max-w-md mx-auto">
                  Você já sabe a teoria da arquitetura. Agora, vamos colocar as mãos no teclado e entender como o algoritmo extrai dados da sua mente em tempo real.
                </p>
              </div>
            </div>

          </transition>
        </div>

        <!-- Rodapé de Navegação da Teoria -->
        <div class="p-6 bg-app border-t border-borderbase flex items-center justify-between font-mono text-xs">
          <button 
            @click="prevStep"
            class="px-5 py-2.5 rounded-lg border border-borderbase hover:border-borderfocus text-content-muted hover:text-content transition-all cursor-pointer font-bold"
            :class="currentStep === 1 ? 'opacity-0 pointer-events-none' : ''"
          >
            [←] Voltar
          </button>

          <button 
            @click="nextStep"
            class="px-6 py-2.5 rounded-lg bg-content text-content-invert font-bold tracking-wider uppercase transition-all shadow-md flex items-center gap-2 cursor-pointer hover:opacity-90"
          >
            <span>{{ currentStep === totalSteps ? 'Entrar no Laboratório [ENTER]' : 'Avançar [ENTER]' }}</span>
            <ArrowRight class="w-4 h-4 stroke-[3]" />
          </button>
        </div>
      </div>

      <!-- ========================================== -->
      <!-- FASE 2: O LABORATÓRIO (LAB)                -->
      <!-- ========================================== -->
      <div v-else-if="currentPhase === 'LAB'" class="h-full w-full" key="lab">
        <InteractiveLabView @complete="finishTutorial" />
      </div>

    </transition>
  </div>
</template>

<style scoped>
/* Transição Macro entre Teoria e Lab */
.phase-transition-enter-active,
.phase-transition-leave-active {
  transition: all 400ms cubic-bezier(0.16, 1, 0.3, 1);
}
.phase-transition-enter-from { opacity: 0; transform: scale(0.98); }
.phase-transition-leave-to { opacity: 0; transform: scale(1.02); }

/* Transição Micro entre os 5 Passos Teóricos */
.step-fade-enter-active,
.step-fade-leave-active {
  transition: all 200ms ease;
}
.step-fade-enter-from { opacity: 0; transform: translateX(10px); }
.step-fade-leave-to { opacity: 0; transform: translateX(-10px); }
</style>