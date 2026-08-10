import { apiClient } from '../../../shared/api/client';

export const SchedulesApi = {
  fetchTodaySchedule: async () => 
    (await apiClient.get('/schedules/today')).data,
};