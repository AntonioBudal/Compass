<script setup lang="ts">
import { ref } from 'vue';
import PageHeader from '@/components/layout/PageHeader.vue';
import { 
  BookOpen, CircleDashed, Calendar, RefreshCw, 
  Target, Folder, FileText, Command, Keyboard, Search, Sparkles
} from 'lucide-vue-next';
import { useToastStore } from '@/stores/toastStore';

const toast = useToastStore();

// --- Controle de Abas ---
const activeTab = ref<'tipos' | 'nlp' | 'atalhos' | 'arquitetura'>('tipos');

// --- Dados Fixos para a Tela (Documentação Viva) ---
const entityTypes = [
  { id: 'task', icon: CircleDashed, title: 'Tarefa (Task)', color: 'text-content', desc: 'Ação única e executável. Entra na fila do Now Engine.', fields: ['Título', 'Duração Estimada', 'Energia Requerida'] },
  { id: 'event', icon: Calendar, title: 'Evento (Hard Blocker)', color: 'text-status-danger-text', desc: 'Compromisso fixo na agenda. Bloqueia o cálculo de tempo livre.', fields: ['Título', 'Data/Hora de Início', 'Data/Hora de Término'] },
  { id: 'habit', icon: RefreshCw, title: 'Hábito (Habit)', color: 'text-content-accent', desc: 'Atividade recorrente. Calcula streaks (sequências) diários.', fields: ['Título', 'Expressão CRON', 'Duração Estimada'] },
  { id: 'goal', icon: Target, title: 'Meta Estratégica', color: 'text-status-success-text', desc: 'Guarda-chuva de longo prazo. Agrupa projetos e módulos.', fields: ['Nome da Meta', 'Propósito (Por quê?)', 'Prazo Alvo'] },
  { id: 'project', icon: Folder, title: 'Projeto', color: 'text-content', desc: 'Agrupador tático. Agrega tarefas e rastreia o tempo investido.', fields: ['Nome do Projeto', 'Meta Vinculada'] },
  { id: 'note', icon: FileText, title: 'Nota (Note)', color: 'text-content-muted', desc: 'Texto livre e logs sem duração ou energia associada.', fields: ['Título', 'Conteúdo'] },
];

const nlpSyntax = [
  { command: '@45m', desc: 'Define a duração da tarefa para 45 minutos.', example: 'Revisar PR @45m' },
  { command: '!3', desc: 'Define o nível de energia requerida (1=MAINT, 2=OPER, 3=DEEP).', example: 'Arquitetura do Banco !3' },
  { command: '#Projeto', desc: 'Vincula a tarefa a um projeto existente (com Autocomplete).', example: 'Criar tela de Login #Compass' },
  { command: '^amanhã', desc: 'Define uma data/prazo alvo natural.', example: 'Pagar boleto ^amanhã' },
  { command: '/h', desc: 'Força o sistema a interpretar como Hábito (CRON Diário).', example: 'Ler 10 páginas /h' },
  { command: '/e', desc: 'Força o sistema a interpretar como Evento (Pede horários).', example: 'Dentista /e' },
];

const shortcuts = [
  { key: 'Cmd+K / Ctrl+K', desc: 'Abrir barra de comando global (K-Menu)' },
  { key: 'C', desc: 'Abrir o Quick Capture (NLP) para novos compromissos' },
  { key: 'E', desc: 'Concluir imediatamente o Top Focus atual no Now Engine' },
  { key: 'S', desc: 'Adiar/Pular o Top Focus atual para o final da fila' },
  { key: 'Cmd+Z / Ctrl+Z', desc: 'Desfazer (Undo) a última exclusão ou mutação' },
  { key: 'Esc', desc: 'Fechar o Inspetor Universal ou Modais' },
  { key: 'Tab', desc: 'Completar automaticamente o Projeto ou Data no NLP' },
];

const copyToClipboard = (text: string) => {
  navigator.clipboard.writeText(text);
  toast.showToast('Comando copiado para a área de transferência.', 'neutral');
};
</script>

<template>
  <div class="max-w-5xl mx-auto space-y-6 select-none pb-12">
    
    <PageHeader 
      title="Biblioteca do Domínio"
      description="Documentação viva da arquitetura, motor de linguagem (NLP) e modelo mental do Compass."
      :actionIcon="BookOpen"
      viewName="library"
      :showDensityToggle="false"
    />

    <!-- NAVEGAÇÃO POR ABAS -->
    <div class="flex flex-wrap items-center gap-2 font-mono text-xs border-b border-borderbase pb-2">
      <button @click="activeTab = 'tipos'" :class="activeTab === 'tipos' ? 'bg-surface-active text-content border border-borderfocus' : 'text-content-muted hover:text-content'" class="px-4 py-2 rounded-lg font-semibold transition-colors cursor-pointer">
        1. Ontologia de Tipos
      </button>
      <button @click="activeTab = 'nlp'" :class="activeTab === 'nlp' ? 'bg-surface-active text-content border border-borderfocus' : 'text-content-muted hover:text-content'" class="px-4 py-2 rounded-lg font-semibold transition-colors cursor-pointer flex items-center gap-1.5">
        <Sparkles class="w-3.5 h-3.5" /> 2. Sintaxe (NLP)
      </button>
      <button @click="activeTab = 'atalhos'" :class="activeTab === 'atalhos' ? 'bg-surface-active text-content border border-borderfocus' : 'text-content-muted hover:text-content'" class="px-4 py-2 rounded-lg font-semibold transition-colors cursor-pointer flex items-center gap-1.5">
        <Keyboard class="w-3.5 h-3.5" /> 3. Teclado
      </button>
      <button @click="activeTab = 'arquitetura'" :class="activeTab === 'arquitetura' ? 'bg-surface-active text-content border border-borderfocus' : 'text-content-muted hover:text-content'" class="px-4 py-2 rounded-lg font-semibold transition-colors cursor-pointer">
        4. Modelo Mental
      </button>
    </div>

    <!-- CONTEÚDO DAS ABAS -->
    <div class="pt-2">

      <!-- ABA 1: TIPOS -->
      <transition name="fade" mode="out-in">
        <div v-if="activeTab === 'tipos'" class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div v-for="type in entityTypes" :key="type.id" class="p-5 rounded-xl bg-surface border border-borderbase relative overflow-hidden group">
            <component :is="type.icon" class="absolute -right-4 -bottom-4 w-24 h-24 opacity-[0.03] text-content pointer-events-none group-hover:opacity-[0.06] transition-opacity" />
            <div class="flex items-center gap-2.5 mb-3">
              <component :is="type.icon" class="w-5 h-5" :class="type.color" />
              <h3 class="text-base font-bold text-content tracking-tight">{{ type.title }}</h3>
            </div>
            <p class="text-xs text-content-muted mb-4 h-8 leading-relaxed">{{ type.desc }}</p>
            <div class="space-y-1.5">
              <span class="text-[10px] font-mono uppercase text-content-muted tracking-wider">Campos Cruciais:</span>
              <div class="flex flex-wrap gap-1.5">
                <span v-for="field in type.fields" :key="field" class="px-2 py-1 bg-app border border-borderbase rounded text-[10px] font-mono text-content">
                  {{ field }}
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- ABA 2: NLP SINTAXE -->
        <div v-else-if="activeTab === 'nlp'" class="space-y-6">
          <div class="p-5 rounded-xl border border-borderbase bg-app/50 space-y-2">
            <h3 class="text-sm font-semibold text-content flex items-center gap-2"><Command class="w-4 h-4" /> Quick Capture (Motor NLP)</h3>
            <p class="text-xs text-content-muted leading-relaxed">
              Pressione <kbd class="px-1.5 py-0.5 rounded bg-surface border border-borderbase text-content">C</kbd> em qualquer tela. 
              O parser local traduz texto humano em estruturas de dados complexas em milissegundos. Clique em um comando para copiá-lo.
            </p>
          </div>

          <div class="border border-borderbase rounded-xl overflow-hidden bg-surface">
            <div class="grid grid-cols-12 gap-4 px-4 py-3 bg-surface border-b border-borderbase text-[11px] font-mono font-semibold text-content-muted uppercase tracking-wider">
              <div class="col-span-3">Modificador</div>
              <div class="col-span-5">Ação do Parser</div>
              <div class="col-span-4">Como escrever</div>
            </div>
            <div class="divide-y divide-borderbase">
              <div v-for="rule in nlpSyntax" :key="rule.command" class="grid grid-cols-12 gap-4 py-3.5 px-4 items-center hover:bg-surface-hover transition-colors">
                <div class="col-span-3">
                  <button @click="copyToClipboard(rule.command)" class="px-2.5 py-1.5 bg-app border border-borderbase rounded text-xs font-mono font-bold text-content hover:border-borderfocus transition-colors cursor-pointer" title="Clique para copiar">
                    {{ rule.command }}
                  </button>
                </div>
                <div class="col-span-5 text-xs text-content-muted leading-relaxed">{{ rule.desc }}</div>
                <div class="col-span-4 text-[11px] font-mono text-content-accent opacity-80">ex: {{ rule.example }}</div>
              </div>
            </div>
          </div>
        </div>

        <!-- ABA 3: ATALHOS DE TECLADO (Migrado das Configurações) -->
        <div v-else-if="activeTab === 'atalhos'" class="border border-borderbase rounded-xl overflow-hidden bg-surface">
          <div class="p-5 border-b border-borderbase bg-app/50">
             <p class="text-xs text-content-muted">O Compass foi desenhado com a filosofia <strong>Zero-Mouse</strong>. Mantenha as mãos no teclado.</p>
          </div>
          <div class="grid grid-cols-12 gap-4 px-4 py-3 bg-surface border-b border-borderbase text-[11px] font-mono font-semibold text-content-muted uppercase tracking-wider">
            <div class="col-span-4">Comando Global</div>
            <div class="col-span-8">Ação no Sistema</div>
          </div>
          <div class="divide-y divide-borderbase">
            <div v-for="shortcut in shortcuts" :key="shortcut.key" class="grid grid-cols-12 gap-4 py-3.5 px-4 items-center hover:bg-surface-hover transition-colors">
              <div class="col-span-4">
                <kbd class="px-2.5 py-1.5 bg-app border border-borderbase rounded text-xs font-mono font-bold text-content shadow-sm">{{ shortcut.key }}</kbd>
              </div>
              <div class="col-span-8 text-xs text-content-muted">{{ shortcut.desc }}</div>
            </div>
          </div>
        </div>

        <!-- ABA 4: ARQUITETURA E MODELO MENTAL -->
        <div v-else-if="activeTab === 'arquitetura'" class="p-8 rounded-xl border border-borderbase bg-app flex flex-col items-center justify-center space-y-6">
          <div class="text-center max-w-lg mb-4">
             <h3 class="text-lg font-bold text-content mb-2">O Determinismo do Now Engine</h3>
             <p class="text-xs text-content-muted">O Compass não é uma To-Do list passiva. Ele é um funil tático que empurra suas ideias até a execução sem exigir que você ordene coisas manualmente.</p>
          </div>

          <!-- Fluxograma CSS -->
          <div class="w-full max-w-2xl font-mono text-xs font-bold text-content flex flex-col items-center gap-2">
            <!-- Camada 1 -->
            <div class="w-full p-4 border border-dashed border-content-muted rounded-lg text-center bg-surface">
              🧠 Ideia Bruta (O que preciso fazer?)
            </div>
            
            <div class="w-px h-6 bg-borderfocus mx-auto animate-pulse"></div>
            
            <!-- Camada 2 -->
            <div class="w-3/4 p-4 border border-borderfocus rounded-lg text-center bg-surface-active shadow-md">
              ⌨️ Quick Capture (Uso do NLP)
            </div>

            <div class="w-px h-6 bg-borderfocus mx-auto animate-pulse"></div>

            <!-- Camada 3: Bifurcação -->
            <div class="w-full flex justify-between gap-6">
              <div class="flex-1 flex flex-col items-center gap-2">
                <span class="text-[10px] text-content-muted uppercase">Com horário fixo (Hard Blocker)</span>
                <div class="w-full p-6 border-l-4 border-l-status-danger-border border-y border-r border-borderbase rounded-r-lg text-center bg-surface">
                  📅 Vira EVENTO e bloqueia a Agenda
                </div>
              </div>
              
              <div class="flex-1 flex flex-col items-center gap-2">
                <span class="text-[10px] text-content-muted uppercase">Ação Flexível (Soft Blocker)</span>
                <div class="w-full p-6 border-l-4 border-l-content border-y border-r border-borderbase rounded-r-lg text-center bg-surface">
                  🎯 Cai no Now Engine e disputa o Top Focus
                </div>
              </div>
            </div>
          </div>
        </div>
      </transition>
    </div>
  </div>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}
.fade-enter-from {
  opacity: 0;
  transform: translateY(5px);
}
.fade-leave-to {
  opacity: 0;
  transform: translateY(-5px);
}
</style>