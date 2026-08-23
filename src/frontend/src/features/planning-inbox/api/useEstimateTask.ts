import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { httpClient } from '@/shared/api/httpClient'
import { taskInboxQueryKeys } from '@/entities/task-inbox/api/useTaskInboxQuery'

export interface EstimateTaskRequest {
  estimatedDurationMinutes: number
}

// Interface auxiliar para os argumentos da mutação
interface EstimateTaskArgs {
  id: string
  request: EstimateTaskRequest
}

async function estimateTask({ id, request }: EstimateTaskArgs): Promise<void> {
  await httpClient.put(`/planning/tasks/${id}/estimate`, request)
}

export function useEstimateTask() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: estimateTask,
    onSuccess: () => {
      // Invalida a inbox para refletir visualmente a transição Draft -> Ready
      queryClient.invalidateQueries({ queryKey: taskInboxQueryKeys.all })
      
      // Se tivéssemos o DailyPlan carregado em paralelo, 
      // invalidar a chave raiz garante que todas as projeções afetadas sejam re-buscadas.
      // queryClient.invalidateQueries({ queryKey: ['planning'] })
    }
  })
}