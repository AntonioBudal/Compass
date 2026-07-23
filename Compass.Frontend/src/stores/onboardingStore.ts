import { defineStore } from 'pinia';
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useToastStore } from '@/stores/toastStore';

export interface SandboxCommitment {
  id: string;
  title: string;
  type: 'TASK' | 'EVENT' | 'HABIT' | 'NOTE';
  status: 'pending' | 'completed' | 'postponed';
  estimatedDurationMinutes: number;
  energyRequired: number;
  timeLabel?: string;
}

export const useOnboardingStore = defineStore('onboarding', () => {
  const router = useRouter();
  const toastStore = useToastStore();
  
  const isSandboxActive = ref(false);
  const commitments = ref<SandboxCommitment[]>([]);

  // 1. Injeção de Dados Fictícios na Memória RAM
  const seedSandboxData = () => {
    commitments.value = [
      {
        id: 'box-1',
        title: 'Explorar a interface monocromática do Compass',
        type: 'TASK',
        status: 'pending',
        estimatedDurationMinutes: 15,
        energyRequired: 1
      },
      {
        id: 'box-2',
        title: 'Alinhamento de Arquitetura Local-First',
        type: 'EVENT',
        status: 'pending',
        estimatedDurationMinutes: 45,
        energyRequired: 3,
        timeLabel: '14:00 - 14:45'
      },
      {
        id: 'box-3',
        title: 'Beber 500ml de água e alongar a coluna',
        type: 'HABIT',
        status: 'pending',
        estimatedDurationMinutes: 5,
        energyRequired: 1
      },
      {
        id: 'box-4',
        title: 'A Janela de Foco calcula seu tempo livre automaticamente',
        type: 'NOTE',
        status: 'pending',
        estimatedDurationMinutes: 0,
        energyRequired: 1
      }
    ];
    isSandboxActive.value = true;
  };

  // 2. Mutações Isoladas em RAM (Zero-DB)
  const toggleComplete = (id: string) => {
    const item = commitments.value.find(c => c.id === id);
    if (item) {
      if (item.status === 'completed') {
        item.status = 'pending';
        // Corrigido de 'info' para 'neutral'
        toastStore.showToast('Item restaurado para pendente (Modo RAM).', 'neutral');
      } else {
        item.status = 'completed';
        toastStore.showToast('Item concluído com sucesso! (Modo RAM).', 'success');
      }
    }
  };

  const postponeItem = (id: string) => {
    const item = commitments.value.find(c => c.id === id);
    if (item) {
      item.status = 'postponed';
      // Corrigido de 'warning' para 'urgent'
      toastStore.showToast('Compromisso adiado para o próximo turno.', 'urgent');
    }
  };

  const addSandboxItem = (title: string, type: 'TASK' | 'EVENT' | 'HABIT' | 'NOTE' = 'TASK') => {
    const newItem: SandboxCommitment = {
      id: 'box-' + Math.random().toString(36).substring(2, 7),
      title,
      type,
      status: 'pending',
      estimatedDurationMinutes: 25,
      energyRequired: 2
    };
    commitments.value.unshift(newItem);
    toastStore.showToast('Novo item simulado adicionado à memória.', 'success');
  };

  // 3. Encerrar o Sandbox e Limpar a Memória
  const finishOnboarding = () => {
    isSandboxActive.value = false;
    commitments.value = []; // Esvazia o lixo da RAM
    
    try {
      localStorage.setItem('compass_onboarded', 'true');
    } catch (e) {
      console.warn('[SandboxStore]: Falha ao persistir flag de onboarding.', e);
    }

    toastStore.showToast('Sandbox encerrado. Bem-vindo ao ecossistema real!', 'success');
    router.push('/');
  };

  const skipOnboarding = () => {
    finishOnboarding();
  };

  return {
    isSandboxActive,
    commitments,
    seedSandboxData,
    toggleComplete,
    postponeItem,
    addSandboxItem,
    finishOnboarding,
    skipOnboarding
  };
});