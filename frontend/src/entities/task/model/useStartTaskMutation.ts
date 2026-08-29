import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { taskApi } from '../api/taskApi'

export function useStartTaskMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => taskApi.startTask(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] })
    }
  })
}
