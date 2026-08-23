import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { httpClient } from '@/shared/api/httpClient'
import { scheduleProfileQueryKeys } from '@/entities/calendar-profile/api/useScheduleProfileQuery'

export interface AddWindowRequest {
  dayOfWeek: string
  startTime: string // "HH:mm"
  endTime: string   // "HH:mm"
}

interface AddWindowArgs {
  profileId: string
  request: AddWindowRequest
}

async function addScheduleWindow({ profileId, request }: AddWindowArgs): Promise<void> {
  await httpClient.post(`/calendar/profiles/${profileId}/windows`, request)
}

export function useAddScheduleWindow() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: addScheduleWindow,
    onSuccess: (_, variables) => {
      // 1. Invalida o setup do calendário para mostrar a nova janela
      queryClient.invalidateQueries({
        queryKey: scheduleProfileQueryKeys.detail(variables.profileId)
      })

      // 2. Invalida o DailyPlan via string (desacoplado) para respeitar o FSD
      queryClient.invalidateQueries({ queryKey: ['daily-plan-preview'] })
      queryClient.invalidateQueries({ queryKey: ['daily-plan'] })
    }
  })
}