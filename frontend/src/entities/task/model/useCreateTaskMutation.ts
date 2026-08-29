import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { taskApi } from '../api/taskApi'
import type { CreateTaskPayload } from '../types'

export function useCreateTaskMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: CreateTaskPayload) => taskApi.createTask(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] })
    }
  })
}
