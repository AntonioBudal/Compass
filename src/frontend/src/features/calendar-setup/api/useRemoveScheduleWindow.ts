import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { httpClient } from '@/shared/api/httpClient'
import { scheduleProfileQueryKeys } from '@/entities/calendar-profile/api/useScheduleProfileQuery'

interface RemoveWindowArgs {
  profileId: string
  windowId: string
}

async function removeScheduleWindow({ profileId, windowId }: RemoveWindowArgs): Promise<void> {
  await httpClient.delete(`/calendar/profiles/${profileId}/windows/${windowId}`)
}

export function useRemoveScheduleWindow() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: removeScheduleWindow,
    onSuccess: (_, variables) => {
      // 1. Invalida o setup do calendário para sumir com a janela excluída
      queryClient.invalidateQueries({
        queryKey: scheduleProfileQueryKeys.detail(variables.profileId)
      })

      // 2. Invalida o DailyPlan via string (desacoplado) para respeitar o FSD
      queryClient.invalidateQueries({ queryKey: ['daily-plan-preview'] })
      queryClient.invalidateQueries({ queryKey: ['daily-plan'] })
    }
  })
}