<script setup lang="ts">
import { ref, watch, nextTick, computed } from 'vue';
import { omniEngine } from '@/utils/autocomplete/AutocompleteEngine';
import type { Suggestion, GhostPrediction } from '@/utils/autocomplete/types';
import { 
  Folder, Clock, Zap, Calendar, Terminal, History, Command, CornerDownLeft
} from 'lucide-vue-next';

const props = defineProps<{
  modelValue: string;
  placeholder?: string;
  autofocus?: boolean;
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void;
  (e: 'submit', value: string): void;
  (e: 'escape'): void;
}>();

const inputRef = ref<HTMLInputElement | null>(null);
const internalValue = ref(props.modelValue);
const cursorPos = ref(0);

// Estados Isolados do Duplo Pipeline
const suggestions = ref<Suggestion[]>([]);
const activePrediction = ref<GhostPrediction | null>(null);
const selectedIndex = ref(0);
const isDropdownOpen = ref(false);

watch(() => props.modelValue, (newVal) => {
  if (newVal !== internalValue.value) internalValue.value = newVal;
});

const updateEngine = () => {
  if (!inputRef.value) {
    console.warn('DIAGNÓSTICO: inputRef está nulo!');
    return;
  }
  
  cursorPos.value = inputRef.value.selectionStart || internalValue.value.length;
  
  console.log('--- DIAGNÓSTICO ETAPA 1 ---');
  console.log('1. Texto Atual:', internalValue.value);

  // Usa const local em vez de .value (mantém o Vue limpo)
  const activeContext = omniEngine.resolveActiveQuery(internalValue.value, cursorPos.value);
  console.log('2. Contexto Resolvido:', activeContext);
  
  if (activeContext.isActive) {
    suggestions.value = omniEngine.getSuggestions(internalValue.value, cursorPos.value);
    console.log('3. Sugestões Retornadas:', suggestions.value);
    
    isDropdownOpen.value = suggestions.value.length > 0;
    selectedIndex.value = 0;
  } else {
    isDropdownOpen.value = false;
    suggestions.value = [];
  }

  activePrediction.value = omniEngine.getGhostPrediction(internalValue.value, cursorPos.value);
  console.log('4. Ghost Prediction:', activePrediction.value);
};

const handleInput = (e: Event) => {
  internalValue.value = (e.target as HTMLInputElement).value;
  emit('update:modelValue', internalValue.value);
  updateEngine();
};

const handleKeyupAndClick = () => {
  updateEngine();
};

// --- APLICAÇÃO DE MUTAÇÃO (Delega matemática para a Engine) ---
const commitSuggestion = (suggestion: Suggestion) => {
  const result = omniEngine.applySuggestion(internalValue.value, cursorPos.value, suggestion);
  
  internalValue.value = result.newText;
  emit('update:modelValue', result.newText);
  isDropdownOpen.value = false;
  activePrediction.value = null;
  
  nextTick(() => {
    if (inputRef.value) {
      inputRef.value.setSelectionRange(result.newCursorPosition, result.newCursorPosition);
      inputRef.value.focus();
    }
  });
};

const handleKeyDown = (e: KeyboardEvent) => {
  const max = suggestions.value.length;

  // Interceptador Prioritário: Predição Inline (Tab / Seta para a Direita)
  if (e.key === 'Tab' || e.key === 'ArrowRight') {
    // Só acata o Ghost Text se o usuário não tiver navegado no dropdown
    if (activePrediction.value && (!isDropdownOpen.value || selectedIndex.value === 0)) {
      e.preventDefault();
      commitSuggestion(activePrediction.value.suggestion);
      return;
    }
  }

  if (!isDropdownOpen.value) {
    if (e.key === 'Enter') {
      e.preventDefault();
      emit('submit', internalValue.value);
    }
    if (e.key === 'Escape') emit('escape');
    return;
  }

  switch (e.key) {
    case 'ArrowDown':
      e.preventDefault();
      selectedIndex.value = (selectedIndex.value + 1) % max;
      break;
    case 'ArrowUp':
      e.preventDefault();
      selectedIndex.value = (selectedIndex.value - 1 + max) % max;
      break;
    case 'Tab':
    case 'Enter':
      e.preventDefault();
      if (max > 0) commitSuggestion(suggestions.value[selectedIndex.value]);
      break;
    case 'Escape':
      e.preventDefault();
      isDropdownOpen.value = false;
      break;
  }
};

// Visual passivo do Ghost Text
const textBeforeCursor = computed(() => internalValue.value.slice(0, cursorPos.value));
// Oculta o Ghost Text se o usuário começou a descer pelas setas no dropdown
const ghostTextTail = computed(() => (selectedIndex.value === 0 && activePrediction.value) ? activePrediction.value.ghostSuffix : '');

const getIconForType = (type: string) => {
  switch (type) {
    case 'project': return Folder;
    case 'goal': return Zap;
    case 'history': return History;
    case 'time': return Clock;
    case 'date': return Calendar;
    case 'type': return Terminal;
    default: return Command;
  }
};

watch(() => props.autofocus, (val) => {
  if (val) nextTick(() => inputRef.value?.focus());
}, { immediate: true });
</script>

<template>
  <div class="relative w-full text-base font-sans select-none">
    
    <!-- 1. CAMADA DE GHOST TEXT (Magia Preditiva Passiva) -->
    <div 
      v-if="ghostTextTail" 
      class="absolute inset-0 px-4 py-3 pointer-events-none flex whitespace-pre overflow-hidden"
    >
      <span class="opacity-0">{{ textBeforeCursor }}</span>
      <span class="text-content-muted/50">{{ ghostTextTail }}</span>
    </div>

    <!-- 2. CAMADA DO INPUT REAL -->
    <input
      ref="inputRef"
      :value="internalValue"
      type="text"
      :placeholder="placeholder"
      spellcheck="false"
      autocomplete="off"
      @input="handleInput"
      @keyup="handleKeyupAndClick"
      @click="handleKeyupAndClick"
      @keydown="handleKeyDown"
      class="w-full bg-transparent text-content focus:outline-none px-4 py-3 border border-borderfocus rounded-xl relative z-10 shadow-inner"
    />

    <!-- 3. DROPDOWN DE SUGESTÕES -->
    <Transition name="dropdown-fade">
      <ul 
        v-if="isDropdownOpen && suggestions.length > 0" 
        class="absolute top-full left-0 right-0 mt-2 bg-surface border border-borderbase rounded-xl shadow-2xl z-50 max-h-[320px] overflow-y-auto overflow-x-hidden divide-y divide-borderbase/50"
      >
        <li 
          v-for="(sug, i) in suggestions" 
          :key="i"
          @mousedown.prevent="commitSuggestion(sug)"
          @mouseenter="selectedIndex = i"
          class="px-4 py-2.5 flex items-center justify-between gap-3 cursor-pointer transition-colors"
          :class="i === selectedIndex ? 'bg-surface-hover' : ''"
        >
          <div class="flex items-center gap-3 min-w-0">
            <component :is="getIconForType(sug.type)" class="w-4 h-4 flex-shrink-0 text-content-muted" />
            <span class="truncate text-sm font-medium text-content" v-html="sug.htmlHighlight || sug.label"></span>
          </div>

          <div class="flex items-center gap-2 flex-shrink-0">
            <!-- Hint visual inteligente: Tab se for o 1º item e houver Ghost Text, Enter para os demais -->
            <kbd v-if="i === selectedIndex" class="hidden sm:inline-block px-1.5 py-0.5 rounded bg-surface-active border border-borderbase font-mono text-[10px] text-content-muted uppercase tracking-wider">
              {{ (i === 0 && ghostTextTail) ? 'Tab' : 'Enter' }}
            </kbd>
            <CornerDownLeft v-if="i === selectedIndex" class="w-3 h-3 text-content-muted" />
          </div>
        </li>
      </ul>
    </Transition>
  </div>
</template>

<style scoped>
:deep(b) {
  font-weight: 800;
  color: var(--color-content-accent);
}

.dropdown-fade-enter-active, .dropdown-fade-leave-active {
  transition: opacity 100ms ease, transform 100ms ease;
}
.dropdown-fade-enter-from, .dropdown-fade-leave-to {
  opacity: 0;
  transform: translateY(-4px);
}
</style>