import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { taskApi } from '../api/taskApi'

export function useCompleteTaskMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => taskApi.completeTask(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] })
    }
  })
}
