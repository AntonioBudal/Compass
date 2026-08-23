import { httpClient } from '@/shared/api/httpClient'
import { isAxiosError } from 'axios'
import type { 
  DailyCycleDetailsDto, 
  StartDailyCycleRequest, 
  StartDailyCycleResponse, 
  RecordExecutionRequest,
  DailyAdherenceReportDto
} from './types'

export async function getDailyCycleByDate(date: string): Promise<DailyCycleDetailsDto | null> {
  try {
    const response = await httpClient.get<DailyCycleDetailsDto>(`/execution/daily-cycles/by-date/${date}`)
    return response.data
  } catch (error) {
    if (isAxiosError(error) && error.response?.status === 404) {
      return null // 404 é um estado esperado: Ciclo não iniciado
    }
    throw error
  }
}

export async function startDailyCycle(data: StartDailyCycleRequest): Promise<StartDailyCycleResponse> {
  const response = await httpClient.post<StartDailyCycleResponse>('/execution/daily-cycles', data)
  return response.data
}

export async function recordExecution(cycleId: string, data: RecordExecutionRequest): Promise<void> {
  await httpClient.post(`/execution/daily-cycles/${cycleId}/executions`, data)
}

export async function closeDailyCycle(cycleId: string): Promise<void> {
  await httpClient.put(`/execution/daily-cycles/${cycleId}/close`)
}

export async function getDailyAdherence(profileId: string, date: string): Promise<DailyAdherenceReportDto | null> {
  try {
    const response = await httpClient.get<DailyAdherenceReportDto>('/execution/daily-adherence', {
      params: { profileId, date }
    })
    return response.data
  } catch (error) {
    if (isAxiosError(error) && error.response?.status === 404) {
      return null // 404 é um estado esperado: Sem plano ou ciclo para comparar
    }
    throw error
  }
}