import type { TaskDto, CreateTaskPayload, UpdateTaskPayload, TaskStatus } from '../types'

const BASE_URL = '/api/planning/tasks'

export const taskApi = {
  async fetchTasks(status?: TaskStatus): Promise<TaskDto[]> {
    const url = status ? `${BASE_URL}?status=${status}` : BASE_URL
    const response = await fetch(url)
    if (!response.ok) {
      const error = await response.json().catch(() => ({}))
      throw new Error(error.detail || error.title || 'Falha ao carregar tarefas.')
    }
    return response.json()
  },

  async getTaskById(id: string): Promise<TaskDto> {
    const response = await fetch(`${BASE_URL}/${id}`)
    if (!response.ok) {
      const error = await response.json().catch(() => ({}))
      throw new Error(error.detail || error.title || 'Tarefa não encontrada.')
    }
    return response.json()
  },

  async createTask(payload: CreateTaskPayload): Promise<TaskDto> {
    const response = await fetch(BASE_URL, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(payload)
    })

    if (!response.ok) {
      const error = await response.json().catch(() => ({}))
      throw new Error(error.detail || error.title || 'Falha ao criar tarefa.')
    }

    return response.json()
  },

  async updateTask(id: string, payload: UpdateTaskPayload): Promise<TaskDto> {
    const response = await fetch(`${BASE_URL}/${id}`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(payload)
    })

    if (!response.ok) {
      const error = await response.json().catch(() => ({}))
      throw new Error(error.detail || error.title || 'Falha ao atualizar tarefa.')
    }

    return response.json()
  },

  async startTask(id: string): Promise<TaskDto> {
    const response = await fetch(`${BASE_URL}/${id}/start`, {
      method: 'POST'
    })

    if (!response.ok) {
      const error = await response.json().catch(() => ({}))
      throw new Error(error.detail || error.title || 'Falha ao iniciar tarefa.')
    }

    return response.json()
  },

  async completeTask(id: string): Promise<TaskDto> {
    const response = await fetch(`${BASE_URL}/${id}/complete`, {
      method: 'POST'
    })

    if (!response.ok) {
      const error = await response.json().catch(() => ({}))
      throw new Error(error.detail || error.title || 'Falha ao concluir tarefa.')
    }

    return response.json()
  }
}
