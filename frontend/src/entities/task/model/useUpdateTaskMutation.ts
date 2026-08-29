import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { taskApi } from '../api/taskApi'
import type { UpdateTaskPayload } from '../types'

export function useUpdateTaskMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateTaskPayload }) =>
      taskApi.updateTask(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] })
    }
  })
}
