import axios from 'axios';
import type { 
  CommitmentDto, 
  CreateCommitmentDto, 
  UpdateCommitmentDto,
  DecisionResponseDto, 
  StatusTransitionResponseDto, 
  UpdateStatusDto 
} from '@/types/index';

export interface UserSettingsDto {
  workDayStart: string;
  workDayEnd: string;
  defaultEnergy: number;
  defaultDurationMinutes: number;
}

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
    'X-User-Id': '11111111-1111-1111-1111-111111111111'
  }
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.data) {
      console.error('[RFC 7807 Problem Details]:', error.response.data);
    }
    return Promise.reject(error);
  }
);

export const CompassApi = {
  getNowDecision: async (timeZoneId = 'America/Sao_Paulo'): Promise<DecisionResponseDto> => {
    const response = await api.get<DecisionResponseDto>('/decisions/now', {
      params: { timeZoneId }
    });
    return response.data;
  },

  registerChoice: async (snapshotId: string, chosenCommitmentId: string): Promise<void> => {
    await api.post('/decisions/now/choice', null, {
      params: { snapshotId, chosenCommitmentId }
    });
  },

  getActiveCommitments: async (): Promise<CommitmentDto[]> => {
    const response = await api.get<CommitmentDto[]>('/commitments');
    return response.data;
  },

  createCommitment: async (payload: CreateCommitmentDto): Promise<CommitmentDto> => {
    const response = await api.post<CommitmentDto>('/commitments', payload);
    return response.data;
  },

  updateCommitment: async (id: string, payload: UpdateCommitmentDto): Promise<CommitmentDto> => {
    const response = await api.put<CommitmentDto>(`/commitments/${id}`, payload);
    return response.data;
  },

  updateStatus: async (id: string, payload: UpdateStatusDto): Promise<StatusTransitionResponseDto> => {
    const response = await api.patch<StatusTransitionResponseDto>(`/commitments/${id}/status`, payload);
    return response.data;
  },

  deleteCommitment: async (id: string): Promise<void> => {
    await api.delete(`/commitments/${id}`);
  },

  // --- Rotas da Fase 3: Configurações e Portabilidade ---
  getSettings: async (): Promise<UserSettingsDto> => {
    const response = await api.get<UserSettingsDto>('/settings');
    return response.data;
  },

  updateSettings: async (payload: UserSettingsDto): Promise<UserSettingsDto> => {
    const response = await api.put<UserSettingsDto>('/settings', payload);
    return response.data;
  },

  exportBackup: async (): Promise<Blob> => {
    const response = await api.get('/data/export', { responseType: 'blob' });
    return response.data;
  },

  importBackup: async (file: File): Promise<void> => {
    const formData = new FormData();
    formData.append('file', file);
    await api.post('/data/import', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
  },

  resetDatabase: async (): Promise<void> => {
    await api.post('/data/reset');
  }
};