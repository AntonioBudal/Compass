import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { httpClient } from '@/shared/api/httpClient'
import { taskInboxQueryKeys } from '@/entities/task-inbox/api/useTaskInboxQuery'

export interface CreateTaskCommand {
  title: string
  projectId?: string | null
  hardDeadline?: string | null
  estimatedDurationMinutes?: number | null
}

export interface CreateTaskResult {
  id: string
  status: string
}

async function createTask(command: CreateTaskCommand): Promise<CreateTaskResult> {
  const response = await httpClient.post<CreateTaskResult>('/planning/tasks', command)
  return response.data
}

export function useCreateTask() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createTask,
    onSuccess: () => {
      // Invalida a lista da Inbox para buscar a nova Task imediatamente após a criação
      queryClient.invalidateQueries({ queryKey: taskInboxQueryKeys.all })
    }
  })
}