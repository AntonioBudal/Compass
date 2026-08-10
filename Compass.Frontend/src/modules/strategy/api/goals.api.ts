import { apiClient } from '../../../shared/api/client';

export const GoalsApi = {
  createGoal: async (payload: { title: string, whyDescription?: string | null, targetDate?: string | null }) => 
    (await apiClient.post('/goals', payload)).data,
  getActiveGoals: async () => 
    (await apiClient.get('/goals/active')).data,
  updateGoal: async (id: string, payload: { title: string, whyDescription?: string | null, targetDate?: string | null }) => 
    (await apiClient.put(`/goals/${id}`, payload)).data,
  deleteGoal: async (id: string) => 
    await apiClient.delete(`/goals/${id}`),
};