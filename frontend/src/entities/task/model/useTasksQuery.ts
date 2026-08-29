import { useQuery } from '@tanstack/vue-query'
import { type Ref, computed } from 'vue'
import { taskApi } from '../api/taskApi'
import type { TaskStatus } from '../types'

export function useTasksQuery(statusRef?: Ref<TaskStatus | undefined>) {
  return useQuery({
    queryKey: computed(() => ['tasks', statusRef?.value]),
    queryFn: () => taskApi.fetchTasks(statusRef?.value)
  })
}
