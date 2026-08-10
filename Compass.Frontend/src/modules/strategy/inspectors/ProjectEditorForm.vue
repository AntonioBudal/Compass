<script setup lang="ts">
import { Folder, Target, AlignLeft } from 'lucide-vue-next';
import { useGoalsStore } from '@/modules/strategy/stores/goalsStore';

//  ARQ: Injeção do Catálogo de Metas para Vínculo Dinâmico
const goalsStore = useGoalsStore();

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
        <Target class="w-3 h-3" /> Bússola (Meta Estratégica Vinculada)
      </label>
      <div class="relative flex items-center">
        <!--  ARQ: Select relacional alimentado dinamicamente pela Store de Metas -->
        <select 
          v-model="draft.goalId" 
          @change="triggerSave"
          class="w-full pl-3 pr-8 py-2 bg-app border border-borderbase rounded-tactic text-sm text-content focus:border-borderfocus focus:outline-none transition-colors cursor-pointer appearance-none"
        >
          <option :value="null" class="italic">Nenhuma (Projeto sem destino estratégico)</option>
          <option v-for="goal in goalsStore.activeGoals" :key="goal.id" :value="goal.id">
            {{ goal.title }}
          </option>
        </select>
        <div class="absolute right-3 pointer-events-none text-[10px] text-content-muted font-mono">▼</div>
      </div>
    </div>
    
    <div>
      <label class="block text-[10px] font-mono uppercase text-content-muted mb-1.5 tracking-wider flex items-center gap-1.5">
        <AlignLeft class="w-3 h-3" /> Descrição / Escopo
      </label>
      <textarea 
        v-model="draft.description" 
        @input="triggerSave"
        rows="3"
        placeholder="Qual é o escopo tático ou a entrega esperada deste projeto?"
        class="w-full px-3 py-2 bg-app border border-borderbase rounded-tactic text-sm text-content focus:border-borderfocus focus:outline-none resize-none transition-colors"
      ></textarea>
    </div>

  </div>
</template>