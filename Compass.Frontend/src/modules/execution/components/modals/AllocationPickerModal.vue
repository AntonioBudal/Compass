<script setup lang="ts">
import { ref, computed, watch, nextTick } from 'vue';
import { useCommitmentsStore, type CommitmentItem } from '@/modules/tactical/stores/commitmentsStore';
import { useFocusTrap } from '@/shared/composables/useFocusTrap';
import { useToastStore } from '@/shared/stores/toastStore'; //  ARQ-00: Injetado Toast para feedback
import { 
  Search, Calendar, Clock, Folder, 
  CircleDashed, RefreshCw, ArrowRight, CalendarPlus 
} from 'lucide-vue-next';

const props = defineProps<{ isOpen: boolean }>();
const emit = defineEmits<{ (e: 'close'): void }>();

const commitmentsStore = useCommitmentsStore();
const toastStore = useToastStore();

const searchQuery = ref('');
const selectedIndex = ref(0);
const inputRef = ref<HTMLInputElement | null>(null);
const modalRef = ref<HTMLElement | null>(null);

// Trava de foco para acessibilidade
useFocusTrap(modalRef, computed(() => props.isOpen));

// Limpa a busca e foca no input sempre que o modal abre
watch(() => props.isOpen, async (isOpen) => {
  if (isOpen) {
    searchQuery.value = '';
    selectedIndex.value = 0;
    await nextTick();
    inputRef.value?.focus();
  }
});

// A Mágica do Filtro Especialista: 
// Traz apenas Tarefas e Hábitos pendentes. Prioriza os que AINDA NÃO têm horário.
const allocatableItems = computed(() => {
  let list = commitmentsStore.items.filter(i => 
    (i.type === 'TASK' || i.type === 'HABIT') && 
    (i.status === 'PENDING' || i.status === 'IN_PROGRESS')
  );

  if (searchQuery.value.trim()) {
    const q = searchQuery.value.toLowerCase().trim();
    list = list.filter(i => 
      i.title.toLowerCase().includes(q) || 
      (i.projectName && i.projectName.toLowerCase().includes(q))
    );
  }

  // Ordenação: 1º Sem horário (Órfãos), 2º Com horário (Para reagendamento)
  return list.sort((a, b) => {
    if (!a.startTime && b.startTime) return -1;
    if (a.startTime && !b.startTime) return 1;
    return 0;
  });
});

watch(searchQuery, () => {
  selectedIndex.value = 0;
});

//  CORREÇÃO DEFINITIVA: Auto-Alocação Direta na Agenda (Enviando o DTO completo)
const selectItem = async (item: CommitmentItem) => {
  emit('close'); // Oculta o modal instantaneamente para UX fluida
  
  // 1. Calcula o horário atual arredondado para o próximo bloco de 5 minutos
  const now = new Date();
  const coeff = 1000 * 60 * 5; 
  const roundedStart = new Date(Math.ceil(now.getTime() / coeff) * coeff);

  const durationMin = item.estimatedDurationMinutes || 30;
  
  // 2. Calcula o fim baseado na duração estimada (padrão 30m se não tiver)
  const safeDuration = Math.max(5, durationMin); 
    const durationMs = safeDuration * 60000;
    const roundedEnd = new Date(roundedStart.getTime() + durationMs);

  try {
    // 3. Monta o DTO completo para agradar o PUT do .NET
    const fullPayload = {
      ...item, // Copia todos os dados originais (title, type, energy, etc)
      estimatedDurationMinutes: safeDuration, 
        energyRequired: item.energyRequired || 2, // Se energia for 0, vira 2
        startTime: roundedStart.toISOString(),
        endTime: roundedEnd.toISOString()
    };

    // 4. Dispara a atualização
    await commitmentsStore.updateCommitment(item.id, fullPayload, true); 

    toastStore.showToast(`Alocado para as ${roundedStart.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`, 'success');
  } catch (error) {
    console.error('[AllocationModal] Falha ao alocar', error);
    toastStore.showToast('Falha ao alocar tarefa na agenda.', 'error');
  }
};

const handleKeyDown = (e: KeyboardEvent) => {
  const max = allocatableItems.value.length;
  if (e.key === 'Escape') {
    e.preventDefault();
    emit('close');
    return;
  }
  
  if (max === 0) return;

  if (e.key === 'ArrowDown') {
    e.preventDefault();
    selectedIndex.value = (selectedIndex.value + 1) % max;
  } else if (e.key === 'ArrowUp') {
    e.preventDefault();
    selectedIndex.value = (selectedIndex.value - 1 + max) % max;
  } else if (e.key === 'Enter') {
    e.preventDefault();
    const selected = allocatableItems.value[selectedIndex.value];
    if (selected) selectItem(selected);
  }
};
</script>

<template>
  <transition name="modal-snap">
    <!-- Backdrop Oculto -->
    <div
      v-if="isOpen"
      class="fixed inset-0 z-[100] flex items-start justify-center pt-[15vh] p-4 bg-app/80 backdrop-blur-sm select-none"
      @click="emit('close')"
    >
      <div
        ref="modalRef"
        role="dialog"
        aria-modal="true"
        aria-label="Alocar Tarefa na Agenda"
        class="max-w-2xl w-full rounded-xl bg-surface border border-borderhighlight shadow-2xl overflow-hidden gpu-accelerated relative flex flex-col max-h-[70vh]"
        @click.stop
        @keydown="handleKeyDown"
      >
        <!-- Cabeçalho Informativo -->
        <div class="px-5 py-3.5 bg-surface border-b border-borderbase flex items-center justify-between">
          <div class="flex items-center gap-3">
            <div class="w-8 h-8 rounded-full bg-content text-content-invert flex items-center justify-center">
              <CalendarPlus class="w-4 h-4" />
            </div>
            <div>
              <h2 class="text-sm font-bold text-content leading-tight">Alocar no Turno</h2>
              <p class="text-[11px] font-mono text-content-muted mt-0.5">Selecione uma tarefa pendente para definir seu horário de início.</p>
            </div>
          </div>
          <button 
            @click="emit('close')"
            class="px-1.5 py-0.5 text-[10px] font-mono bg-app text-content-muted rounded border border-borderbase hover:text-content hover:border-borderfocus cursor-pointer transition-colors"
          >
            ESC
          </button>
        </div>

        <!-- Barra de Busca -->
        <div class="relative flex items-center px-4 border-b border-borderbase bg-app/50">
          <Search class="w-4 h-4 text-content-muted flex-shrink-0" />
          <input 
            ref="inputRef"
            v-model="searchQuery"
            type="text" 
            placeholder="Buscar no backlog..." 
            class="w-full py-3 px-3 bg-transparent text-sm text-content placeholder:text-content-muted focus:outline-none font-sans font-medium"
          />
        </div>

        <!-- Lista de Backlog -->
        <div class="flex-1 overflow-y-auto p-2 space-y-1">
          <div v-if="allocatableItems.length === 0" class="py-12 text-center text-xs font-mono text-content-muted flex flex-col items-center gap-2">
            <CircleDashed class="w-6 h-6 text-content-muted opacity-50" />
            <span>Nenhuma tarefa ou hábito pendente encontrado no backlog.</span>
          </div>

          <button 
            v-for="(item, idx) in allocatableItems" 
            :key="item.id"
            @click="selectItem(item)"
            @mouseenter="selectedIndex = idx"
            class="w-full flex items-center justify-between gap-3 px-3 py-2.5 rounded-lg text-left transition-colors cursor-pointer group"
            :class="selectedIndex === idx ? 'bg-surface-active border-l-2 border-content' : 'border-l-2 border-transparent hover:bg-surface-hover'"
          >
            <!-- Ícone e Título -->
            <div class="flex items-center gap-3 min-w-0 flex-1">
              <CircleDashed v-if="item.type === 'TASK'" class="w-4 h-4 flex-shrink-0" :class="selectedIndex === idx ? 'text-content' : 'text-content-muted'" />
              <RefreshCw v-else-if="item.type === 'HABIT'" class="w-4 h-4 flex-shrink-0" :class="selectedIndex === idx ? 'text-content' : 'text-content-muted'" />
              
              <div class="truncate flex-1 min-w-0">
                <span class="text-sm font-medium text-content block truncate" :class="{'text-content-accent': selectedIndex === idx}">{{ item.title }}</span>
                <div class="flex items-center gap-3 mt-1">
                  <!-- Badges Informativas -->
                  <span v-if="!item.startTime" class="text-[9px] font-bold font-mono uppercase px-1.5 py-[1px] rounded bg-status-warning-bg text-status-warning border border-status-warning-border">
                    Não Agendado
                  </span>
                  <span v-else class="text-[9px] font-mono uppercase flex items-center gap-1 text-content-muted">
                    <Calendar class="w-3 h-3" />
                    {{ new Date(item.startTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}
                  </span>
                  
                  <span v-if="item.projectName" class="text-[10px] text-content-muted font-mono flex items-center gap-1 truncate">
                    <Folder class="w-3 h-3" /> {{ item.projectName }}
                  </span>
                </div>
              </div>
            </div>

            <!-- Flecha de Ação -->
            <div class="flex items-center gap-3 flex-shrink-0 text-content-muted">
              <span class="text-[10px] font-mono flex items-center gap-1">
                <Clock class="w-3 h-3" /> {{ item.estimatedDurationMinutes }}m
              </span>
              <ArrowRight v-if="selectedIndex === idx" class="w-4 h-4 text-content ml-2" />
            </div>
          </button>
        </div>

        <!-- Rodapé Monocromático -->
        <div class="px-4 py-2 bg-app border-t border-borderbase flex items-center justify-between text-[11px] font-mono text-content-muted">
          <div class="flex items-center gap-3">
            <span><kbd class="bg-surface border border-borderbase px-1 rounded text-content-muted">↑↓</kbd> Navegar</span>
            <span><kbd class="bg-surface border border-borderbase px-1 rounded text-content-muted">Enter</kbd> Alocar Agora</span>
          </div>
          <span>Compass Timeblocking</span>
        </div>

      </div>
    </div>
  </transition>
</template>

<style scoped>
.modal-snap-enter-active,
.modal-snap-leave-active {
  transition: opacity 150ms cubic-bezier(0.16, 1, 0.3, 1), transform 150ms cubic-bezier(0.16, 1, 0.3, 1);
}
.modal-snap-enter-from,
.modal-snap-leave-to {
  opacity: 0;
  transform: scale(0.96);
}
</style>