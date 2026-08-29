import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { scheduleProfileApi } from '../api/scheduleProfileApi'
import { profileStorage } from './profileStorage'
import type { CreateScheduleProfileRequest, ScheduleProfile } from '../api/types'

export function useCreateScheduleProfileMutation() {
  const queryClient = useQueryClient()

  return useMutation<ScheduleProfile, Error, CreateScheduleProfileRequest>({
    mutationFn: (data) => scheduleProfileApi.createScheduleProfile(data),
    onSuccess: (profile) => {
      profileStorage.setActiveProfileId(profile.id)
      queryClient.setQueryData(['schedule-profile', profile.id], profile)
    }
  })
}
