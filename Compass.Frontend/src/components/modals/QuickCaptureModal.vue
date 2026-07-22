<script setup lang="ts">
import { ref, watch, nextTick, computed } from 'vue';
import { useRoute } from 'vue-router';
import { useCommitmentsStore } from '@/stores/commitmentsStore';
import { useToastStore } from '@/stores/toastStore';
import { useQuickCaptureParser } from '@/composables/useQuickCaptureParser';
import { isQuickCaptureOpen, editingCommitment } from '@/composables/useKeyboardShortcuts';
import type { CommitmentType, CreateCommitmentDto } from '@/types/index';
import { Terminal, CornerDownLeft, Clock, Folder } from 'lucide-vue-next';

const route = useRoute();
const commitmentsStore = useCommitmentsStore();
const toastStore = useToastStore();
const { parseInput } = useQuickCaptureParser();

const rawInput = ref('');
const inputRef = ref<HTMLInputElement | null>(null);
const isSubmitting = ref(false);

// Herança de Contexto (Context-Aware Defaults com base na Rota do Vue Router)
const defaultContextType = computed<CommitmentType>(() => {
  const path = route.path;
  if (path.includes('/habits')) return 'HABIT';
  if (path.includes('/agenda')) return 'EVENT';
  if (path.includes('/journal')) return 'NOTE';
  return 'TASK';
});

// Inferência NLP em tempo real para feedback visual (Zero-Mouse)
const parsedPreview = computed(() => {
  return parseInput(rawInput.value, defaultContextType.value, 30, 2);
});

const visualEnergyLabel = computed(() => {
  switch (parsedPreview.value.energyRequired) {
    case 3: return '■■■ DEEP';
    case 1: return '■□□ MAINT';
    case 2:
    default: return '■■□ OPER';
  }
});

watch(isQuickCaptureOpen, async (isOpen) => {
  if (isOpen) {
    rawInput.value = '';
    isSubmitting.value = false;
    await nextTick();
    inputRef.value?.focus();
  }
});

const handleSubmit = async () => {
  if (!rawInput.value.trim() || isSubmitting.value) return;

  isSubmitting.value = true;
  const parsed = parsedPreview.value;

  // Montagem defensiva do DTO: preenchendo datas padrão para evitar falha no FluentValidation do .NET 10
  const now = new Date();
  const oneHourLater = new Date(now.getTime() + 60 * 60 * 1000);

  const payload: CreateCommitmentDto = {
    title: parsed.cleanTitle,
    type: parsed.type,
    estimatedDurationMinutes: parsed.estimatedDurationMinutes,
    energyRequired: parsed.energyRequired,
    // Se for EVENT e o usuário não passou data, assumimos o próximo intervalo inteiro para satisfazer a API
    startTime: parsed.type === 'EVENT' ? now.toISOString() : null,
    endTime: parsed.type === 'EVENT' ? oneHourLater.toISOString() : null,
    // Se for HABIT, assumimos execução diária padrão à meia-noite
    cronExpression: parsed.type === 'HABIT' ? '0 0 * * *' : null,
    content: parsed.projectToken ? `Projeto vinculado via token: #${parsed.projectToken}` : null
  };

  try {
    const created = await commitmentsStore.createCommitment(payload);
    isQuickCaptureOpen.value = false;
    
    // Feedback ergonômico no Toast com atalho direto para o Estágio 2 de Refinamento
    toastStore.showToast(
      `[${parsed.type}] "${parsed.cleanTitle}" capturado.`,
      'success',
      async () => {
        // Ação de Desfazer em RAM
        await commitmentsStore.deleteCommitment(created.id);
      },
      5000
    );
  } catch (err: any) {
    // O detalhe do erro já foi propagado e interceptado pela store/axios
    console.error('[QuickCapture:Error] Submissão abortada:', err);
  } finally {
    isSubmitting.value = false;
  }
};

const handleKeyDown = (e: KeyboardEvent) => {
  if (e.key === 'Enter' && !e.shiftKey) {
    e.preventDefault();
    handleSubmit();
  }
};
</script>

<template>
  <transition name="modal-snap">
    <div 
      v-if="isQuickCaptureOpen" 
      class="fixed inset-0 z-50 flex items-start justify-center pt-[20vh] px-4 bg-app/75 backdrop-blur-sm select-none"
      @click.self="isQuickCaptureOpen = false"
    >
      <div 
        class="w-full max-w-xl bg-surface border border-borderbase shadow-2xl rounded-xl overflow-hidden flex flex-col transition-all duration-tactic gpu-accelerated"
        @keydown="handleKeyDown"
      >
        <!-- Barra de Digitação de Comando CLI -->
        <div class="relative flex items-center px-4 py-3 border-b border-borderbase bg-app/60">
          <Terminal class="w-5 h-5 text-content-muted flex-shrink-0 mr-3" />
          <input 
            ref="inputRef"
            v-model="rawInput"
            type="text" 
            placeholder="Digitar ação... (Use @30m, !3, #proj, /h para formatar)" 
            class="w-full py-2 bg-transparent text-base text-content placeholder:text-content-muted focus:outline-none font-sans font-medium"
            :disabled="isSubmitting"
          />
          <div class="flex items-center gap-1.5 ml-2">
            <kbd class="px-1.5 py-0.5 text-[10px] font-mono bg-surface text-content-muted rounded border border-borderbase">ESC</kbd>
            <kbd class="px-1.5 py-0.5 text-[10px] font-mono bg-content text-content-invert font-bold rounded">↵</kbd>
          </div>
        </div>

        <!-- Área de Inferência Realtime (Badges de Metadados NLP) -->
        <div class="px-4 py-2.5 bg-app flex flex-wrap items-center justify-between gap-2 border-b border-borderbase text-xs font-mono text-content-muted">
          <div class="flex items-center gap-2 overflow-x-auto py-0.5">
            <!-- Badge de Tipo/Arquétipo -->
            <span 
              class="px-1.5 py-0.5 rounded border text-[10px] uppercase font-bold tracking-wider"
              :class="parsedPreview.explicitType ? 'bg-surface-active text-content-accent border-borderhighlight' : 'bg-surface text-content-muted border-borderbase'"
            >
              {{ parsedPreview.type }}
            </span>

            <!-- Badge de Duração -->
            <span class="inline-flex items-center gap-1 px-1.5 py-0.5 rounded bg-surface border border-borderbase text-content">
              <Clock class="w-3 h-3 text-content-muted" />
              <span>{{ parsedPreview.estimatedDurationMinutes }}m</span>
            </span>

            <!-- Badge de Energia Geométrico -->
            <span class="inline-flex items-center gap-1 px-1.5 py-0.5 rounded bg-surface border border-borderbase text-content">
              <span>{{ visualEnergyLabel }}</span>
            </span>

            <!-- Badge de Projeto Tokenizado -->
            <span 
              v-if="parsedPreview.projectToken" 
              class="inline-flex items-center gap-1 px-1.5 py-0.5 rounded bg-surface border border-borderfocus text-content font-sans font-medium"
            >
              <Folder class="w-3 h-3 text-content-muted" />
              <span>#{{ parsedPreview.projectToken }}</span>
            </span>
          </div>

          <!-- Dica Ergonômica -->
          <span class="text-[10px] text-content-muted hidden sm:inline-block">
            Estágio 1 (Brain Dump)
          </span>
        </div>

        <!-- Rodapé Guia Monocromático -->
        <div class="px-4 py-2 bg-app/30 flex items-center justify-between text-[10px] font-mono text-content-muted">
          <div class="flex items-center gap-3">
            <span><strong class="text-content">@15m</strong> Tempo</span>
            <span><strong class="text-content">!1 a !3</strong> Energia</span>
            <span><strong class="text-content">/h /e /t</strong> Tipo</span>
          </div>
          <span>Compass Zero-Mouse v2.0</span>
        </div>
      </div>
    </div>
  </transition>
</template>

<style scoped>
.modal-snap-enter-active,
.modal-snap-leave-active {
  transition: opacity 120ms cubic-bezier(0.16, 1, 0.3, 1), transform 120ms cubic-bezier(0.16, 1, 0.3, 1);
}
.modal-snap-enter-from,
.modal-snap-leave-to {
  opacity: 0;
  transform: scale(0.97) translateY(-4px);
}
</style>