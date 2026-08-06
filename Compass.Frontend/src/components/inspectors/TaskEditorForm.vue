<script setup lang="ts">
import { Clock, Folder } from 'lucide-vue-next';
import { useProjectsStore } from '@/stores/projectsStore';

//  ARQ: Injeção do Catálogo de Projetos para Vínculo Dinâmico
const projectsStore = useProjectsStore();

// Isso legaliza a mutação bidirecional (v-model) sem gerar warnings no Vue.
const draft = defineModel<any>('draft', { required: true });

// O @update avisa o Pai que ele deve iniciar o timer do Auto-Save
const emit = defineEmits<{ (e: 'update'): void }>();
const triggerSave = () => emit('update');
</script>

<template>
  <div class="space-y-5">
    <div>
      <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Título da Tarefa</label>
      <input 
        v-model="draft.title" 
        @input="triggerSave"
        type="text" 
        class="w-full px-4 py-2.5 bg-app border border-borderbase rounded-tactic text-sm text-content focus:border-borderfocus focus:outline-none transition-colors font-semibold"
      />
    </div>

    <div class="grid grid-cols-2 gap-4">
      <div>
        <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Duração (m)</label>
        <div class="relative flex items-center">
          <Clock class="w-4 h-4 text-content-muted absolute left-3" />
          <input 
            v-model.number="draft.estimatedDurationMinutes" 
            @input="triggerSave"
            type="number" step="15" 
            class="w-full pl-9 pr-3 py-2 bg-app border border-borderbase rounded-tactic text-sm font-mono text-content focus:border-borderfocus focus:outline-none"
          />
        </div>
      </div>
      <div>
        <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Energia</label>
        <select 
          v-model.number="draft.energyRequired" 
          @change="triggerSave"
          class="w-full px-3 py-2 bg-app border border-borderbase rounded-tactic text-sm font-mono text-content focus:border-borderfocus focus:outline-none cursor-pointer appearance-none"
        >
          <option :value="1">■□□ MAINT (1)</option>
          <option :value="2">■■□ OPER (2)</option>
          <option :value="3">■■■ DEEP (3)</option>
        </select>
      </div>
    </div>

    <div>
      <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider">Veículo (Projeto Vinculado)</label>
      <div class="relative flex items-center">
        <Folder class="w-4 h-4 text-content-muted absolute left-3 pointer-events-none" />
        
        <!--  ARQ: O Select que garante a Integridade Referencial no Frontend -->
        <select 
          v-model="draft.projectId" 
          @change="triggerSave"
          class="w-full pl-9 pr-8 py-2 bg-app border border-borderbase rounded-tactic text-sm text-content focus:border-borderfocus focus:outline-none transition-colors cursor-pointer appearance-none"
        >
          <option :value="null" class="italic">Nenhum (Tarefa Avulsa)</option>
          <option v-for="proj in projectsStore.lruProjects" :key="proj.id" :value="proj.id">
            {{ proj.name }}
          </option>
        </select>
        
        <div class="absolute right-3 pointer-events-none text-[10px] text-content-muted font-mono">▼</div>
      </div>
    </div>
  </div>
</template>