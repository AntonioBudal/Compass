import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import type { Ref } from 'vue'
import { 
  getDailyCycleByDate, 
  startDailyCycle, 
  recordExecution, 
  closeDailyCycle 
} from './dailyCycleApi'
import type { StartDailyCycleRequest, RecordExecutionRequest } from './types'

// QUERY: Carrega o estado atual do ciclo no dia alvo
export function useDailyCycle(date: Ref<string>) {
  return useQuery({
    queryKey: ['daily-cycle', date],
    queryFn: () => getDailyCycleByDate(date.value),
    enabled: !!date.value // Só roda se a data existir
  })
}

// MUTATION: Inicia o ciclo
export function useStartCycle(date: Ref<string>) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (req: StartDailyCycleRequest) => startDailyCycle(req),
    onSuccess: () => {
      // Força recarregar a query que dita a máquina de estado
      queryClient.invalidateQueries({ queryKey: ['daily-cycle', date.value] })
    }
  })
}

// MUTATION: Aponta Execução
export function useRecordExecution(date: Ref<string>) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ cycleId, req }: { cycleId: string, req: RecordExecutionRequest }) => 
      recordExecution(cycleId, req),
    onSuccess: () => {
      // Invalida para o log recém criado descer para o frontend na listagem
      queryClient.invalidateQueries({ queryKey: ['daily-cycle', date.value] })
      
      // Invalida o relatório de Aderência para recalcular o impacto matemático (Fuzzy Invalidation)
      // Como o array inicia com 'daily-adherence', o Vue Query invalidará todas as chaves
      // que possuírem esse prefixo, independente de profileId ou date específicos.
      queryClient.invalidateQueries({ queryKey: ['daily-adherence'] })
    }
  })
}

// MUTATION: Encerra o Ciclo
export function useCloseCycle(date: Ref<string>) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (cycleId: string) => closeDailyCycle(cycleId),
    onSuccess: () => {
      // Recarrega, o status mudará para "Closed"
      queryClient.invalidateQueries({ queryKey: ['daily-cycle', date.value] })
    }
  })
}