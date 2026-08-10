import { apiClient } from '../../../shared/api/client';
import type { CommitmentDto, CreateCommitmentDto, UpdateCommitmentDto, StatusTransitionResponseDto, UpdateStatusDto } from '@/shared/types/global';

export const CommitmentsApi = {
  getActiveCommitments: async (): Promise<CommitmentDto[]> => 
    (await apiClient.get<CommitmentDto[]>('/commitments')).data,
    
  fetchDatabase: async (params: string): Promise<any> => 
    (await apiClient.get(`/commitments/all?${params}`)).data,
    
  createCommitment: async (payload: CreateCommitmentDto): Promise<CommitmentDto> => 
    (await apiClient.post<CommitmentDto>('/commitments', payload)).data,
    
  updateCommitment: async (id: string, payload: UpdateCommitmentDto): Promise<CommitmentDto> => 
    (await apiClient.put<CommitmentDto>(`/commitments/${id}`, payload)).data,
    
  updateStatus: async (id: string, payload: UpdateStatusDto): Promise<StatusTransitionResponseDto> => 
    (await apiClient.patch<StatusTransitionResponseDto>(`/commitments/${id}/status`, payload)).data,
    
  deleteCommitment: async (id: string): Promise<void> => 
    await apiClient.delete(`/commitments/${id}`),
};