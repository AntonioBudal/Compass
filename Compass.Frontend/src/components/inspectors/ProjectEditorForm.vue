<script setup lang="ts">
import { Calendar, Folder, Target } from 'lucide-vue-next';

// Contrato Universal de Auto-Save
const draft = defineModel<any>('draft', { required: true });
const emit = defineEmits<{ (e: 'update'): void }>();

const triggerSave = () => emit('update');
</script>

<template>
  <div class="space-y-6">
    
    <div>
      <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Nome do Projeto</label>
      <div class="relative flex items-center">
        <Folder class="w-4 h-4 text-content-muted absolute left-3 pointer-events-none" />
        <input 
          v-model="draft.name" 
          @input="triggerSave"
          type="text" 
          class="w-full pl-9 pr-4 py-2.5 bg-app border border-borderbase rounded-tactic text-sm text-content focus:border-borderfocus focus:outline-none transition-colors font-semibold"
        />
      </div>
    </div>

    <div>
      <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider flex items-center gap-1.5">
        <Target class="w-3 h-3" /> Meta Estratégica Vinculada
      </label>
      <input 
        v-model="draft.linkedGoal" 
        @input="triggerSave"
        type="text" 
        placeholder="ID ou Nome da Meta..."
        class="w-full px-4 py-2 bg-app border border-borderbase rounded-tactic text-sm text-content focus:border-borderfocus focus:outline-none transition-colors"
      />
    </div>

    <div>
      <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Prazo Final (Deadline)</label>
      <div class="relative flex items-center">
        <Calendar class="w-4 h-4 text-content-muted absolute left-3 pointer-events-none" />
        <input 
          v-model="draft.deadline" 
          @input="triggerSave"
          type="date" 
          class="w-full pl-9 pr-3 py-2 bg-app border border-borderbase rounded-tactic text-sm font-mono text-content focus:border-borderfocus focus:outline-none"
        />
      </div>
    </div>

  </div>
</template>