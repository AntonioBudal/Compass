<script setup lang="ts">
import { Clock, Folder } from 'lucide-vue-next';
import VisualCronEditor from './VisualCronEditor.vue';

//  CORREÇÃO (ARQ-013): Uso do defineModel para garantir Two-Way Binding
// Isso permite que o filho edite o rascunho sem quebrar a reatividade do UniversalEntityInspector
const draft = defineModel<any>('draft', { required: true });

const emit = defineEmits<{ (e: 'update'): void }>();

// Auxiliar para disparar o emit em eventos de tecla/mudança para o Auto-Save
const triggerSave = () => emit('update');
</script>

<template>
  <div class="space-y-6">
    
    <!-- 1. TÍTULO DO HÁBITO -->
    <div>
      <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Título do Hábito</label>
      <input 
        v-model="draft.title" 
        @input="triggerSave"
        type="text" 
        class="w-full px-4 py-2.5 bg-app border border-borderbase rounded-tactic text-sm text-content focus:border-borderfocus focus:outline-none transition-colors font-semibold"
      />
    </div>

    <!-- 2. TRADUTOR VISUAL DE RECORRÊNCIA (CRON) -->
    <div class="p-4 bg-surface-active border border-borderbase rounded-xl shadow-sm">
      <VisualCronEditor 
        v-model="draft.cronExpression" 
        @update:modelValue="triggerSave" 
      />
    </div>

    <!-- 3. METADADOS TÁTICOS (Duração e Energia) -->
    <div class="grid grid-cols-2 gap-4">
      <div>
        <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Duração (m)</label>
        <div class="relative flex items-center">
          <Clock class="w-4 h-4 text-content-muted absolute left-3 pointer-events-none" />
          <input 
            v-model.number="draft.estimatedDurationMinutes" 
            @input="triggerSave"
            type="number" step="5" min="5"
            class="w-full pl-9 pr-3 py-2 bg-app border border-borderbase rounded-tactic text-sm font-mono text-content focus:border-borderfocus focus:outline-none"
          />
        </div>
      </div>
      <div>
        <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Energia</label>
        <select 
          v-model.number="draft.energyRequired" 
          @change="triggerSave"
          class="w-full px-3 py-2 bg-app border border-borderbase rounded-tactic text-sm font-mono text-content focus:border-borderfocus focus:outline-none cursor-pointer"
        >
          <option :value="1">■□□ MAINT (1)</option>
          <option :value="2">■■□ OPER (2)</option>
          <option :value="3">■■■ DEEP (3)</option>
        </select>
      </div>
    </div>

    <!-- 4. PROJETO VINCULADO -->
    <div>
      <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Projeto Vinculado</label>
      <div class="relative flex items-center">
        <Folder class="w-4 h-4 text-content-muted absolute left-3 pointer-events-none" />
        <input 
          v-model="draft.projectName" 
          @input="triggerSave"
          type="text" placeholder="Sem projeto (avulso)"
          class="w-full pl-9 pr-3 py-2 bg-app border border-borderbase rounded-tactic text-sm text-content focus:border-borderfocus focus:outline-none"
        />
      </div>
    </div>

  </div>
</template>