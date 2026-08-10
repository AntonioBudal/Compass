<script setup lang="ts">
import { computed, ref } from 'vue';
import { parseQuickCapture } from '@/shared/utils/nlpParser';
import CommitmentCard from '@/components/core/CommitmentCard.vue';
import { 
  Activity, ArrowDown, Code2, Cpu, Eye, EyeOff, 
  HelpCircle, CheckCircle2, XCircle, Database, ShieldAlert,
  Network, Battery, Folder, Calculator
} from 'lucide-vue-next';

const props = defineProps<{
  rawInput: string;
}>();

const isXRayMode = ref(false);

const parsed = computed(() => {
  if (!props.rawInput.trim()) return null;
  return parseQuickCapture(props.rawInput);
});

// REGRAS DE VALIDAÇÃO ESTRITA
const validation = computed(() => {
  if (!parsed.value) return null;
  const p = parsed.value;
  const checks = [];

  checks.push({ 
    passed: p.title.length > 0, 
    msg: p.title.length > 0 ? 'Sintaxe válida (título limpo)' : 'Erro: Faltou o título da ação' 
  });

  if (p.type === 'TASK') {
    checks.push({ 
      passed: !!p.projectQuery, 
      msg: p.projectQuery ? `Projeto vinculado: #${p.projectQuery}` : 'Tarefa avulsa (Perca de pontuação no Escore)' 
    });
  } else if (p.type === 'EVENT') {
    const hasTimeRange = /\d{2}:\d{2}\s*-\s*\d{2}:\d{2}/.test(props.rawInput);
    checks.push({ 
      passed: hasTimeRange, 
      msg: hasTimeRange ? 'Bloco temporal fechado (Início e Fim).' : 'ERRO CRÍTICO: Eventos exigem horário final (ex: 14:00-15:00).' 
    });
  } else if (p.type === 'HABIT') {
    checks.push({ passed: true, msg: 'Hábito detectado. Injeção de CRON padrão preparada.' });
  }

  return {
    allPassed: checks.every(c => c.passed),
    checks
  };
});

// O POR QUÊ E O SIMULADOR DE SCORING
const engineRouting = computed(() => {
  if (!parsed.value || !validation?.value) return null;
  
  if (!validation?.value.allPassed) {
    return {
      destination: 'BLOQUEADO (Rejeitado pela UX Defensiva)',
      why: 'O motor de decisão protege seu banco de dados contra informações ambíguas. Sem dados precisos, o algoritmo calcularia sua disponibilidade incorretamente.',
      impact: 'Abortando mutação. A RAM permanece intacta.',
      isError: true,
      scoreBreakdown: null
    };
  }

  const type = parsed.value.type;
  
  // Simulador visual de Scoring (Mock pedagógico)
  const taskScoreBreakdown = [
    { label: 'Urgência (Agendado p/ Hoje)', points: '+40', color: 'text-status-success-text' },
    { label: 'Energia (Match com Pico Atual)', points: '+25', color: 'text-status-success-text' },
    { label: 'Projeto (Prioridade do Contexto)', points: parsed.value.projectQuery ? '+25' : '0', color: parsed.value.projectQuery ? 'text-status-success-text' : 'text-content-muted' },
    { label: 'Fadiga (Custo da Duração)', points: '-10', color: 'text-status-danger-text' }
  ];
  const finalScore = 40 + 25 + (parsed.value.projectQuery ? 25 : 0) - 10;

  const explanations: Record<string, { destination: string, why: string, impact: string, isError: boolean, scoreBreakdown: any, finalScore: number }> = {
    TASK: {
      destination: 'Now Engine (Fila Dinâmica)',
      why: 'Tarefas são esforço finito. O algoritmo irá pontuá-la contra as outras para decidir se ela deve ser o seu "Top Focus" de agora.',
      impact: `Reduzirá sua Janela de Foco livre em ${parsed.value.estimatedDurationMinutes || 30}m.`,
      isError: false,
      scoreBreakdown: taskScoreBreakdown,
      finalScore: finalScore
    },
    EVENT: {
      destination: 'Agenda (Hard Blocker)',
      why: 'Eventos possuem horário rígido. Eles não precisam de pontuação porque obrigam o algoritmo a trabalhar ao redor deles.',
      impact: 'Recortará a linha do tempo, impedindo o agendamento de tarefas sobrepostas.',
      isError: false,
      scoreBreakdown: null,
      finalScore: 0
    },
    HABIT: {
      destination: 'Lista de Hábitos (Disciplina)',
      why: 'Hábitos não concorrem por prioridade no Now Engine. Eles servem para proteger a consistência (Streak) de rotinas diárias.',
      impact: 'Gera gatilho dopamínico imediato (+1 Streak) sem alterar a janela de foco.',
      isError: false,
      scoreBreakdown: null,
      finalScore: 0
    },
    NOTE: {
      destination: 'Brain Dump (Inbox)',
      why: 'Capturas rápidas de atrito zero. Elas saem da sua mente para não poluir sua memória de trabalho (RAM mental).',
      impact: 'Zero impacto (0m). O Motor exigirá que você as transforme em Tarefa ou Projeto depois.',
      isError: false,
      scoreBreakdown: null,
      finalScore: 0
    }
  };

  return explanations[type] || explanations['TASK'];
});

// EFEITO BORBOLETA
const baseFocus = 480; 
const baseProjectCompleted = 120;
const baseProjectTotal = 300;

const butterfly = computed(() => {
  const isValid = validation?.value?.allPassed || false;
  const duration = parsed.value?.estimatedDurationMinutes || 0;
  const type = parsed.value?.type || 'TASK';
  
  const consumesFocus = type === 'TASK' || type === 'EVENT';
  const currentFocus = isValid && consumesFocus ? Math.max(0, baseFocus - duration) : baseFocus;
  const focusPct = Math.round((currentFocus / baseFocus) * 100);

  const isProjectTask = isValid && type === 'TASK' && parsed.value?.projectQuery;
  const currentProjTotal = isProjectTask ? baseProjectTotal + duration : baseProjectTotal;
  const projPct = Math.round((baseProjectCompleted / currentProjTotal) * 100);
  const baseProjPct = Math.round((baseProjectCompleted / baseProjectTotal) * 100);

  return {
    focus: { current: currentFocus, max: baseFocus, pct: focusPct, changed: currentFocus !== baseFocus },
    project: { currentCompleted: baseProjectCompleted, currentTotal: currentProjTotal, pct: projPct, basePct: baseProjPct, changed: currentProjTotal !== baseProjectTotal }
  };
});
</script>

<template>
  <div class="h-full bg-surface border-l border-borderbase overflow-y-auto flex flex-col font-sans select-none relative transition-colors duration-500"
       :class="validation?.allPassed === false ? 'bg-status-danger-bg/5' : ''">
    
    <!-- HEADER -->
    <div class="sticky top-0 z-20 bg-surface/90 backdrop-blur border-b border-borderbase px-6 py-4 flex items-center justify-between">
      <div class="flex items-center gap-2 text-content font-bold">
        <Activity class="w-5 h-5" :class="validation?.allPassed === false ? 'text-status-danger-text animate-pulse' : 'text-status-success-text'" />
        <span>Pipeline de Processamento</span>
      </div>

      <button 
        @click="isXRayMode = !isXRayMode"
        class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-mono font-semibold transition-all border cursor-pointer shadow-md"
        :class="isXRayMode ? 'bg-content text-content-invert border-content shadow-content/20' : 'bg-app text-content-muted border-borderbase hover:text-content'"
      >
        <component :is="isXRayMode ? Eye : EyeOff" class="w-3.5 h-3.5" />
        <span>{{ isXRayMode ? 'Raio-X Ativo' : 'Modo Raio-X' }}</span>
      </button>
    </div>

    <div v-if="!parsed" class="flex-1 flex flex-col items-center justify-center text-center p-8 opacity-50">
      <Code2 class="w-12 h-12 mb-4 text-content-muted" />
      <p class="font-mono text-sm">Aguardando telemetria do teclado...</p>
    </div>

    <!-- PIPELINE VISUAL -->
    <div v-else class="p-6 space-y-6 pb-20">
      
      <!-- 1. BLOCO PARSER -->
      <div class="rounded-xl border border-borderfocus bg-app p-4 space-y-3">
        <div class="flex items-center gap-2 text-xs font-mono font-bold text-content-muted uppercase">
          <Code2 class="w-4 h-4" /> 1. Parser Engine
        </div>
        <div class="flex flex-wrap gap-2 text-sm font-mono bg-surface p-3 rounded border border-borderbase">
          <span class="text-content break-all">{{ parsed.title || '...' }}</span>
          <span v-if="parsed.rawTokens?.time" class="text-status-warning bg-status-warning-bg px-1 rounded">{{ parsed.rawTokens.time }}</span>
          <span v-if="parsed.rawTokens?.energy" class="text-status-danger-text bg-status-danger-bg px-1 rounded">{{ parsed.rawTokens.energy }}</span>
          <span v-if="parsed.rawTokens?.project" class="text-content-accent bg-surface-active px-1 rounded">{{ parsed.rawTokens.project }}</span>
          <span v-if="parsed.rawTokens?.date" class="text-status-success-text bg-status-success-bg px-1 rounded">{{ parsed.rawTokens.date }}</span>
          <span v-if="parsed.rawTokens?.type" class="text-content-invert bg-content px-1 rounded">{{ parsed.rawTokens.type }}</span>
        </div>
      </div>

      <div class="flex justify-center"><ArrowDown class="w-5 h-5 text-borderfocus" /></div>

      <!-- 2. BLOCO VALIDAÇÃO -->
      <div class="rounded-xl border bg-surface p-4 space-y-3 transition-colors"
           :class="validation?.allPassed ? 'border-borderbase' : 'border-status-danger-border bg-status-danger-bg/20'">
        <div class="flex items-center gap-2 text-xs font-mono font-bold uppercase"
             :class="validation?.allPassed ? 'text-content-muted' : 'text-status-danger-text'">
          <component :is="validation?.allPassed ? CheckCircle2 : ShieldAlert" class="w-4 h-4" /> 
          2. Escudo de Validação
        </div>
        <div class="space-y-1.5">
          <div 
            v-for="(check, i) in validation?.checks" 
            :key="i"
            class="flex items-center gap-2 text-xs font-mono"
            :class="check.passed ? 'text-status-success-text' : 'text-status-danger-text font-bold'"
          >
            <component :is="check.passed ? CheckCircle2 : XCircle" class="w-3.5 h-3.5" />
            <span>{{ check.msg }}</span>
          </div>
        </div>
      </div>

      <div class="flex justify-center"><ArrowDown class="w-5 h-5 text-borderfocus" /></div>

      <!-- 3. MOTOR / POR QUÊ? & DECISION SIMULATOR -->
      <div class="rounded-xl border bg-app p-4 space-y-4"
           :class="engineRouting?.isError ? 'border-status-danger-border' : 'border-borderfocus'">
        <div class="flex items-center gap-2 text-xs font-mono font-bold uppercase"
             :class="engineRouting?.isError ? 'text-status-danger-text' : 'text-content-muted'">
          <Cpu class="w-4 h-4" /> 3. Motor de Decisão (The "Why")
        </div>

        <div class="bg-surface p-4 rounded border border-borderbase space-y-3">
          <div>
            <span class="text-[10px] font-mono uppercase text-content-muted block mb-1">Destino Final:</span>
            <span class="font-bold" :class="engineRouting?.isError ? 'text-status-danger-text' : 'text-content-accent'">
              {{ engineRouting?.destination }}
            </span>
          </div>
          
          <div class="pt-2 border-t border-borderbase">
            <span class="text-[10px] font-mono uppercase text-content-muted block mb-1 flex items-center gap-1">
              <HelpCircle class="w-3 h-3" /> Por que isso aconteceu?
            </span>
            <p class="text-sm text-content leading-relaxed">{{ engineRouting?.why }}</p>
          </div>

          <!-- SIMULADOR DE CÁLCULO DE SCORE (Aparece se for TASK válida) -->
          <div v-if="engineRouting?.scoreBreakdown && validation?.allPassed" class="pt-3 border-t border-borderbase space-y-2 mt-2">
            <span class="text-[10px] font-mono uppercase text-content-muted block flex items-center gap-1">
              <Calculator class="w-3 h-3" /> Como o Compass pontuou esta tarefa?
            </span>
            
            <div class="bg-app border border-borderbase rounded p-3 font-mono text-[11px] space-y-1.5 shadow-inner">
              <div v-for="(item, idx) in engineRouting.scoreBreakdown" :key="idx" class="flex justify-between items-center border-b border-borderbase/50 pb-1 last:border-0 last:pb-0">
                <span class="text-content-muted">{{ item.label }}</span>
                <span class="font-bold" :class="item.color">{{ item.points }}</span>
              </div>
              <div class="flex justify-between items-center pt-2 mt-2 border-t border-borderbase font-bold text-sm">
                <span class="text-content uppercase">Score Final</span>
                <span class="text-content">{{ engineRouting.scoreBreakdown }}/100</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="flex justify-center"><ArrowDown class="w-5 h-5 text-borderfocus" /></div>

      <!-- 4. PREVIEW VISUAL -->
      <div class="rounded-xl border bg-surface p-4 space-y-4"
           :class="validation?.allPassed ? 'border-borderbase' : 'border-status-danger-border opacity-50 grayscale'">
        <div class="flex items-center gap-2 text-xs font-mono font-bold text-content-muted uppercase">
          <Eye class="w-4 h-4" /> 4. Preview da Interface
        </div>
        <div class="pointer-events-none relative">
          <div v-if="!validation?.allPassed" class="absolute inset-0 z-10 flex items-center justify-center bg-app/80 backdrop-blur-[2px] rounded-lg border border-status-danger-border">
            <span class="text-xs font-mono font-bold text-status-danger-text uppercase tracking-widest bg-status-danger-bg px-2 py-1 rounded">Roteamento Abortado</span>
          </div>

          <CommitmentCard 
            :action="{
              commitmentId: 'temp',
              title: parsed.title || '...',
              type: parsed.type,
              nominalDurationMinutes: parsed.estimatedDurationMinutes || 30,
              effectiveDurationMinutes: parsed.estimatedDurationMinutes || 30,
              energyRequired: parsed.energyRequired || 2,
              scorePercentage: engineRouting?.scoreBreakdown || 0,
              reason: 'Priorizado pelo simulador pedagógico.',
              wasTimeAdjustedByEai: false,
              projectName: parsed.projectQuery || null
            }"
          />
        </div>
      </div>

      <!-- 5. EFEITO BORBOLETA (MODO RAIO-X EXCLUSIVO) -->
      <transition
        enter-active-class="transition duration-500 ease-out"
        enter-from-class="opacity-0 translate-y-4"
        enter-to-class="opacity-100 translate-y-0"
        leave-active-class="transition duration-300 ease-in"
        leave-from-class="opacity-100 translate-y-0"
        leave-to-class="opacity-0 translate-y-4"
      >
        <div v-if="isXRayMode" class="rounded-xl border border-content bg-content/5 p-4 space-y-4 mt-6">
          <div class="flex items-center gap-2 text-xs font-mono font-bold text-content uppercase">
            <Network class="w-4 h-4" /> 5. Efeito Borboleta (Mutação em Cadeia)
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div class="p-3 bg-surface rounded border border-borderbase space-y-2 relative overflow-hidden">
              <div class="flex items-center justify-between text-xs font-mono">
                <span class="text-content-muted flex items-center gap-1.5"><Battery class="w-3.5 h-3.5" /> Janela de Foco</span>
                <span :class="butterfly.focus.changed ? 'text-status-danger-text font-bold' : 'text-content'">{{ butterfly.focus.current }}m / {{ butterfly.focus.max }}m</span>
              </div>
              <div class="h-2 w-full bg-app rounded-full overflow-hidden flex">
                <div 
                  class="h-full bg-content transition-all duration-1000 ease-out"
                  :style="{ width: `${butterfly.focus.pct}%` }"
                />
              </div>
            </div>

            <div class="p-3 bg-surface rounded border border-borderbase space-y-2">
              <div class="flex items-center justify-between text-xs font-mono">
                <span class="text-content-muted flex items-center gap-1.5"><Folder class="w-3.5 h-3.5" /> Escopo do Projeto</span>
                <span :class="butterfly.project.changed ? 'text-status-warning font-bold' : 'text-content'">{{ butterfly.project.pct }}% concluído</span>
              </div>
              <div class="h-2 w-full bg-app rounded-full overflow-hidden">
                <div 
                  class="h-full transition-all duration-1000 ease-out"
                  :class="butterfly.project.changed ? 'bg-status-warning' : 'bg-content-accent'"
                  :style="{ width: `${butterfly.project.pct}%` }"
                />
              </div>
            </div>
          </div>
        </div>
      </transition>

    </div>
  </div>
</template>