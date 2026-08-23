import { useQuery } from '@tanstack/vue-query'
import { httpClient } from '@/shared/api/httpClient'

export interface ScheduleWindowDto {
  id: string
  dayOfWeek: 'Sunday' | 'Monday' | 'Tuesday' | 'Wednesday' | 'Thursday' | 'Friday' | 'Saturday'
  startTime: string // Formato "hh:mm:ss" originado do TimeSpan
  endTime: string   // Formato "hh:mm:ss" originado do TimeSpan
}

export interface ScheduleProfileDetailsDto {
  id: string
  timezone: string
  windows: ScheduleWindowDto[]
}

export const scheduleProfileQueryKeys = {
  all: ['calendar', 'profile'] as const,
  detail: (profileId: string) => [...scheduleProfileQueryKeys.all, profileId] as const,
}

async function getScheduleProfile(profileId: string): Promise<ScheduleProfileDetailsDto> {
  const response = await httpClient.get<ScheduleProfileDetailsDto>(`/calendar/profiles/${profileId}`)
  return response.data
}

export function useScheduleProfileQuery(profileId: string | undefined) {
  return useQuery({
    queryKey: scheduleProfileQueryKeys.detail(profileId || ''),
    queryFn: () => getScheduleProfile(profileId!),
    enabled: !!profileId && profileId.length > 30, // Só executa a query se tivermos um GUID válido
  })
}