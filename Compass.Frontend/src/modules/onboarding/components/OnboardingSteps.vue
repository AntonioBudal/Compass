<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useProjectsStore } from '@/modules/strategy/stores/projectsStore';
import { CheckCircle2, Circle, X, Rocket, ArrowRight } from 'lucide-vue-next';

const isDismissed = ref(false);
const isMinimized = ref(false);
const projectsStore = useProjectsStore();

onMounted(() => {
  if (localStorage.getItem('compass_pilot_checklist_dismissed') === 'true') {
    isDismissed.value = true;
  }
});

const dismiss = () => {
  isDismissed.value = true;
  localStorage.setItem('compass_pilot_checklist_dismissed', 'true');
};

// Reatividade: Checa se já existe um projeto criado de verdade no banco
const hasProject = computed(() => projectsStore.catalog.length > 0);
// Simulação de Daily Review (Poderia vir da journalStore)
const hasDailyReview = computed(() => localStorage.getItem('compass_first_review_done') === 'true');

const allDone = computed(() => hasProject.value && hasDailyReview.value);
</script>

<template>
  <transition
    enter-active-class="transition duration-700 ease-out delay-1000"
    enter-from-class="opacity-0 translate-y-10"
    enter-to-class="opacity-100 translate-y-0"
    leave-active-class="transition duration-300 ease-in"
    leave-from-class="opacity-100 translate-y-0"
    leave-to-class="opacity-0 translate-y-10 scale-95"
  >
    <div 
      v-if="!isDismissed" 
      class="fixed bottom-12 right-6 z-40 w-80 bg-surface/95 backdrop-blur-md border shadow-2xl rounded-xl overflow-hidden font-sans select-none transition-all duration-300"
      :class="allDone ? 'border-status-success-border' : 'border-borderfocus'"
    >
      <!-- Header do Widget -->
      <div 
        class="px-4 py-3 border-b flex items-center justify-between cursor-pointer hover:bg-surface-hover transition-colors"
        :class="allDone ? 'border-status-success-border/50 bg-status-success-bg/10' : 'border-borderbase bg-app/50'"
        @click="isMinimized = !isMinimized"
      >
        <div class="flex items-center gap-2">
          <Rocket class="w-4 h-4" :class="allDone ? 'text-status-success-text' : 'text-content-accent'" />
          <span class="text-xs font-mono font-bold uppercase tracking-wider text-content">Jornada do Piloto</span>
        </div>
        <button @click.stop="dismiss" class="text-content-muted hover:text-content p-1">
          <X class="w-3.5 h-3.5" />
        </button>
      </div>

      <!-- Corpo Minimizado -->
      <div v-show="!isMinimized" class="p-5 space-y-4">
        <p class="text-xs text-content-muted leading-relaxed">
          O Laboratório foi apenas o começo. Complete estas missões para dominar a governança do seu banco de dados local.
        </p>
        
        <div class="space-y-3 font-mono text-xs">
          <!-- Item 1 (Já vem marcado) -->
          <div class="flex items-center gap-3 text-content-muted opacity-50 line-through">
            <CheckCircle2 class="w-4 h-4 text-status-success-text" />
            <span>Passar no Laboratório Interativo</span>
          </div>

          <!-- Item 2 (Reativo ao banco de Projetos) -->
          <div class="flex items-center gap-3 transition-colors" :class="hasProject ? 'text-content-muted opacity-50 line-through' : 'text-content'">
            <component :is="hasProject ? CheckCircle2 : Circle" class="w-4 h-4" :class="hasProject ? 'text-status-success-text' : 'text-content-accent'" />
            <span>Criar o 1º Projeto Real</span>
          </div>

          <!-- Item 3 (Daily Review) -->
          <div class="flex items-center gap-3 transition-colors" :class="hasDailyReview ? 'text-content-muted opacity-50 line-through' : 'text-content'">
            <component :is="hasDailyReview ? CheckCircle2 : Circle" class="w-4 h-4" :class="hasDailyReview ? 'text-status-success-text' : 'text-status-warning'" />
            <span>Encerrar o Turno (Shutdown)</span>
          </div>
        </div>

        <button 
          v-if="allDone"
          @click="dismiss"
          class="w-full mt-2 py-2 rounded bg-content text-content-invert text-xs font-bold uppercase flex items-center justify-center gap-2 shadow-md animate-fadeIn"
        >
          <span>Dispensar Painel</span> <ArrowRight class="w-3.5 h-3.5" />
        </button>
      </div>
    </div>
  </transition>
</template>