import { useQuery } from '@tanstack/vue-query'
import type { Ref } from 'vue'
import { httpClient } from '@/shared/api/httpClient'

export interface TaskAdherenceDto {
  referenceId: string
  title: string
  plannedMinutes: number
  executedMinutes: number
  intersectedMinutes: number
}

export interface DailyAdherenceReportDto {
  profileId: string
  date: string
  totalPlannedMinutes: number
  totalExecutedMinutes: number
  globalConformityPercentage: number
  tasks: TaskAdherenceDto[]
}

export async function getDailyAdherence(profileId: string, date: string): Promise<DailyAdherenceReportDto> {
  const response = await httpClient.get<DailyAdherenceReportDto>('/execution/daily-adherence', {
    params: { profileId, date }
  })
  return response.data
}

// Chave exposta (Query Key Factory) para permitir que a mutação de apontamento a invalide explicitamente
export const adherenceQueryKeys = {
  byDate: (profileId: string, date: string) => ['daily-adherence', profileId, date] as const
}

export function useDailyAdherenceQuery(profileId: Ref<string>, date: Ref<string>) {
  return useQuery({
    queryKey: adherenceQueryKeys.byDate(profileId.value, date.value),
    queryFn: () => getDailyAdherence(profileId.value, date.value),
    enabled: () => !!profileId.value && !!date.value,
    retry: false // Se não houver plano, a Minimal API retorna 404 (Not Found). Falhamos rápido.
  })
}