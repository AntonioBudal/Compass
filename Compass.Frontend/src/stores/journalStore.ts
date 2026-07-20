import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { useCommitmentsStore, type CommitmentItem } from '@/stores/commitmentsStore';
import { useToastStore } from '@/stores/toastStore';

export const useJournalStore = defineStore('journal', () => {
  const commitmentsStore = useCommitmentsStore();
  const toastStore = useToastStore();

  const activeTab = ref<'today' | 'week' | 'all'>('today');
  const isShutdownOpen = ref(false);
  const shutdownStep = ref<number>(1); // 1 = Tarefas Pendentes | 2 = Hábitos | 3 = Resumo

  // Filtragem cronológica limpa sem gráficos (High SNR)
  const completedItems = computed(() => {
    return commitmentsStore.items
      .filter(i => i.status === 'COMPLETED')
      .sort((a, b) => {
        // Ordenação decrescente por ID temporário ou timestamp de conclusão
        return b.id.localeCompare(a.id);
      });
  });

  const completedToday = computed(() => {
    // Para o MVP Local-First, consideramos os itens concluídos presentes no cache ativo
    return completedItems.value.slice(0, 15);
  });

  const pendingForShutdown = computed(() => {
    return commitmentsStore.items.filter(
      i => (i.status === 'PENDING' || i.status === 'IN_PROGRESS') && i.type === 'TASK'
    );
  });

  const pendingHabitsForShutdown = computed(() => {
    return commitmentsStore.items.filter(
      i => i.type === 'HABIT' && i.status !== 'COMPLETED' && i.status !== 'ARCHIVED'
    );
  });

  // Métricas de Fechamento Inline (Substituindo Dashboards)
  const auditMetrics = computed(() => {
    const totalCompleted = completedToday.value.length;
    const deepWorkMinutes = completedToday.value.reduce((acc, curr) => {
      return acc + (curr.energyRequired === 3 ? (curr.estimatedDurationMinutes || 30) : 0);
    }, 0);
    const totalMinutes = completedToday.value.reduce((acc, curr) => {
      return acc + (curr.estimatedDurationMinutes || 30);
    }, 0);

    return {
      totalCompleted,
      deepWorkMinutes,
      totalMinutes
    };
  });

  // Ações do Ritual de Shutdown
  const startShutdown = () => {
    shutdownStep.value = 1;
    isShutdownOpen.value = true;
  };

  const postponeItem = async (item: CommitmentItem) => {
    await commitmentsStore.updateStatus(item.id, 'PENDING');
    toastStore.showToast(`"${item.title}" adiado para amanhã.`, 'neutral');
  };

  const completeItem = async (item: CommitmentItem) => {
    await commitmentsStore.updateStatus(item.id, 'COMPLETED');
  };

  const archiveItem = async (item: CommitmentItem) => {
    await commitmentsStore.deleteCommitment(item.id);
  };

  const postponeAllRemaining = async () => {
    const targets = [...pendingForShutdown.value];
    for (const item of targets) {
      await commitmentsStore.updateStatus(item.id, 'PENDING');
    }
    toastStore.showToast(`${targets.length} tarefas adiadas para o próximo turno.`, 'neutral');
    shutdownStep.value = 2;
  };

  const finishShutdown = () => {
    isShutdownOpen.value = false;
    shutdownStep.value = 1;
    toastStore.showToast('Turno útil encerrado com sucesso. Bom descanso.', 'success');
  };

  return {
    activeTab,
    isShutdownOpen,
    shutdownStep,
    completedItems,
    completedToday,
    pendingForShutdown,
    pendingHabitsForShutdown,
    auditMetrics,
    startShutdown,
    postponeItem,
    completeItem,
    archiveItem,
    postponeAllRemaining,
    finishShutdown
  };
});