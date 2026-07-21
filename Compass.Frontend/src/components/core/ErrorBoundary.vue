<script setup lang="ts">
import { ref, onErrorCaptured } from 'vue';
import { ShieldAlert, RefreshCw, Terminal } from 'lucide-vue-next';

const hasError = ref(false);
const errorDetails = ref<Error | null>(null);

onErrorCaptured((err, instance, info) => {
  hasError.value = true;
  errorDetails.value = err as Error;

  // Em modo DEV, o log já será feito pelo Global Error Handler,
  // mas aqui nós paramos a propagação para evitar a "Tela Branca" no navegador.
  return false; 
});

const reloadView = () => {
  hasError.value = false;
  errorDetails.value = null;
  window.location.reload();
};
</script>

<template>
  <div v-if="hasError" class="h-full w-full flex items-center justify-center p-6 select-none">
    <div class="max-w-md w-full p-6 rounded-xl border border-rose-500/30 bg-rose-950/10 shadow-2xl space-y-6">
      
      <div class="flex items-center gap-3 text-rose-500 pb-4 border-b border-rose-500/20">
        <ShieldAlert class="w-6 h-6 flex-shrink-0" />
        <h2 class="text-base font-semibold tracking-tight">Falha de Renderização de Tela</h2>
      </div>

      <div class="space-y-2">
        <p class="text-sm text-zinc-300">
          A interface de usuário encontrou uma anomalia em tempo de execução e foi suspensa por segurança. Seus dados não salvos em outras telas não foram afetados.
        </p>
        
        <div class="p-3 mt-4 rounded bg-zinc-950 border border-zinc-800 font-mono text-xs overflow-x-auto">
          <span class="text-rose-400 font-bold block mb-1">Exception:</span>
          <span class="text-zinc-400">{{ errorDetails?.message || 'Unknown Error' }}</span>
        </div>
      </div>

      <div class="pt-4 flex items-center justify-between gap-4">
        <span class="text-[10px] font-mono text-zinc-500 flex items-center gap-1.5">
          <Terminal class="w-3 h-3" /> Verifique o Dev Console
        </span>
        
        <button 
          @click="reloadView"
          class="inline-flex items-center justify-center gap-2 px-4 py-2 bg-zinc-100 hover:bg-white text-zinc-950 text-xs font-semibold rounded shadow-sm transition-all cursor-pointer"
        >
          <RefreshCw class="w-3.5 h-3.5" />
          <span>Recarregar Tela</span>
        </button>
      </div>

    </div>
  </div>

  <slot v-else />
</template>