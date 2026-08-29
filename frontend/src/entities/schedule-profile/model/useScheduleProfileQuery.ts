import { computed } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { useRouter } from 'vue-router'
import { scheduleProfileApi } from '../api/scheduleProfileApi'
import { profileStorage } from './profileStorage'
import type { ScheduleProfile } from '../api/types'

export function useScheduleProfileQuery() {
  const router = useRouter()
  const activeId = computed(() => profileStorage.getActiveProfileId())

  const query = useQuery<ScheduleProfile, Error>({
    queryKey: computed(() => ['schedule-profile', activeId.value]),
    queryFn: async () => {
      const id = activeId.value
      if (!id) {
        throw new Error('NO_PROFILE_ID')
      }
      try {
        return await scheduleProfileApi.getScheduleProfile(id)
      } catch (err: any) {
        if (err?.message === 'NOT_FOUND' || err?.message === 'NO_PROFILE_ID') {
          profileStorage.clearActiveProfileId()
          if (router) {
            await router.push('/onboarding')
          }
        }
        throw err
      }
    },
    enabled: computed(() => !!activeId.value),
    retry: false
  })

  return query
}
