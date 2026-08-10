import { useRoute, useRouter } from 'vue-router';
import { useToastStore } from '@/shared/stores/toastStore';
import type { CommitmentDto } from '@/shared/types/global';

export function useVisibilityTracker() {
  const route = useRoute();
  const router = useRouter();
  const toastStore = useToastStore();

  /**
   * Avalia onde o compromisso recém-criado foi parar e avisa o usuário se ele não for visível na tela atual.
   */
  function verifyCreationVisibility(item: CommitmentDto, currentViewItems: { id: string }[]) {
    // 1. Se o item já apareceu renderizado na grade atual, suprimimos qualquer aviso (Silêncio Positivo)
    const isRendered = currentViewItems.some(i => i.id === item.id);
    if (isRendered) return;

    // 2. INTERVENÇÃO: Hábito criado que não aparece na visão "Hoje" (Cron ou fuso horário fora)
    if (item.type === 'HABIT' && route.path === '/habits') {
      toastStore.showIntervention({
        code: 'HABIT_NOT_VISIBLE_TODAY',
        title: 'Hábito criado, mas fora do ciclo de hoje.',
        explanation: `Seu novo hábito "${item.title}" foi configurado com recorrência (${item.cronExpression || 'Custom'}). Ele começará a ser exibido no próximo ciclo correspondente.`,
        severity: 'info',
        actions: [
          {
            label: 'Ver no Horizonte (Amanhã)',
            isPrimary: true,
            handler: () => {
              // Dispara evento para alternar a aba do horizonte tático na tela
              window.dispatchEvent(new CustomEvent('compass:set-horizon', { detail: 'tomorrow' }));
            }
          }
        ]
      });
      return;
    }

    // 3. INTERVENÇÃO: Tarefa criada após as 18:00 ou antes das 07:00 (Fora do Turno Útil)
    const currentHour = new Date().getHours();
    const isOutsideShift = currentHour >= 18 || currentHour < 7;
    if (item.type === 'TASK' && isOutsideShift && route.path === '/now') {
      toastStore.showIntervention({
        code: 'TASK_SCHEDULED_FOR_TOMORROW',
        title: 'Seu turno de hoje já encerrou (18:00).',
        explanation: `A tarefa "${item.title}" foi salva em segurança, mas o motor de decisão reservou sua execução para o início de amanhã para proteger sua recuperação.`,
        severity: 'info',
        actions: [
          {
            label: 'Mover para Amanhã (Padrão)',
            isPrimary: true,
            handler: () => {}
          },
          {
            label: 'Executar Hoje (Hora Extra)',
            handler: () => {
              window.dispatchEvent(new CustomEvent('compass:force-today', { detail: { commitmentId: item.id } }));
            }
          }
        ]
      });
      return;
    }

    // 4. INTERVENÇÃO: Item criado na tela errada (ex: criou um Evento na tela de Hábitos)
    if (item.type === 'EVENT' && route.path !== '/agenda') {
      toastStore.showIntervention({
        code: 'EVENT_ROUTED_TO_AGENDA',
        title: 'Evento adicionado à sua Agenda.',
        explanation: `Como eventos possuem horários rígidos (${item.startTime || 'Definido'}), eles são visualizados na linha do tempo cronológica, não em filas estáticas.`,
        severity: 'info',
        actions: [
          {
            label: 'Ver na Agenda agora',
            isPrimary: true,
            handler: async () => {
            await router.push('/agenda');
            }
          }
        ]
      });
      return;
    }
  }

  return {
    verifyCreationVisibility
  };
}