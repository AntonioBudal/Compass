<script setup lang="ts">
import { Calendar, Target } from 'lucide-vue-next';

//  CORREÇÃO (ARQ-013): Substituído defineProps por defineModel para manter o padrão universal
const draft = defineModel<any>('draft', { required: true });
const emit = defineEmits<{ (e: 'update'): void }>();

// Auxiliar para disparar o emit em eventos de tecla/mudança
const triggerSave = () => emit('update');
</script>

<template>
  <div class="space-y-6">
    
    <!-- 1. TÍTULO DA META -->
    <div>
      <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Nome da Meta Estratégica</label>
      <input 
        v-model="draft.title" 
        @input="triggerSave"
        type="text" 
        placeholder="Ex: Lançar versão 2.0 do Produto"
        class="w-full px-4 py-2.5 bg-app border border-borderbase rounded-tactic text-sm text-content focus:border-borderfocus focus:outline-none transition-colors font-semibold"
      />
    </div>

    <!-- 2. PRAZO ALVO (DEADLINE) -->
    <div>
      <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Prazo Alvo (Target Date)</label>
      <div class="relative flex items-center">
        <Calendar class="w-4 h-4 text-content-muted absolute left-3 pointer-events-none" />
        <input 
          v-model="draft.targetDate" 
          @input="triggerSave"
          type="date" 
          class="w-full pl-9 pr-3 py-2 bg-app border border-borderbase rounded-tactic text-sm font-mono text-content focus:border-borderfocus focus:outline-none"
        />
      </div>
    </div>

    <!-- 3. PROPÓSITO ESTRATÉGICO (WHY) -->
    <div>
      <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider flex items-center gap-1.5">
        <Target class="w-3 h-3" /> Propósito Estratégico (O "Porquê")
      </label>
      <textarea 
        v-model="draft.why" 
        @input="triggerSave"
        rows="4"
        placeholder="Por que esta meta é crucial? Qual o impacto esperado ao atingi-la?"
        class="w-full px-4 py-3 bg-app border border-borderbase rounded-tactic text-sm text-content focus:border-borderfocus focus:outline-none transition-colors resize-none leading-relaxed"
      ></textarea>
    </div>

  </div>
</template>