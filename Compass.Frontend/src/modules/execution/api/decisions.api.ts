import { apiClient } from '../../../shared/api/client';

export const DecisionsApi = {
  //  FIX 404: A rota real do backend é /now (a Store antiga ignorava o /decisions)
  getNowDecision: async (windowMinutes: number, energy: number, timeZoneId = 'America/Sao_Paulo') => 
    (await apiClient.get('/now', { params: { windowMinutes, energy, timeZoneId } })).data,
    
  registerChoice: async (snapshotId: string, chosenCommitmentId: string) => 
    await apiClient.post('/now/choice', null, { params: { snapshotId, chosenCommitmentId } }),
};