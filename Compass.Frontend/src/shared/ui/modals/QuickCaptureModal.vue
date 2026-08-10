<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue';
import { useCommitmentsStore } from '@/modules/tactical/stores/commitmentsStore';
import { useProjectsStore } from '@/modules/strategy/stores/projectsStore';
import { useToastStore } from '@/shared/stores/toastStore';
import { parseQuickCapture } from '@/shared/utils/nlpParser';
import { useVisibilityTracker } from '@/shared/composables/useVisibilityTracker';
import { Terminal, CornerDownLeft, Clock, Zap, Calendar, Folder } from 'lucide-vue-next';
import OmniInput from '@/components/core/OmniInput.vue';
import { useGoalsStore } from '@/modules/strategy/stores/goalsStore';

const props = defineProps<{ isOpen: boolean }>();
const emit = defineEmits<{ (e: 'close'): void }>();

const commitmentsStore = useCommitmentsStore();
const projectsStore = useProjectsStore();
const toastStore = useToastStore();
const { verifyCreationVisibility } = useVisibilityTracker();

const rawInput = ref('');
const isSubmitting = ref(false);

onMounted(() => {
  window.addEventListener('compass:inject-project', ((e: CustomEvent<string>) => {
    rawInput.value = `#${e.detail} `;
  }) as EventListener);
});

watch(() => props.isOpen, (open) => {
  if (!open) rawInput.value = '';
});

const livePreview = computed(() => parseQuickCapture(rawInput.value));

const getNextWorkDay = (currentIso: string | null) => {
  const date = currentIso ? new Date(currentIso) : new Date();
  date.setDate(date.getDate() + 1);
  if (date.getDay() === 6) date.setDate(date.getDate() + 2);
  if (date.getDay() === 0) date.setDate(date.getDate() + 1);
  return date.toISOString();
};

// Adicione um interceptador tático antes do parser normal
const captureIntent = computed(() => {
  const text = rawInput.value.trim().toLowerCase();
  
  if (text.startsWith('/meta ') || text.startsWith('/goal ')) {
    return { type: 'GOAL', title: rawInput.value.substring(6).trim() };
  }
  
  if (text.startsWith('/projeto ') || text.startsWith('/proj ')) {
    const titlePart = rawInput.value.split(/#|@|!|\^/)[0].substring(9).trim(); // Pega só o título
    return { type: 'PROJECT', title: titlePart };
  }

  return { type: 'COMMITMENT', data: parseQuickCapture(rawInput.value) };
});

// A interface reage a intenção para mostrar a badge correta
const displayType = computed(() => {
  if (captureIntent.value.type === 'GOAL') return 'META ESTRATÉGICA';
  if (captureIntent.value.type === 'PROJECT') return 'NOVO PROJETO';
  return captureIntent.value.data?.type || 'COMANDO'; //  O '?' salva a vida aqui!
});



const handleSubmit = async (forceSchedule = false, overrideDateIso: string | null = null) => {
  if (!rawInput.value.trim() || isSubmitting.value) return;
  isSubmitting.value = true;
  const intent = captureIntent.value;

  try {
    //  ROTA 1: CRIAÇÃO PURA DE META (Top-Down)
   
    if (intent.type === 'GOAL') {
      const goalsStore = useGoalsStore();
      
      // O await aqui é crucial para aguardar o Guid do Backend
      await goalsStore.createGoal({ title: intent.title! }); 
      
      toastStore.showToast(`Meta "${intent.title}" criada com sucesso!`, 'success');
    }
    //  ROTA 2: CRIAÇÃO PURA DE PROJETO (Top-Down)
    else if (intent.type === 'PROJECT') {
      const projectsStore = useProjectsStore();
      await projectsStore.createProject(intent.title!);
      toastStore.showToast(`Projeto "${intent.title}" criado com sucesso!`, 'success');
    }
    //  ROTA 3: CRIAÇÃO DE COMPROMISSO (Bottom-Up normal)
    else if (intent.type === 'COMMITMENT' && intent.data) {
      const parsed = intent.data;
      let matchedProjectId: string | null = null;
      
      const projectsStore = useProjectsStore();
      if (parsed.projectQuery) {
        const match = projectsStore.catalog.find(p => p.name.toLowerCase() === parsed.projectQuery?.toLowerCase());
        if (match) {
          matchedProjectId = match.id;
          projectsStore.promoteUsage(match.id);
        } else {
          const newProject = await projectsStore.createProject(parsed.projectQuery);
          matchedProjectId = newProject.id;
        }
      }

      const createdItem = await commitmentsStore.createCommitment({
        title: parsed.title,
        type: parsed.type,
        estimatedDurationMinutes: parsed.estimatedDurationMinutes,
        energyRequired: parsed.energyRequired,
        deadline: overrideDateIso || parsed.deadlineIso,
        projectId: matchedProjectId
      });

      toastStore.showToast(`[${parsed.type}] capturado com sucesso!`, 'success');
      
      setTimeout(() => {
        const currentList = parsed.type === 'HABIT' ? commitmentsStore.habitsToday : commitmentsStore.activeCandidates;
        verifyCreationVisibility(createdItem, currentList);
      }, 100);
    }

    rawInput.value = '';
    emit('close');
  } catch (err: any) {
    emit('close'); //  ARQ: Libera a tela para o usuário poder ler os alertas!

    console.error('[QuickCaptureModal] Falha Crítica Capturada:', err);

    if (err.response?.status === 404) {
      toastStore.showToast('Endpoint não encontrado (404). Você esqueceu de reiniciar o Backend?', 'error');
      return;
    }

    const errData = err.response?.data || {};
    const errorCode = errData.code || errData.type || '';
    
    //  ARQ: Correção do escopo. Só tenta ler a data se for realmente um Compromisso (Tarefa).
    if (errorCode.includes('SCHEDULE_CONFLICT') || errData.message?.includes('turno') || errData.detail?.includes('turno') || errData.detail?.includes('Schedule')) {
       const deadline = intent.type === 'COMMITMENT' ? intent.data?.deadlineIso : null;
       const suggestedDate = errData.suggestedDate || getNextWorkDay(deadline || null);
       
       toastStore.showIntervention({
         code: 'SCHEDULE_CONFLICT',
         title: 'Fora do Calendário Útil',
         explanation: 'A data que você escolheu cai em um período bloqueado. O algoritmo pode ajustá-la.',
         severity: 'warning',
         actions: [
           { label: 'Mover para Próximo Dia Útil', isPrimary: true, handler: async () => {
               isSubmitting.value = false;
               await handleSubmit(false, suggestedDate);
             }
           },
           { label: 'Cancelar', handler: () => {} }
         ]
       });
       return;
    }

    const isNetworkError = !err.response;
    const errorMessage = errData.detail || errData.title || 'Falha de validação no servidor.';
    
    toastStore.showIntervention({
      code: isNetworkError ? 'NETWORK_FAILURE' : 'VALIDATION_ERROR',
      title: isNetworkError ? 'Falha de Conexão' : 'Ação Bloqueada',
      explanation: errorMessage,
      severity: isNetworkError ? 'blocking' : 'warning',
      actions: [{ label: 'Fechar', isPrimary: true, handler: () => {} }]
    });
  } finally {
    isSubmitting.value = false;
  }
};
</script>

<template>
  <Teleport to="body">
    <Transition name="modal-fade">
      <div v-if="isOpen" class="fixed inset-0 z-50 flex items-start justify-center pt-[15vh] px-4 bg-app/80 backdrop-blur-sm select-none" @click.self="emit('close')" role="dialog" aria-modal="true">
        <div class="w-full max-w-2xl bg-surface border border-borderfocus rounded-xl shadow-2xl font-mono flex flex-col relative animate-scale-in">
          
          <div class="flex items-center justify-between px-4 py-2.5 bg-app border-b border-borderbase text-xs text-content-muted rounded-t-xl">
            <span class="flex items-center gap-2 font-bold uppercase tracking-wider text-content">
              <Terminal class="w-4 h-4 text-content-muted" />
              <span>Quick Capture CLI v3.0</span>
            </span>
            <span class="text-[11px] flex items-center gap-3">
              <span>Gatilhos: <strong class="text-content">#</strong>proj <strong class="text-content">@</strong>tempo <strong class="text-content">!</strong>ene <strong class="text-content">^</strong>data <strong class="text-content">/</strong>tipo</span>
              <kbd class="px-1.5 py-0.5 rounded bg-surface border border-borderbase font-sans text-[10px]">ESC para fechar</kbd>
            </span>
          </div>

          <div class="p-4 relative bg-surface">
            <OmniInput
              v-model="rawInput"
              placeholder="O que vamos fazer agora? (ex: Revisar PR #core @45m !3 ^amanha /t)"
              :autofocus="isOpen"
              @submit="handleSubmit(false, null)"
              @escape="emit('close')"
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
              <span v-if="livePreview.deadlineIso" class="flex items-center gap-1 text-content-muted" title="Limite temporal">
                <Calendar class="w-3.5 h-3.5 text-content" />
                <span>{{ new Date(livePreview.deadlineIso).toLocaleDateString() }}</span>
              </span>
            </div>

            <button
              @click="() => handleSubmit(false, null)"
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