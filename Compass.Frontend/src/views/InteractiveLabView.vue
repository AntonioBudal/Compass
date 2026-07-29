<script setup lang="ts">
import { ref, computed } from 'vue';
import { 
  Terminal, CheckCircle2, ArrowRight, Sparkles, ShieldAlert, 
  RefreshCw, FileText, Folder, Target, Cpu, Activity, BrainCircuit,
  Battery, Clock, Trash2, Flame, Copy
} from 'lucide-vue-next';
import GlassBoxPipeline from '@/components/onboarding/GlassBoxPipeline.vue';
import { parseQuickCapture } from '@/utils/nlpParser';
import { useToastStore } from '@/stores/toastStore';

const emit = defineEmits<{ (e: 'complete'): void }>();
const toastStore = useToastStore();

const liveInput = ref('');
const currentChallenge = ref(1);
const totalChallenges = 10;

const goToNext = () => {
  liveInput.value = '';
  if (currentChallenge.value < totalChallenges) {
    currentChallenge.value++;
  } else {
    emit('complete');
  }
};

// ==========================================
// MÁQUINAS DE ESTADO DOS DESAFIOS (1 a 5)
// ==========================================
const isChallenge1Completed = computed(() => {
  const p = parseQuickCapture(liveInput.value);
  return p.type === 'TASK' && p.estimatedDurationMinutes === 45 && p.energyRequired === 3;
});

const isChallenge2Completed = computed(() => {
  const p = parseQuickCapture(liveInput.value);
  return p.type === 'EVENT' && /\d{2}:\d{2}\s*-\s*\d{2}:\d{2}/.test(liveInput.value);
});

const isChallenge3Completed = computed(() => parseQuickCapture(liveInput.value).type === 'HABIT');
const isChallenge4Completed = computed(() => parseQuickCapture(liveInput.value).type === 'NOTE');
const isChallenge5Completed = computed(() => {
  const p = parseQuickCapture(liveInput.value);
  return p.type === 'TASK' && !!p.projectQuery;
});

// Botões interativos (Fase 3 - Hábito)
const simulateDuplicateHabit = () => {
  toastStore.showIntervention({
    code: 'HABIT_ALREADY_COMPLETED',
    title: 'Hábito já registrado hoje!',
    explanation: 'Sua sequência (Streak) já está garantida. O Compass protege a disciplina impedindo over-tracking no mesmo dia.',
    severity: 'info',
    actions: [{ label: 'Entendi', isPrimary: true, handler: () => {} }]
  });
};

// ==========================================
// SIMULADOR: FASE 7 (NOW ENGINE REORDER)
// ==========================================
const simEnergy = ref(3);
const simTime = ref(120);

const simulatedTasks = computed(() => {
  const tasks = [
    { id: 1, title: 'Planejamento Estratégico (DEEP)', energy: 3, time: 60, base: 50 },
    { id: 2, title: 'Responder Emails (MAINT)', energy: 1, time: 15, base: 30 },
    { id: 3, title: 'Revisão de Pull Requests (OPER)', energy: 2, time: 45, base: 40 }
  ];
  return tasks.map(t => {
    let penalty = 0;
    if (t.energy > simEnergy.value) penalty -= 100;
    if (t.time > simTime.value) penalty -= 100;
    return { ...t, score: t.base + (t.energy === simEnergy.value ? 20 : 0) + penalty };
  }).sort((a, b) => b.score - a.score);
});

// ==========================================
// SIMULADOR: FASE 8 (EAI MACHINE LEARNING)
// ==========================================
const eaiValue = ref(1.0);
const isEaiCalculated = ref(false);

const calculateEai = () => {
  isEaiCalculated.value = true;
  let curr = 1.0;
  const interval = setInterval(() => {
    curr += 0.05;
    eaiValue.value = Number(curr.toFixed(2));
    if (curr >= 1.5) clearInterval(interval);
  }, 50);
};

// ==========================================
// SIMULADOR: FASE 9 (RECUPERAÇÃO DE ERROS)
// ==========================================
const fireError = (type: string) => {
  const errors: any = {
    shift: { code: 'OUTSIDE_SHIFT', title: 'Fora do Turno', exp: 'Tentativa de criar tarefa às 23h. Movido para o horizonte de Amanhã.' },
    overlap: { code: 'EVENT_OVERLAP', title: 'Conflito de Agenda', exp: 'Já existe uma Reunião de Marketing neste horário (14:00-15:00).' },
    project: { code: 'MISSING_PROJECT', title: 'Projeto Inexistente', exp: 'A tag #web3 não existe no banco. Criar novo projeto ou usar avulsa?' }
  };
  const err = errors[type];
  toastStore.showIntervention({
    code: err.code, title: err.title, explanation: err.exp, severity: 'warning',
    actions: [{ label: 'Corrigir Automaticamente', isPrimary: true, handler: () => {} }]
  });
};

// ==========================================
// SIMULADOR: FASE 10 (MISSÃO FINAL BOSS)
// ==========================================
const bossItems = ref<any[]>([]);
const bossProgress = computed(() => ({
  task: bossItems.value.some(i => i.type === 'TASK'),
  event: bossItems.value.some(i => i.type === 'EVENT'),
  habit: bossItems.value.some(i => i.type === 'HABIT'),
  note: bossItems.value.some(i => i.type === 'NOTE')
}));
const isBossDefeated = computed(() => Object.values(bossProgress.value).every(v => v));

const captureBossItem = () => {
  if (!liveInput.value.trim()) return;
  bossItems.value.unshift(parseQuickCapture(liveInput.value));
  liveInput.value = '';
};
</script>

<template>
  <div class="h-full w-full bg-app flex overflow-hidden font-sans select-none">
    
    <!-- ============================================================== -->
    <!-- COLUNA ESQUERDA: O PROFESSOR & INSTRUÇÕES DA TRILHA          -->
    <!-- ============================================================== -->
    <div class="flex-1 max-w-lg lg:max-w-xl xl:max-w-2xl flex flex-col p-8 md:p-12 space-y-8 overflow-y-auto relative border-r border-borderbase bg-surface">
      
      <!-- Cabeçalho Mestre -->
      <div class="space-y-2 pb-6 border-b border-borderbase">
        <div class="flex items-center justify-between">
          <span class="px-2 py-1 rounded bg-content text-content-invert text-[10px] font-mono font-bold tracking-widest uppercase shadow-sm">
            Trilha de Descoberta ({{ currentChallenge }}/10)
          </span>
          <button v-if="currentChallenge > 1 && currentChallenge < 10" @click="currentChallenge--" class="text-xs font-mono text-content-muted hover:text-content transition-colors cursor-pointer">
            ← Voltar Fase
          </button>
        </div>
        <h1 class="text-3xl font-bold tracking-tight text-content mt-2">Laboratório Vivo.</h1>
      </div>

      <transition name="fade-slide" mode="out-in">
        
        <!-- FASE 1: TAREFA -->
        <div v-if="currentChallenge === 1" key="c1" class="space-y-6">
          <h2 class="text-xl font-bold flex items-center gap-2"><CheckCircle2 class="w-5 h-5 text-content-accent"/> 1. Tarefa (Score & Energia)</h2>
          <p class="text-sm text-content-muted leading-relaxed">
            Veja no painel à direita como a duração e a energia de uma tarefa influenciam o Score matemático que o Now Engine usará para priorizá-la.
          </p>

          <!-- BOX DE INSTRUÇÃO DESTACADO -->
          <div v-if="!isChallenge1Completed" class="my-4 p-5 rounded-xl bg-app border-l-4 border-content-accent shadow-sm flex flex-col gap-2">
            <span class="text-[10px] font-mono font-bold uppercase text-content-muted tracking-widest flex items-center gap-1.5">
              <Terminal class="w-3.5 h-3.5" /> Digite exatamente assim:
            </span>
            <code class="text-lg md:text-xl font-mono font-bold text-content select-all">Estudar Inglês @45m !3 #idiomas</code>
          </div>
          
          <div v-else class="my-4 text-sm bg-status-success-bg/20 p-4 border border-status-success-border rounded-xl text-status-success-text flex items-center gap-3">
            <CheckCircle2 class="w-6 h-6 flex-shrink-0" />
            <span><strong>Perfeito!</strong> O Raio-X à direita calculou a pontuação exata dessa tarefa.</span>
          </div>

          <!-- INPUT -->
          <input v-model="liveInput" type="text" placeholder="Digite aqui..." class="w-full p-4 rounded-xl bg-app border-2 border-borderfocus text-content focus:outline-none focus:border-content transition-colors font-mono" :class="isChallenge1Completed ? 'border-status-success-border text-status-success-text' : ''" :disabled="isChallenge1Completed"/>
          
          <button v-if="isChallenge1Completed" @click="goToNext" class="w-full p-3.5 rounded-tactic bg-content text-content-invert font-bold uppercase flex justify-center items-center gap-2 hover:opacity-90 shadow-md transition-all animate-fadeIn">
            Próxima Descoberta <ArrowRight class="w-4 h-4"/>
          </button>
        </div>

        <!-- FASE 2: EVENTO -->
        <div v-else-if="currentChallenge === 2" key="c2" class="space-y-6">
          <h2 class="text-xl font-bold flex items-center gap-2"><Clock class="w-5 h-5 text-status-warning"/> 2. Evento (Hard Blocker)</h2>
          <p class="text-sm text-content-muted leading-relaxed">
            Eventos não entram na fila comum. Eles bloqueiam a linha do tempo rigidamente. O Escudo de Validação bloqueará a criação se o horário estiver incompleto.
          </p>
          
          <!-- BOX DE INSTRUÇÃO DESTACADO (2 PASSOS) -->
          <div v-if="!isChallenge2Completed" class="my-4 p-5 rounded-xl bg-app border-l-4 border-status-warning shadow-sm flex flex-col gap-3">
            <div>
              <span class="text-[10px] font-mono font-bold uppercase text-status-warning tracking-widest flex items-center gap-1.5 mb-1">
                <ShieldAlert class="w-3.5 h-3.5" /> 1º Provoque o erro:
              </span>
              <code class="text-base font-mono font-bold text-content-muted line-through select-all">Reunião /e 14h</code>
            </div>
            <div class="pt-2 border-t border-borderbase">
              <span class="text-[10px] font-mono font-bold uppercase text-content-muted tracking-widest flex items-center gap-1.5 mb-1">
                <CheckCircle2 class="w-3.5 h-3.5" /> 2º Agora, conserte:
              </span>
              <code class="text-lg md:text-xl font-mono font-bold text-content select-all">Reunião /e 14:00-15:00</code>
            </div>
          </div>

          <div v-else class="my-4 text-sm bg-status-success-bg/20 p-4 border border-status-success-border rounded-xl text-status-success-text flex items-center gap-3">
            <CheckCircle2 class="w-6 h-6 flex-shrink-0" />
            <span><strong>Proteção Desativada!</strong> O motor reconheceu o bloco de horário exato.</span>
          </div>

          <input v-model="liveInput" type="text" placeholder="Digite aqui..." class="w-full p-4 rounded-xl bg-app border-2 border-borderfocus text-content focus:outline-none focus:border-content transition-colors font-mono" :class="isChallenge2Completed ? 'border-status-success-border text-status-success-text' : ''" :disabled="isChallenge2Completed"/>
          
          <button v-if="isChallenge2Completed" @click="goToNext" class="w-full p-3.5 rounded-tactic bg-content text-content-invert font-bold uppercase flex justify-center items-center gap-2 hover:opacity-90 shadow-md transition-all animate-fadeIn">
            Próxima Descoberta <ArrowRight class="w-4 h-4"/>
          </button>
        </div>

        <!-- FASE 3: HÁBITO -->
        <div v-else-if="currentChallenge === 3" key="c3" class="space-y-6">
          <h2 class="text-xl font-bold flex items-center gap-2"><RefreshCw class="w-5 h-5 text-status-success-text"/> 3. Hábito (Consistência)</h2>
          <p class="text-sm text-content-muted leading-relaxed">
            Hábitos representam disciplina diária e não gastam energia da sua Janela de Foco.
          </p>

          <!-- BOX DE INSTRUÇÃO DESTACADO -->
          <div v-if="!isChallenge3Completed" class="my-4 p-5 rounded-xl bg-app border-l-4 border-status-success-border shadow-sm flex flex-col gap-2">
            <span class="text-[10px] font-mono font-bold uppercase text-content-muted tracking-widest flex items-center gap-1.5">
              <Terminal class="w-3.5 h-3.5" /> Digite exatamente assim:
            </span>
            <code class="text-lg md:text-xl font-mono font-bold text-content select-all">Ler 10 páginas /h</code>
          </div>

          <div v-else class="my-4 text-sm bg-status-success-bg/20 p-4 border border-status-success-border rounded-xl text-status-success-text flex flex-col gap-3">
            <div class="flex items-center gap-3">
              <CheckCircle2 class="w-6 h-6 flex-shrink-0" />
              <span><strong>Hábito Injetado!</strong> Agora, clique no botão abaixo para tentar concluí-lo duas vezes no mesmo dia.</span>
            </div>
            <button @click="simulateDuplicateHabit" class="py-2.5 rounded bg-surface border border-status-success-border text-status-success-text font-bold shadow-sm hover:bg-status-success-bg/30 transition-colors">
              Simular Duplo Clique (Errar)
            </button>
          </div>

          <input v-model="liveInput" type="text" placeholder="Digite aqui..." class="w-full p-4 rounded-xl bg-app border-2 border-borderfocus text-content focus:outline-none focus:border-content transition-colors font-mono" :class="isChallenge3Completed ? 'border-status-success-border text-status-success-text' : ''" :disabled="isChallenge3Completed"/>
          
          <button v-if="isChallenge3Completed" @click="goToNext" class="w-full p-3.5 mt-2 rounded-tactic bg-content text-content-invert font-bold uppercase flex justify-center items-center gap-2 hover:opacity-90 shadow-md transition-all animate-fadeIn">
            Próxima Descoberta <ArrowRight class="w-4 h-4"/>
          </button>
        </div>

        <!-- FASE 4: NOTA -->
        <div v-else-if="currentChallenge === 4" key="c4" class="space-y-6">
          <h2 class="text-xl font-bold flex items-center gap-2"><FileText class="w-5 h-5 text-content-muted"/> 4. Nota (Brain Dump)</h2>
          <p class="text-sm text-content-muted leading-relaxed">A Nota (<kbd>/n</kbd>) esvazia a mente sem gastar Janela de Foco. Use-a quando tiver uma ideia no meio do dia.</p>
          
          <!-- BOX DE INSTRUÇÃO DESTACADO -->
          <div v-if="!isChallenge4Completed" class="my-4 p-5 rounded-xl bg-app border-l-4 border-borderhighlight shadow-sm flex flex-col gap-2">
            <span class="text-[10px] font-mono font-bold uppercase text-content-muted tracking-widest flex items-center gap-1.5">
              <Terminal class="w-3.5 h-3.5" /> Digite exatamente assim:
            </span>
            <code class="text-lg md:text-xl font-mono font-bold text-content select-all">Pesquisar sobre Vue 3 /n</code>
          </div>

          <div v-else class="my-4 text-sm bg-status-success-bg/20 p-4 border border-status-success-border rounded-xl text-status-success-text flex items-center gap-3">
            <CheckCircle2 class="w-6 h-6 flex-shrink-0" />
            <span><strong>Idéia Capturada!</strong> Você formou sua base neural de referência.</span>
          </div>

          <input v-model="liveInput" type="text" placeholder="Digite aqui..." class="w-full p-4 rounded-xl bg-app border-2 border-borderfocus text-content focus:outline-none focus:border-content transition-colors font-mono" :class="isChallenge4Completed ? 'border-status-success-border text-status-success-text' : ''" :disabled="isChallenge4Completed"/>
          
          <button v-if="isChallenge4Completed" @click="goToNext" class="w-full p-3.5 rounded-tactic bg-content text-content-invert font-bold uppercase flex justify-center items-center gap-2 hover:opacity-90 shadow-md transition-all animate-fadeIn">
            Próxima Descoberta <ArrowRight class="w-4 h-4"/>
          </button>
        </div>

        <!-- FASE 5: PROJETO -->
        <div v-else-if="currentChallenge === 5" key="c5" class="space-y-6">
          <h2 class="text-xl font-bold flex items-center gap-2"><Folder class="w-5 h-5 text-content-accent"/> 5. Projeto (Contexto)</h2>
          <p class="text-sm text-content-muted leading-relaxed">
            Veja no painel à direita como o algoritmo recompensa atividades contextualizadas com pontuação bônus quando atreladas a um projeto.
          </p>
          
          <!-- BOX DE INSTRUÇÃO DESTACADO -->
          <div v-if="!isChallenge5Completed" class="my-4 p-5 rounded-xl bg-app border-l-4 border-content-accent shadow-sm flex flex-col gap-2">
            <span class="text-[10px] font-mono font-bold uppercase text-content-muted tracking-widest flex items-center gap-1.5">
              <Terminal class="w-3.5 h-3.5" /> Digite exatamente assim:
            </span>
            <code class="text-lg md:text-xl font-mono font-bold text-content select-all">Ajustar CSS da home #website</code>
          </div>
          
          <div v-else class="my-4 text-sm bg-status-success-bg/20 p-4 border border-status-success-border rounded-xl text-status-success-text flex items-center gap-3">
            <CheckCircle2 class="w-6 h-6 flex-shrink-0" />
            <span><strong>Contextualizado!</strong> O Now Engine dará preferência para essa tarefa.</span>
          </div>

          <input v-model="liveInput" type="text" placeholder="Digite aqui..." class="w-full p-4 rounded-xl bg-app border-2 border-borderfocus text-content focus:outline-none focus:border-content transition-colors font-mono" :class="isChallenge5Completed ? 'border-status-success-border text-status-success-text' : ''" :disabled="isChallenge5Completed"/>
          
          <button v-if="isChallenge5Completed" @click="goToNext" class="w-full p-3.5 rounded-tactic bg-content text-content-invert font-bold uppercase flex justify-center items-center gap-2 hover:opacity-90 shadow-md transition-all animate-fadeIn">
            Ir para Diagrama de Metas <ArrowRight class="w-4 h-4"/>
          </button>
        </div>

        <!-- FASE 6: META (Leitura) -->
        <div v-else-if="currentChallenge === 6" key="c6" class="space-y-6">
          <h2 class="text-xl font-bold flex items-center gap-2"><Target class="w-5 h-5 text-status-warning"/> 6. Metas (Visão de Longo Prazo)</h2>
          <p class="text-sm text-content-muted leading-relaxed">As <strong>Metas (Goals)</strong> não são executadas diretamente. Elas são a bússola superior. Veja no painel direito como a energia da sua tarefa diária flui até atingir um objetivo de longo prazo.</p>
          <button @click="goToNext" class="w-full p-3.5 rounded-tactic bg-content text-content-invert font-bold uppercase flex justify-center items-center gap-2 hover:opacity-90 shadow-md transition-all">Ir para Simulador do Algoritmo <ArrowRight class="w-4 h-4"/></button>
        </div>

        <!-- FASE 7: NOW ENGINE -->
        <div v-else-if="currentChallenge === 7" key="c7" class="space-y-6">
          <h2 class="text-xl font-bold flex items-center gap-2"><Cpu class="w-5 h-5 text-content-accent"/> 7. Now Engine (Reordenamento Real)</h2>
          <p class="text-sm text-content-muted leading-relaxed">Mova os controles abaixo e observe a inteligência artificial reorganizando suas prioridades à direita com base no que "cabe" na sua energia de agora.</p>
          <div class="space-y-4 bg-app p-6 rounded-xl border border-borderfocus shadow-inner">
            <div class="space-y-2">
              <label class="text-xs font-mono uppercase text-content-muted flex justify-between">Minha Energia Atual: <span class="font-bold text-content">Nível {{simEnergy}}</span></label>
              <input type="range" v-model.number="simEnergy" min="1" max="3" class="w-full accent-content cursor-pointer" />
            </div>
            <div class="space-y-2">
              <label class="text-xs font-mono uppercase text-content-muted flex justify-between mt-4">Tempo Livre na Janela: <span class="font-bold text-content">{{simTime}}m</span></label>
              <input type="range" v-model.number="simTime" min="15" max="120" step="15" class="w-full accent-content cursor-pointer" />
            </div>
          </div>
          <button @click="goToNext" class="w-full p-3.5 rounded-tactic bg-content text-content-invert font-bold uppercase flex justify-center items-center gap-2 hover:opacity-90 shadow-md transition-all">Ir para Machine Learning (EAI) <ArrowRight class="w-4 h-4"/></button>
        </div>

        <!-- FASE 8: EAI (Machine Learning) -->
        <div v-else-if="currentChallenge === 8" key="c8" class="space-y-6">
          <h2 class="text-xl font-bold flex items-center gap-2"><BrainCircuit class="w-5 h-5 text-status-success-text"/> 8. EAI (Estimation Accuracy Index)</h2>
          <p class="text-sm text-content-muted leading-relaxed">O Compass aprende se você é otimista ou pessimista com tempo. Simule que uma tarefa planejada para 30m acabou levando 45m na vida real.</p>
          
          <button @click="calculateEai" class="w-full p-4 rounded-xl bg-app border-2 border-borderfocus text-content font-mono font-bold hover:bg-surface-hover transition-colors shadow-sm">
            Simular Atraso (Gastou 45m)
          </button>
          
          <button v-if="isEaiCalculated" @click="goToNext" class="w-full p-3.5 rounded-tactic bg-content text-content-invert font-bold uppercase flex justify-center items-center gap-2 hover:opacity-90 shadow-md transition-all mt-4 animate-fadeIn">
            Explorar UX Defensiva <ArrowRight class="w-4 h-4"/>
          </button>
        </div>

        <!-- FASE 9: ERROS (UX Defensiva) -->
        <div v-else-if="currentChallenge === 9" key="c9" class="space-y-6">
          <h2 class="text-xl font-bold flex items-center gap-2"><ShieldAlert class="w-5 h-5 text-status-danger-text"/> 9. Recuperação de Erros</h2>
          <p class="text-sm text-content-muted leading-relaxed">O Compass não te deixa no escuro. Se você quebrar uma regra, ele bloqueia a ação, explica o motivo e oferece a solução a um clique.</p>
          <div class="space-y-3 font-mono text-xs">
            <button @click="fireError('shift')" class="w-full p-4 bg-app border border-borderbase rounded-xl hover:border-content transition-colors flex justify-between shadow-sm font-bold">Forçar Criação Fora do Turno <span>⚡</span></button>
            <button @click="fireError('overlap')" class="w-full p-4 bg-app border border-borderbase rounded-xl hover:border-content transition-colors flex justify-between shadow-sm font-bold">Forçar Evento Sobreposto <span>⚡</span></button>
            <button @click="fireError('project')" class="w-full p-4 bg-app border border-borderbase rounded-xl hover:border-content transition-colors flex justify-between shadow-sm font-bold">Forçar Projeto Inexistente <span>⚡</span></button>
          </div>
          <button @click="goToNext" class="w-full p-3.5 rounded-tactic bg-content text-content-invert font-bold uppercase flex justify-center items-center gap-2 hover:opacity-90 shadow-md transition-all mt-4">
            Iniciar Missão Final <ArrowRight class="w-4 h-4"/>
          </button>
        </div>

        <!-- FASE 10: MISSÃO FINAL BOSS -->
        <div v-else-if="currentChallenge === 10" key="c10" class="space-y-6">
          <h2 class="text-xl font-bold flex items-center gap-2 text-status-warning"><Flame class="w-5 h-5"/> 10. Missão de Certificação</h2>
          <p class="text-sm text-content-muted leading-relaxed">Prove seu domínio sobre o Compass. Usando o terminal abaixo, crie um de cada (aperte Enter após digitar cada um):</p>
          
          <!-- BOX DE INSTRUÇÃO DESTACADO (BOSS) -->
          <div v-if="!isBossDefeated" class="my-4 p-5 rounded-xl bg-app border-l-4 border-status-danger-border shadow-sm flex flex-col gap-3">
            <span class="text-[10px] font-mono font-bold uppercase text-status-danger-text tracking-widest flex items-center gap-1.5">
              <Terminal class="w-3.5 h-3.5" /> Digite esses 4 comandos:
            </span>
            <div class="flex flex-col gap-2 font-mono text-sm">
              <code class="select-all transition-colors" :class="bossProgress.task ? 'text-status-success-text line-through opacity-50' : 'text-content'">> Codificar script @60m !3</code>
              <code class="select-all transition-colors" :class="bossProgress.event ? 'text-status-success-text line-through opacity-50' : 'text-content'">> Daily /e 10:00-10:15</code>
              <code class="select-all transition-colors" :class="bossProgress.habit ? 'text-status-success-text line-through opacity-50' : 'text-content'">> Alongamento /h</code>
              <code class="select-all transition-colors" :class="bossProgress.note ? 'text-status-success-text line-through opacity-50' : 'text-content'">> Ideia de refatoração /n</code>
            </div>
          </div>

          <input v-model="liveInput" @keydown.enter="captureBossItem" type="text" placeholder="Digite e aperte Enter..." class="w-full p-4 rounded-xl bg-app border-2 border-borderfocus text-content focus:outline-none focus:border-content font-mono transition-colors" :disabled="isBossDefeated"/>

          <button v-if="isBossDefeated" @click="emit('complete')" class="w-full p-4 rounded-tactic bg-content text-content-invert font-bold tracking-wider uppercase transition-all shadow-xl hover:opacity-90 flex items-center justify-center gap-3 animate-fadeIn mt-4">
            <span>Você Dominou o Compass! (Entrar)</span><CheckCircle2 class="w-5 h-5"/>
          </button>
        </div>

      </transition>
    </div>

    <!-- ============================================================== -->
    <!-- COLUNA DIREITA: LABORATÓRIO METAMÓRFICO                        -->
    <!-- ============================================================== -->
    <div class="flex-1 hidden lg:flex bg-app flex-col relative shadow-[-10px_0_30px_rgba(0,0,0,0.03)] z-10 overflow-hidden">
      
      <!-- Se Fase 1-5 ou 10: Usa o Pipeline Raio-X original -->
      <GlassBoxPipeline v-if="currentChallenge <= 5 || currentChallenge === 10" :raw-input="liveInput" class="w-full" />

      <!-- FASE 6: DIAGRAMA DE METAS -->
      <div v-else-if="currentChallenge === 6" class="p-12 h-full flex flex-col justify-center items-center border-l border-borderbase">
        <div class="space-y-8 w-full max-w-sm font-mono text-xs">
          <div class="p-4 rounded-xl bg-surface border-2 border-status-warning-border text-center shadow-lg">
            <span class="text-content-muted">Nível Estratégico</span><br>
            <strong class="text-lg text-content">META: Lançamento Q3</strong>
          </div>
          <div class="flex justify-center"><ArrowDown class="w-6 h-6 text-content-muted" /></div>
          <div class="p-4 rounded-xl bg-surface border-2 border-content-accent text-center shadow-md">
            <span class="text-content-muted">Nível Tático</span><br>
            <strong class="text-lg text-content">PROJETO: Backend API</strong>
          </div>
          <div class="flex justify-center"><ArrowDown class="w-6 h-6 text-content-muted" /></div>
          <div class="p-4 rounded-xl bg-content border-2 border-content text-center text-content-invert shadow-sm">
            <span class="opacity-80">Nível Operacional (O que você faz hoje)</span><br>
            <strong class="text-lg">TAREFA: Refatorar JWT</strong>
          </div>
        </div>
      </div>

      <!-- FASE 7: NOW ENGINE ANIMADO -->
      <div v-else-if="currentChallenge === 7" class="p-10 h-full bg-surface border-l border-borderbase">
        <h3 class="text-lg font-bold font-mono mb-6 flex items-center gap-2"><Cpu class="w-5 h-5"/> Priorização em Tempo Real</h3>
        <transition-group name="list" tag="div" class="space-y-4">
          <div v-for="task in simulatedTasks" :key="task.id" class="p-4 rounded-xl bg-app border border-borderbase flex justify-between items-center transition-all shadow-sm" :class="task.score < 0 ? 'opacity-30 grayscale' : 'opacity-100'">
            <div>
              <strong class="text-content">{{task.title}}</strong>
              <div class="text-xs text-content-muted font-mono mt-1">Gasta: {{task.time}}m | Exige: Nível {{task.energy}}</div>
            </div>
            <div class="text-right">
              <div class="text-[10px] font-mono uppercase text-content-muted">Score Final</div>
              <div class="text-2xl font-black" :class="task.score < 0 ? 'text-status-danger-text' : 'text-status-success-text'">{{task.score}}</div>
            </div>
          </div>
        </transition-group>
      </div>

      <!-- FASE 8: EAI SIMULADOR -->
      <div v-else-if="currentChallenge === 8" class="p-12 h-full flex flex-col justify-center items-center bg-surface border-l border-borderbase">
        <div class="w-full max-w-sm space-y-6 text-center">
          <BrainCircuit class="w-16 h-16 text-content mx-auto mb-6" />
          <h3 class="text-xl font-bold font-mono">Calibração de Machine Learning</h3>
          <div class="p-6 rounded-2xl border-4 border-content-accent bg-app relative overflow-hidden">
            <div class="text-6xl font-black text-content mb-2">{{ eaiValue.toFixed(2) }}x</div>
            <div class="text-sm font-mono text-content-muted">Estimation Accuracy Index</div>
            <div class="absolute bottom-0 left-0 h-2 bg-content-accent transition-all duration-[2000ms]" :style="{ width: `${((eaiValue - 1) / 0.5) * 100}%` }"/>
          </div>
          <p class="text-sm text-content-muted leading-relaxed" v-if="eaiValue > 1.0">
            Como você atrasou, o sistema aprendeu que você é otimista com o tempo. A partir de amanhã, tarefas de 30m cobrarão silenciosamente <strong>45m</strong> da sua Janela de Foco.
          </p>
        </div>
      </div>

      <!-- FASE 9: TELA VAZIA (ESPERANDO O TOAST) -->
      <div v-else-if="currentChallenge === 9" class="p-12 h-full flex flex-col justify-center items-center opacity-50 bg-app border-l border-borderbase">
        <ShieldAlert class="w-16 h-16 text-content-muted mb-4" />
        <p class="font-mono text-center max-w-sm">Observe os modais de Intervenção aparecendo no canto da tela à medida que você aciona os erros na esquerda.</p>
      </div>

      <!-- FASE 10 (Boss): Inbox Animada -->
      <div v-else-if="currentChallenge === 10" class="p-8 h-full bg-surface border-l border-borderbase flex flex-col">
        <h3 class="text-xs font-bold font-mono uppercase tracking-wider text-content-muted mb-4 border-b border-borderbase pb-2">Banco de Dados (RAM)</h3>
        <transition-group name="list" tag="div" class="flex-1 overflow-y-auto space-y-3">
          <div v-for="item in bossItems" :key="item.title" class="p-4 bg-app border border-borderfocus rounded-xl shadow-sm font-mono text-xs flex justify-between items-center">
            <span class="truncate font-bold">{{ item.title }}</span>
            <span class="px-2.5 py-1 rounded bg-content text-content-invert">{{ item.type }}</span>
          </div>
        </transition-group>
      </div>

    </div>
  </div>
</template>

<style scoped>
.fade-slide-enter-active, .fade-slide-leave-active { transition: all 300ms cubic-bezier(0.16, 1, 0.3, 1); }
.fade-slide-enter-from { opacity: 0; transform: translateY(15px); }
.fade-slide-leave-to { opacity: 0; transform: translateY(-15px); }
.animate-fadeIn { animation: fadeIn 500ms ease-out forwards; }
@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
.list-move, .list-enter-active, .list-leave-active { transition: all 0.5s ease; }
.list-enter-from, .list-leave-to { opacity: 0; transform: translateX(30px); }
</style>