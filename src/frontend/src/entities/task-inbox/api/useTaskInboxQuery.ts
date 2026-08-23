import { useQuery } from '@tanstack/vue-query'
import { httpClient } from '@/shared/api/httpClient'

export interface TaskDetailsDto {
  id: string
  title: string
  status: 'Draft' | 'Ready' | 'InProgress' | 'Completed' | 'Blocked' | 'Cancelled'
  estimatedDurationMinutes: number | null
  hardDeadline: string | null
  projectId: string | null
}

export async function getInboxTasks(): Promise<TaskDetailsDto[]> {
  const response = await httpClient.get<TaskDetailsDto[]>('/planning/tasks/inbox')
  return response.data
}

// Factory de query keys para padronização e facilidade de invalidação pelas mutations
export const taskInboxQueryKeys = {
  all: ['planning', 'tasks', 'inbox'] as const
}

// Hook de leitura passiva. Sem mutações ou dependências de Data.
export function useTaskInboxQuery() {
  return useQuery({
    queryKey: taskInboxQueryKeys.all,
    queryFn: getInboxTasks,
    // Opcional: configurar refetchOnWindowFocus ou staleTime conforme a necessidade do negócio,
    // mas os padrões do Vue Query costumam ser bons para a Inbox.
  })
}