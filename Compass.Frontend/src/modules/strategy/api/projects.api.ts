import { apiClient } from '../../../shared/api/client';

export const ProjectsApi = {
  createProject: async (payload: { name: string, goalId?: string | null }) => 
    (await apiClient.post('/projects', payload)).data,
  updateProject: async (id: string, payload: { name: string, goalId?: string | null }) => 
    (await apiClient.put(`/projects/${id}`, payload)).data,
  deleteProject: async (id: string) => 
    await apiClient.delete(`/projects/${id}`),
};