import { useQuery } from '@tanstack/vue-query'
import type { Ref } from 'vue'
import { httpClient } from '@/shared/api/httpClient'

// DTOs espelhando o contrato C#
export interface DailyPlanItemDto {
  id: string
  referenceId: string
  title: string
  type: string
  start: string
  end: string
}

export interface DailyPlanDetailsDto {
  id: string
  profileId: string
  date: string
  items: DailyPlanItemDto[]
}

// Client HTTP
export async function getAcceptedDailyPlan(profileId: string, date: string): Promise<DailyPlanDetailsDto> {
  const response = await httpClient.get<DailyPlanDetailsDto>(`/execution/daily-plans/by-date/${date}`, {
    params: {
      profileId
    }
  })
  return response.data
}

// Hook do Vue Query
export function useAcceptedDailyPlan(profileId: Ref<string>, date: Ref<string>) {
  return useQuery({
    queryKey: ['accepted-daily-plan', profileId, date],
    queryFn: () => getAcceptedDailyPlan(profileId.value, date.value),
    enabled: () => !!profileId.value && !!date.value,
    retry: false // Evita retries infinitos se o plano ainda não existir (404)
  })
}