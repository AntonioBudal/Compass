export type TaskStatus = 'Draft' | 'Ready' | 'InProgress' | 'Done'

export interface TaskDto {
  id: string
  title: string
  description: string | null
  durationMinutes: number | null
  deadline: string | null
  status: TaskStatus
  createdAt: string
  updatedAt: string
  completedAt: string | null
}

export interface CreateTaskPayload {
  title: string
  description?: string | null
  durationMinutes?: number | null
  deadline?: string | null
}

export interface UpdateTaskPayload {
  title: string
  description?: string | null
  durationMinutes?: number | null
  deadline?: string | null
}
