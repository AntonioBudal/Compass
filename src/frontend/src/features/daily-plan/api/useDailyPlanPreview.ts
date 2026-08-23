import { useQuery } from '@tanstack/vue-query'
import { getDailyPlanPreview } from './getDailyPlanPreview'
import type { Ref } from 'vue'

export function useDailyPlanPreview(profileId: Ref<string>, date: Ref<string>) {
  return useQuery({
    queryKey: ['daily-plan', profileId, date],
    queryFn: () => getDailyPlanPreview(profileId.value, date.value),
    enabled: () => !!profileId.value && !!date.value,
    retry: false
  })
}