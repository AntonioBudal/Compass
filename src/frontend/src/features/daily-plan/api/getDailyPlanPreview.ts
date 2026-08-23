import { httpClient } from '@/shared/api/httpClient'
import type { DailyPlanPreview } from './types'

export async function getDailyPlanPreview(profileId: string, date: string): Promise<DailyPlanPreview> {
  const response = await httpClient.get<DailyPlanPreview>('/execution/daily-plan', {
    params: {
      profileId,
      date,
    },
  })
  
  return response.data
}