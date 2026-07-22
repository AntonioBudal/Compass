<script setup lang="ts">
import { useJournalStore } from '@/stores/journalStore';
import { useCommitmentsStore } from '@/stores/commitmentsStore';
import EnergyIndicator from '@/components/core/EnergyIndicator.vue';
import { 
  Power, Check, Clock, ArrowRight, Trash2, 
  RotateCcw, ShieldCheck, CheckCircle2 
} from 'lucide-vue-next';

const journalStore = useJournalStore();
const commitmentsStore = useCommitmentsStore();

const handleKeyDown = (e: KeyboardEvent) => {
  if (!journalStore.isShutdownOpen) return;

  if (e.key === 'Escape') {
    journalStore.isShutdownOpen = false;
    return;
  }

  // Atalhos de processamento rápido no Passo 1
  if (journalStore.shutdownStep === 1 && journalStore.pendingForShutdown.length > 0) {
    const currentItem = journalStore.pendingForShutdown[0];
    if (e.key === '1') {
      e.preventDefault();
      journalStore.postponeItem(currentItem);
    } else if (e.key === '2') {
      e.preventDefault();
      journalStore.completeItem(currentItem);
    } else if (e.key === '3') {
      e.preventDefault();
      journalStore.archiveItem(currentItem);
    }
  }

  // Avançar etapas com Enter
  if (e.key === 'Enter' && !e.shiftKey) {
    e.preventDefault();
    if (journalStore.shutdownStep === 1 && journalStore.pendingForShutdown.length === 0) {
      journalStore.shutdownStep = 2;
    } else if (journalStore.shutdownStep === 2) {
      journalStore.shutdownStep = 3;
    } else if (journalStore.shutdownStep === 3) {
      journalStore.finishShutdown();
    }
  }
};
</script>

<template>
  <transition name="modal-snap">
    <div 
      v-if="journalStore.isShutdownOpen" 
      class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-app/70 backdrop-blur-sm select-none"
      @keydown="handleKeyDown"
      tabindex="0"
    >
      <div class="w-full max-w-2xl bg-surface border border-borderbase rounded-xl shadow-2xl overflow-hidden flex flex-col min-h-[420px] max-h-[85vh]">
        
        <!-- Cabeçalho do Ritual -->
        <div class="px-6 py-4 bg-app border-b border-borderbase flex items-center justify-between">
          <div class="flex items-center gap-2.5 text-content font-semibold text-sm">
            <Power class="w-4 h-4 text-content-muted" />
            <span>Ritual de Fechamento de Turno</span>
          </div>
          
          <!-- Indicador de Etapa Monocromático -->
          <div class="flex items-center gap-2 font-mono text-xs text-content-muted">
            <span :class="journalStore.shutdownStep === 1 ? 'text-content font-bold' : ''">01 Pendências</span>
            <span>/</span>
            <span :class="journalStore.shutdownStep === 2 ? 'text-content font-bold' : ''">02 Hábitos</span>
            <span>/</span>
            <span :class="journalStore.shutdownStep === 3 ? 'text-content font-bold' : ''">03 Encerramento</span>
          </div>
        </div>

        <!-- CONTEÚDO ETAPA 1: Processar Pendências do Dia -->
        <div v-if="journalStore.shutdownStep === 1" class="p-6 flex-1 flex flex-col justify-between space-y-6 overflow-y-auto">
          <div class="space-y-1">
            <h3 class="text-base font-semibold text-content">Varredura de Tarefas Remanescentes</h3>
            <p class="text-xs text-content-muted">
              Decida o destino de cada item pendente para zerar a carga cognitiva de hoje.
            </p>
          </div>

          <!-- Fila de Processamento (Zero Ruído) -->
          <div v-if="journalStore.pendingForShutdown.length > 0" class="flex-1 flex flex-col justify-center space-y-4 my-auto">
            <div class="p-5 rounded-lg border border-borderbase bg-app/60 space-y-4">
              <div class="flex items-start justify-between gap-4">
                <div class="space-y-1">
                  <span class="text-[10px] font-mono uppercase bg-surface-active text-content-muted px-1.5 py-0.5 rounded border border-borderbase">
                    Restam {{ journalStore.pendingForShutdown.length }} itens
                  </span>
                  <h4 class="text-lg font-semibold text-content pt-1">
                    {{ journalStore.pendingForShutdown[0].title }}
                  </h4>
                </div>
                <EnergyIndicator :level="journalStore.pendingForShutdown[0].energyRequired || 2" />
              </div>

              <div class="flex items-center gap-4 text-xs font-mono text-content-muted pt-2 border-t border-borderbase">
                <span class="flex items-center gap-1"><Clock class="w-3.5 h-3.5" /> {{ journalStore.pendingForShutdown[0].estimatedDurationMinutes }}m</span>
                <span v-if="journalStore.pendingForShutdown[0].projectName">[{ journalStore.pendingForShutdown[0].projectName }]</span>
              </div>
            </div>

            <!-- Botões de Decisão Rápida (<kbd>1</kbd>, <kbd>2</kbd>, <kbd>3</kbd>) -->
            <div class="grid grid-cols-3 gap-3 pt-2">
              <button 
                @click="journalStore.postponeItem(journalStore.pendingForShutdown[0])"
                class="flex flex-col items-center justify-center p-3 rounded-tactic bg-surface-active hover:bg-surface-hover text-content text-xs font-medium border border-borderbase transition-colors cursor-pointer gap-1.5"
              >
                <div class="flex items-center gap-1.5">
                  <RotateCcw class="w-3.5 h-3.5 text-content-muted" />
                  <span>Adiar p/ Amanhã</span>
                </div>
                <kbd class="text-[10px] font-mono bg-app px-1 rounded text-content-muted">Tecla 1</kbd>
              </button>

              <button 
                @click="journalStore.completeItem(journalStore.pendingForShutdown[0])"
                class="flex flex-col items-center justify-center p-3 rounded-tactic bg-content hover:bg-content-accent text-content-invert text-xs font-semibold shadow-sm transition-all cursor-pointer gap-1.5"
              >
                <div class="flex items-center gap-1.5">
                  <Check class="w-3.5 h-3.5 stroke-[2.5]" />
                  <span>Concluir Agora</span>
                </div>
                <kbd class="text-[10px] font-mono bg-surface text-content border border-borderbase px-1 rounded">Tecla 2</kbd>
              </button>

              <button 
                @click="journalStore.archiveItem(journalStore.pendingForShutdown[0])"
                class="flex flex-col items-center justify-center p-3 rounded-tactic bg-surface hover:bg-surface-hover text-content-muted hover:text-status-danger-text text-xs font-medium border border-borderbase transition-colors cursor-pointer gap-1.5"
              >
                <div class="flex items-center gap-1.5">
                  <Trash2 class="w-3.5 h-3.5" />
                  <span>Descartar</span>
                </div>
                <kbd class="text-[10px] font-mono bg-app px-1 rounded text-content-muted">Tecla 3</kbd>
              </button>
            </div>
          </div>

          <!-- Todas as tarefas resolvidas -->
          <div v-else class="flex-1 flex flex-col items-center justify-center text-center space-y-3 py-8">
            <CheckCircle2 class="w-10 h-10 text-status-success-text stroke-[1.5]" />
            <div class="space-y-1">
              <h4 class="text-base font-semibold text-content">Pendências de Tarefas Zeradas</h4>
              <p class="text-xs text-content-muted">Nenhuma tarefa avulsa remanescente para o turno de hoje.</p>
            </div>
          </div>

          <!-- Rodapé Etapa 1 -->
          <div class="pt-4 border-t border-borderbase flex items-center justify-between">
            <button 
              v-if="journalStore.pendingForShutdown.length > 1"
              @click="journalStore.postponeAllRemaining"
              class="text-xs text-content-muted hover:text-content underline font-mono cursor-pointer"
            >
              Adiar todas as {{ journalStore.pendingForShutdown.length }} remanescentes
            </button>
            <span v-else />

            <button 
              @click="journalStore.shutdownStep = 2"
              class="inline-flex items-center gap-2 px-4 py-2 rounded-tactic bg-surface-active hover:bg-surface-hover text-content text-xs font-medium transition-colors cursor-pointer ml-auto"
            >
              <span>Avançar para Hábitos</span>
              <ArrowRight class="w-3.5 h-3.5" />
            </button>
          </div>
        </div>

        <!-- CONTEÚDO ETAPA 2: Checagem Final de Hábitos -->
        <div v-else-if="journalStore.shutdownStep === 2" class="p-6 flex-1 flex flex-col justify-between space-y-6 overflow-y-auto">
          <div class="space-y-1">
            <h3 class="text-base font-semibold text-content">Consistência de Hábitos Diários</h3>
            <p class="text-xs text-content-muted">Verifique se algum hábito rotineiro foi concluído antes do fechamento.</p>
          </div>

          <div class="space-y-2 flex-1">
            <div 
              v-for="habit in commitmentsStore.habitsToday" 
              :key="habit.id"
              class="flex items-center justify-between p-3 rounded-lg border border-borderbase bg-app/60"
            >
              <span class="text-sm text-content font-medium">{{ habit.title }}</span>
              <button 
                @click="commitmentsStore.updateStatus(habit.id, habit.status === 'COMPLETED' ? 'PENDING' : 'COMPLETED')"
                class="px-3 py-1 rounded text-xs font-mono border transition-all cursor-pointer"
                :class="habit.status === 'COMPLETED' ? 'bg-status-success-bg text-status-success-text border-status-success-border' : 'bg-surface-active text-content-muted border-borderbase hover:text-content'"
              >
                {{ habit.status === 'COMPLETED' ? '[x] CONCLUÍDO' : '[ ] PENDENTE' }}
              </button>
            </div>
          </div>

          <div class="pt-4 border-t border-borderbase flex items-center justify-between">
            <button 
              @click="journalStore.shutdownStep = 1"
              class="px-3 py-2 rounded hover:bg-surface-hover text-content-muted text-xs font-medium transition-colors cursor-pointer"
            >
              Voltar
            </button>

            <button 
              @click="journalStore.shutdownStep = 3"
              class="inline-flex items-center gap-2 px-4 py-2 rounded-tactic bg-content hover:bg-content-accent text-content-invert text-xs font-semibold shadow-sm transition-all cursor-pointer"
            >
              <span>Avançar para Resumo</span>
              <ArrowRight class="w-3.5 h-3.5" />
            </button>
          </div>
        </div>

        <!-- CONTEÚDO ETAPA 3: Encerramento do Turno -->
        <div v-else-if="journalStore.shutdownStep === 3" class="p-6 flex-1 flex flex-col justify-between space-y-6 text-center">
          <div class="my-auto space-y-4 max-w-sm mx-auto">
            <div class="w-12 h-12 rounded-full bg-surface-active border border-borderbase flex items-center justify-center mx-auto text-content">
              <ShieldCheck class="w-6 h-6" />
            </div>

            <div class="space-y-1">
              <h3 class="text-lg font-semibold text-content">Turno Prontamente Auditado</h3>
              <p class="text-xs text-content-muted leading-relaxed">
                Suas listas estão sincronizadas e suas pendências foram remanejadas para o próximo ciclo operacional.
              </p>
            </div>

            <!-- Resumo Tabular Monocromático -->
            <div class="p-4 rounded-lg bg-app border border-borderbase font-mono text-xs space-y-2 text-left">
              <div class="flex justify-between text-content-muted">
                <span>Itens Concluídos Hoje:</span>
                <span class="text-content font-bold">{{ journalStore.auditMetrics.totalCompleted }}</span>
              </div>
              <div class="flex justify-between text-content-muted">
                <span>Tempo em Deep Work:</span>
                <span class="text-content font-bold">{{ journalStore.auditMetrics.deepWorkMinutes }} min</span>
              </div>
              <div class="flex justify-between text-content-muted">
                <span>Volume Total Auditado:</span>
                <span class="text-content font-bold">{{ journalStore.auditMetrics.totalMinutes }} min</span>
              </div>
            </div>
          </div>

          <div class="pt-4 border-t border-borderbase flex justify-end">
            <button 
              @click="journalStore.finishShutdown"
              class="w-full inline-flex items-center justify-center gap-2 px-4 py-2.5 rounded-tactic bg-content hover:bg-content-accent text-content-invert text-xs font-semibold shadow-sm transition-all cursor-pointer"
            >
              <Power class="w-4 h-4" />
              <span>Desconectar e Encerrar Turno</span>
              <kbd class="ml-1 text-[10px] font-mono bg-surface text-content border border-borderbase px-1 rounded">Enter</kbd>
            </button>
          </div>
        </div>

      </div>
    </div>
  </transition>
</template>

<style scoped>
.modal-snap-enter-active,
.modal-snap-leave-active {
  transition: opacity 150ms cubic-bezier(0.16, 1, 0.3, 1), transform 150ms cubic-bezier(0.16, 1, 0.3, 1);
}
.modal-snap-enter-from,
.modal-snap-leave-to {
  opacity: 0;
  transform: scale(0.96);
}
</style>