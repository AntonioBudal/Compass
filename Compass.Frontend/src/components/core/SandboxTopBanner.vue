<script setup lang="ts">
import { useOnboardingStore } from '@/stores/onboardingStore';
import { useToastStore } from '@/stores/toastStore';
import { useRouter } from 'vue-router';
import { Terminal, LogOut, ShieldAlert } from 'lucide-vue-next';

const onboardingStore = useOnboardingStore();
const toastStore = useToastStore();
const router = useRouter();

const handleExitSandbox = () => {
  onboardingStore.isSandboxActive = false;
  onboardingStore.commitments = [];
  sessionStorage.removeItem('compass_sandbox_mode');
  
  toastStore.showToast('Modo Sandbox desativado. Conectado ao banco PostgreSQL real.', 'neutral');
  router.push('/now');
  
  // Força reidratação limpa das stores originais
  setTimeout(() => {
    window.location.reload();
  }, 300);
};
</script>

<template>
  <Transition name="banner-slide">
    <div
      v-if="onboardingStore.isSandboxActive"
      class="w-full bg-status-warning-bg border-b border-status-warning-border text-status-warning px-4 py-1.5 flex items-center justify-between gap-4 text-xs font-mono select-none z-50 transition-all shadow-sm"
    >
      <div class="flex items-center gap-2.5 min-w-0">
        <span class="p-1 rounded bg-status-warning text-content-invert flex-shrink-0">
          <Terminal class="w-3 h-3 stroke-[2.5]" />
        </span>
        <span class="truncate font-semibold">
          ● MODO RAM SANDBOX ATIVO — Dados na memória temporária (Zero-DB Impact)
        </span>
      </div>

      <div class="flex items-center gap-3 flex-shrink-0">
        <span class="hidden md:inline text-[11px] opacity-80 font-sans">
          Nenhuma mutação será gravada no PostgreSQL.
        </span>
        <button
          @click="handleExitSandbox"
          class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded bg-surface hover:bg-surface-hover text-content border border-status-warning-border hover:border-borderfocus font-bold transition-all cursor-pointer shadow-sm"
          title="Sair do modo simulado e retornar aos dados reais"
        >
          <LogOut class="w-3 h-3" />
          <span>Sair para App Real</span>
        </button>
      </div>
    </div>
  </Transition>
</template>

<style scoped>
.banner-slide-enter-active,
.banner-slide-leave-active {
  transition: all 180ms cubic-bezier(0.16, 1, 0.3, 1);
}
.banner-slide-enter-from,
.banner-slide-leave-to {
  opacity: 0;
  transform: translateY(-100%);
  max-height: 0;
}
</style>