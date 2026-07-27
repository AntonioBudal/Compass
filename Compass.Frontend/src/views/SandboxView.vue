<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useOnboardingStore } from '@/stores/onboardingStore';
import { useToastStore } from '@/stores/toastStore';
import SpotlightOverlay from '@/components/onboarding/SpotlightOverlay.vue';
import OnboardingSteps from '@/components/onboarding/OnboardingSteps.vue';
import { 
  Terminal, BookOpen, XCircle, CheckCircle2, 
  Clock, RefreshCw, FileText, Trash2 
} from 'lucide-vue-next';

const onboardingStore = useOnboardingStore();
const toastStore = useToastStore();

const isTrainingOpen = ref(false);

const unlockNavigation = () => {
  try {
    localStorage.setItem('compass_onboarded', 'true');
  } catch (e) {}
};

const handleTrainingComplete = () => {
  isTrainingOpen.value = false;
  unlockNavigation(); 
  toastStore.showToast('Tutorial concluído! O menu lateral está 100% liberado.', 'success');
};

const handleTrainingClose = () => {
  isTrainingOpen.value = false;
  unlockNavigation(); 
};

onMounted(() => {
  onboardingStore.startTutorialMode();
  setTimeout(() => {
    isTrainingOpen.value = true;
  }, 300);
});
</script>

<template>
  <div class="min-h-screen w-full bg-app text-content flex flex-col justify-between p-6 md:p-12 select-none relative font-mono">
    
    <!-- BANNER SUPERIOR DO SANDBOX -->
    <header class="max-w-4xl mx-auto w-full flex items-center justify-between pb-6 border-b border-borderbase">
      <div class="flex items-center gap-3">
        <div class="p-2 rounded bg-surface border border-borderfocus text-content">
          <Terminal class="w-5 h-5 stroke-[2.5]" />
        </div>
        <div>
          <h1 class="text-sm font-bold tracking-wider uppercase">Compass RAM Sandbox</h1>
          <p class="text-[11px] text-content-muted">Ambiente Pedagógico em Memória (Zero-DB)</p>
        </div>
      </div>

      <div class="flex items-center gap-3">
        <button 
          @click="isTrainingOpen = true"
          class="text-xs px-3 py-1.5 rounded bg-surface border border-borderbase hover:border-borderfocus text-content transition-all flex items-center gap-1.5 cursor-pointer shadow-sm"
        >
          <BookOpen class="w-3.5 h-3.5 text-content" />
          <span>Abrir Guia [T]</span>
        </button>

        <button 
          @click="onboardingStore.skipOnboarding()"
          class="text-xs text-content-muted hover:text-status-danger-text transition-colors flex items-center gap-1.5 cursor-pointer"
        >
          <span>Sair para App Real</span>
          <XCircle class="w-4 h-4" />
        </button>
      </div>
    </header>

    <!-- ÁREA DE VISUALIZAÇÃO AO FUNDO (Mostra os itens criados durante o tutorial) -->
    <main class="max-w-4xl mx-auto w-full flex-1 py-8">
      <div class="flex items-center justify-between mb-4">
        <span class="text-xs uppercase tracking-wider text-content-muted font-bold">
          Itens Gerados na Memória RAM (Sua Fila de Teste):
        </span>
        <button 
          v-if="onboardingStore.commitments.length > 0"
          @click="onboardingStore.commitments = []"
          class="text-[11px] text-content-muted hover:text-status-danger-text flex items-center gap-1 cursor-pointer transition-colors"
        >
          <Trash2 class="w-3 h-3" /> Limpar RAM
        </button>
      </div>

      <!-- Lista de Itens do Sandbox -->
      <div v-if="onboardingStore.commitments.length > 0" class="space-y-2.5">
        <div 
          v-for="item in onboardingStore.commitments" 
          :key="item.id"
          @click="onboardingStore.toggleComplete(item.id)"
          class="p-3.5 rounded-lg bg-surface border border-borderbase hover:border-borderfocus transition-all flex items-center justify-between cursor-pointer group shadow-sm"
          :class="{ 'opacity-50 line-through bg-surface-hover': item.status === 'completed' }"
        >
          <div class="flex items-center gap-3">
            <span class="p-1.5 rounded bg-app border border-borderbase text-content">
              <CheckCircle2 v-if="item.type === 'TASK'" class="w-4 h-4" />
              <Clock v-else-if="item.type === 'EVENT'" class="w-4 h-4" />
              <RefreshCw v-else-if="item.type === 'HABIT'" class="w-4 h-4" />
              <FileText v-else class="w-4 h-4" />
            </span>
            <span class="text-sm font-sans font-medium text-content group-hover:underline">
              {{ item.title }}
            </span>
          </div>

          <div class="flex items-center gap-2 text-xs text-content-muted font-mono">
            <span class="px-2 py-0.5 rounded bg-app border border-borderbase uppercase text-[10px]">
              {{ item.type }}
            </span>
            <span v-if="item.estimatedDurationMinutes > 0">{{ item.estimatedDurationMinutes }}m</span>
          </div>
        </div>
      </div>

      <!-- Estado Vazio ao iniciar o tutorial -->
      <div v-else class="h-64 rounded-xl border-2 border-dashed border-borderbase/60 flex flex-col items-center justify-center text-content-muted/60">
        <Terminal class="w-8 h-8 mb-2 stroke-1" />
        <p class="text-sm font-sans">A memória RAM está limpa.</p>
        <p class="text-xs font-mono">Siga o Guia Interativo para povoar este ambiente.</p>
      </div>
    </main>

    <!-- MODAL DO TUTORIAL -->
    <SpotlightOverlay :is-open="isTrainingOpen" @close="handleTrainingClose">
      <OnboardingSteps 
        @complete="handleTrainingComplete" 
        @skip="handleTrainingClose" 
      />
    </SpotlightOverlay>

  </div>
</template>