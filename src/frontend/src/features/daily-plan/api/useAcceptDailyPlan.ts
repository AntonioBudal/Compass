import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { httpClient } from '@/shared/api/httpClient'
import type { AcceptDailyPlanRequest, AcceptDailyPlanResponse } from './types'

async function acceptDailyPlan(data: AcceptDailyPlanRequest): Promise<AcceptDailyPlanResponse> {
  const response = await httpClient.post<AcceptDailyPlanResponse>('/execution/daily-plans', data)
  return response.data
}

export function useAcceptDailyPlan() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: acceptDailyPlan,
    retry: false, // Nunca tentar POSTs de criação silenciosamente
    onSuccess: (_, variables) => {
      // Invalida a query de preview para forçar atualização se necessário no futuro
      queryClient.invalidateQueries({
        queryKey: ['daily-plan', variables.profileId, variables.date]
      })
    }
  })
}