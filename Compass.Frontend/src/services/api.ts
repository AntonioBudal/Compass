import axios from 'axios';
import type { 
  CommitmentDto, 
  CreateCommitmentDto, 
  DecisionResponseDto, 
  StatusTransitionResponseDto, 
  UpdateStatusDto 
} from '@/types/index';

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
      console.error(' [RFC 7807 Problem Details]:', error.response.data);
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

  updateStatus: async (id: string, payload: UpdateStatusDto): Promise<StatusTransitionResponseDto> => {
    const response = await api.patch<StatusTransitionResponseDto>(`/commitments/${id}/status`, payload);
    return response.data;
  }
};
