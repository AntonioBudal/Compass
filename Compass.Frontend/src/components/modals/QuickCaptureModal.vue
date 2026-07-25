<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useCommitmentsStore } from '@/stores/commitmentsStore';
import { useProjectsStore } from '@/stores/projectsStore';
import { useToastStore } from '@/stores/toastStore';
import { parseQuickCapture } from '@/utils/nlpParser';
import { TrieIndex } from '@/utils/trieIndex';
import { useKeyboardNavigation } from '@/composables/useKeyboardNavigation';
import AutoCompleteDropdown, { type DropdownItem } from '@/components/core/AutoCompleteDropdown.vue';
import { Terminal, CornerDownLeft, Clock, Zap, Calendar, Folder } from 'lucide-vue-next';

const props = defineProps<{ isOpen: boolean }>();
const emit = defineEmits<{ (e: 'close'): void }>();

const commitmentsStore = useCommitmentsStore();
const projectsStore = useProjectsStore();
const toastStore = useToastStore();

const inputRef = ref<HTMLInputElement | null>(null);
const rawInput = ref('');
const isSubmitting = ref(false);

// --- MOTOR DE AUTO-COMPLETE EM RAM ---
const projectsTrie = new TrieIndex();
const activeDropdown = ref<'PROJECT' | 'TYPE' | 'TIME' | 'DATE' | null>(null);
const dropdownQuery = ref('');

const typeSuggestions: DropdownItem[] = [
  { label: '/t — Tarefa Operacional', value: '/t' },
  { label: '/h — Hábito ou Rotina', value: '/h' },
  { label: '/e — Evento ou Reunião', value: '/e' },
  { label: '/n — Nota / Captura Rápida', value: '/n' }
];

const timeSuggestions: DropdownItem[] = [
  { label: '@15m — Sprint Curta (15 min)', value: '@15m' },
  { label: '@30m — Turno Padrão (30 min)', value: '@30m' },
  { label: '@45m — Foco Intenso (45 min)', value: '@45m' },
  { label: '@1h — Bloco Profundo (60 min)', value: '@1h' },
  { label: '@2h — Imersão Total (120 min)', value: '@2h' }
];

const dateSuggestions: DropdownItem[] = [
  { label: '^hoje — Limite às 23:59 de hoje', value: '^hoje' },
  { label: '^amanha — Limite às 23:59 de amanhã', value: '^amanha' },
  { label: '^seg — Próxima Segunda-feira', value: '^seg' },
  { label: '^sex — Próxima Sexta-feira', value: '^sex' }
];

watch(() => projectsStore.catalog, (newCatalog) => {
  projectsTrie.clear();
  newCatalog.forEach(p => {
    projectsTrie.insertMultiWord(p.name, { id: p.id, title: p.name, lastUsedAtUtc: p.lastUsedAtUtc });
  });
}, { immediate: true });

watch(rawInput, (val) => {
  if (!inputRef.value) return;
  const cursor = inputRef.value.selectionStart || val.length;
  const textBeforeCursor = val.slice(0, cursor);
  const match = textBeforeCursor.match(/([#\/^@])([a-zA-Z0-9_-]*)$/);

  if (match) {
    const trigger = match[1];
    dropdownQuery.value = match[2];

    if (trigger === '#') activeDropdown.value = 'PROJECT';
    else if (trigger === '/') activeDropdown.value = 'TYPE';
    else if (trigger === '@') activeDropdown.value = 'TIME';
    else if (trigger === '^') activeDropdown.value = 'DATE';
  } else {
    activeDropdown.value = null;
  }
});

const currentSuggestions = computed<DropdownItem[]>(() => {
  if (!activeDropdown.value) return [];

  if (activeDropdown.value === 'PROJECT') {
    if (!dropdownQuery.value) {
      return projectsStore.lruProjects.slice(0, 6).map(p => ({
        label: `#${p.name}`, value: `#${p.name}`, id: p.id
      }));
    }
    const results = projectsTrie.searchPrefix(dropdownQuery.value, 6);
    return results.map(r => ({ label: `#${r.title}`, value: `#${r.title}`, id: r.id }));
  }
  if (activeDropdown.value === 'TYPE') return typeSuggestions.filter(s => s.value.includes(dropdownQuery.value.toLowerCase()));
  if (activeDropdown.value === 'TIME') return timeSuggestions.filter(s => s.value.includes(dropdownQuery.value.toLowerCase()));
  if (activeDropdown.value === 'DATE') return dateSuggestions.filter(s => s.value.includes(dropdownQuery.value.toLowerCase()));

  return [];
});

const suggestionsCount = computed(() => currentSuggestions.value.length);

// Seleção de um item na interface
const selectSuggestion = (item: DropdownItem) => {
  if (!inputRef.value) return;
  const cursor = inputRef.value.selectionStart || rawInput.value.length;
  const textBefore = rawInput.value.slice(0, cursor);
  const textAfter = rawInput.value.slice(cursor);

  const newTextBefore = textBefore.replace(/([#\/^@])([a-zA-Z0-9_-]*)$/, item.value + ' ');
  rawInput.value = newTextBefore + textAfter;
  activeDropdown.value = null;

  if (item.id) projectsStore.promoteUsage(item.id);
  setTimeout(() => inputRef.value?.focus(), 10);
};

// --- MÁQUINA DE ESTADOS DO TECLADO ---
const { selectedIndex, handleKeyDown } = useKeyboardNavigation(suggestionsCount, {
  onSelect: (index) => {
    const selected = currentSuggestions.value[index];
    if (selected) selectSuggestion(selected);
  },
  onDismiss: () => {
    activeDropdown.value = null;
  },
  onSubmitFallback: () => {
    handleSubmit();
  }
});

const onInputKeyDown = (e: KeyboardEvent) => {
  handleKeyDown(e, Boolean(activeDropdown.value && suggestionsCount.value > 0));
};

const livePreview = computed(() => parseQuickCapture(rawInput.value));

const handleSubmit = async () => {
  if (!rawInput.value.trim() || isSubmitting.value) return;
  const parsed = livePreview.value;
  isSubmitting.value = true;

  try {
    let matchedProjectId: string | null = null;
    if (parsed.projectQuery) {
      const match = projectsStore.catalog.find(p => p.name.toLowerCase() === parsed.projectQuery?.toLowerCase());
      if (match) {
        matchedProjectId = match.id;
        projectsStore.promoteUsage(match.id);
      }
    }

    await commitmentsStore.createCommitment({
      title: parsed.title,
      type: parsed.type,
      estimatedDurationMinutes: parsed.estimatedDurationMinutes,
      energyRequired: parsed.energyRequired,
      deadline: parsed.deadlineIso,
      projectId: matchedProjectId
    });

    toastStore.showToast(`[${parsed.type}] capturado com sucesso!`, 'success');
    rawInput.value = '';
    emit('close');
  } catch (err) {
    toastStore.showToast('Erro ao processar captura rápida.', 'error');
  } finally {
    isSubmitting.value = false;
  }
};

watch(() => props.isOpen, (open) => {
  if (open) setTimeout(() => inputRef.value?.focus(), 50);
  else { rawInput.value = ''; activeDropdown.value = null; }
});
</script>

<template>
  <Teleport to="body">
    <Transition name="modal-fade">
      <div v-if="isOpen" class="fixed inset-0 z-50 flex items-start justify-center pt-[15vh] px-4 bg-app/80 backdrop-blur-sm select-none" @click.self="emit('close')" role="dialog" aria-modal="true">
        <div class="w-full max-w-2xl bg-surface border border-borderfocus rounded-xl shadow-2xl font-mono flex flex-col relative animate-scale-in">
          
          <div class="flex items-center justify-between px-4 py-2.5 bg-app border-b border-borderbase text-xs text-content-muted rounded-t-xl">
            <span class="flex items-center gap-2 font-bold uppercase tracking-wider text-content">
              <Terminal class="w-4 h-4 text-content-muted" />
              <span>Quick Capture CLI v2.0</span>
            </span>
            <span class="text-[11px] flex items-center gap-3">
              <span>Gatilhos: <strong class="text-content">#</strong>proj <strong class="text-content">@</strong>tempo <strong class="text-content">!</strong>ene <strong class="text-content">^</strong>data <strong class="text-content">/</strong>tipo</span>
              <kbd class="px-1.5 py-0.5 rounded bg-surface border border-borderbase font-sans text-[10px]">ESC para fechar</kbd>
            </span>
          </div>

          <div class="p-4 relative">
            <input
              ref="inputRef"
              v-model="rawInput"
              type="text"
              placeholder="Digite sua tarefa... (ex: Revisar PR #core @45m !3 ^amanha /t)"
              class="w-full bg-transparent text-lg font-sans text-content placeholder-content-muted/50 focus:outline-none"
              @keydown="onInputKeyDown"
            />

            <!-- O Dropdown agora flutua sem recortes sobre o rodapé e o backdrop -->
            <AutoCompleteDropdown
              :items="currentSuggestions"
              :selected-index="selectedIndex"
              :trigger-type="activeDropdown"
              @select="selectSuggestion"
            />
          </div>

          <div class="px-4 py-3 bg-app/50 border-t border-borderbase flex flex-wrap items-center justify-between gap-3 text-xs rounded-b-xl">
            <div class="flex items-center gap-3">
              <span class="px-2 py-0.5 rounded bg-surface border border-borderbase font-bold text-content uppercase tracking-wider text-[11px]">
                [{{ livePreview.type }}]
              </span>
              <span class="flex items-center gap-1 text-content-muted" title="Duração estimada">
                <Clock class="w-3.5 h-3.5 text-content" />
                <strong class="text-content font-sans">{{ livePreview.estimatedDurationMinutes }}m</strong>
              </span>
              <span class="flex items-center gap-1 text-content-muted" title="Nível de energia requerida">
                <Zap class="w-3.5 h-3.5 text-content" />
                <strong class="text-content font-sans">!{{ livePreview.energyRequired }}</strong>
              </span>
              <span v-if="livePreview.projectQuery" class="flex items-center gap-1 text-content-accent font-semibold" title="Projeto vinculado">
                <Folder class="w-3.5 h-3.5" />
                <span>#{{ livePreview.projectQuery }}</span>
              </span>
              <span v-if="livePreview.deadlineIso" class="flex items-center gap-1 text-content-muted" title="Limite temporal (23:59 local)">
                <Calendar class="w-3.5 h-3.5 text-content" />
                <span>{{ new Date(livePreview.deadlineIso).toLocaleDateString() }}</span>
              </span>
            </div>

            <button
              @click="handleSubmit"
              :disabled="!rawInput.trim() || isSubmitting"
              class="px-3.5 py-1.5 rounded-tactic bg-content hover:opacity-90 disabled:opacity-40 text-content-invert font-semibold flex items-center gap-1.5 shadow-sm transition-all cursor-pointer"
            >
              <span>Capturar</span>
              <CornerDownLeft class="w-3.5 h-3.5" />
            </button>
          </div>

        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 120ms ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }
.animate-scale-in { animation: scaleIn 140ms cubic-bezier(0.16, 1, 0.3, 1) forwards; }
@keyframes scaleIn {
  from { opacity: 0; transform: scale(0.97); }
  to { opacity: 1; transform: scale(1); }
}
</style>