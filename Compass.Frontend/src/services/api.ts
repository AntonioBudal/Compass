import axios, { type InternalAxiosRequestConfig, type AxiosResponse, type AxiosError } from 'axios';
import { useDevStore } from '@/stores/devStore'
import { useOfflineStore } from '@/stores/offlineStore';
import type { 
  CommitmentDto, 
  CreateCommitmentDto, 
  UpdateCommitmentDto,
  DecisionResponseDto, 
  StatusTransitionResponseDto, 
  UpdateStatusDto 
} from '@/types/index';

// Gerador nativo e rápido de UUID v4 para marcação transacional no cliente
function generateCorrelationId(): string {
  if (typeof crypto !== 'undefined' && crypto.randomUUID) {
    return crypto.randomUUID();
  }
  return 'corr-' + Math.random().toString(36).substring(2, 11) + '-' + Date.now();
}

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5163/api/v1',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
    'X-User-Id': '11111111-1111-1111-1111-111111111111'
  }
});

// Interceptor de Saída (Request Piping)
api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const corrId = generateCorrelationId();
  config.headers.set('X-Correlation-Id', corrId);
  
  // Anexa metadado de temporização no objeto de requisição para cálculo exato de latência
  (config as any)._metadata = { startTime: performance.now(), correlationId: corrId };
  
  return config;
});

// Interceptor de Chegada e Tratamento RFC 7807 (Response & Problem Details Piping)
api.interceptors.response.use(
  (response: AxiosResponse) => {
    const meta = (response.config as any)._metadata || {};
    const duration = Math.round(performance.now() - (meta.startTime || performance.now()));
    
    // Alimenta o Developer Mode em background em modo de desenvolvimento
    if (import.meta.env.DEV) {
      try {
        const devStore = useDevStore();
        devStore.logHttp({
          id: meta.correlationId || generateCorrelationId(),
          timestamp: new Date().toISOString().slice(11, 23),
          method: response.config.method?.toUpperCase() || 'GET',
          url: response.config.url || '',
          status: response.status,
          latencyMs: duration,
          payload: response.data,
          isProblemDetails: false
        });
      } catch (e) { /* Silencia erros de observabilidade para não impactar o app */ }
    }
    
    return response;
  },
  async (error: AxiosError) => {
    const config = error.config as any || {};
    const meta = config._metadata || {};
    const duration = Math.round(performance.now() - (meta.startTime || performance.now()));
    const responseData: any = error.response?.data;

    // --- INTERCEPTADOR DE RESILIÊNCIA OFFLINE (LOCAL-FIRST) ---
    // Se for erro de rede ou timeout em uma operação de escrita, enfileira localmente
    const isNetworkError = !error.response || error.code === 'ECONNABORTED' || error.message.includes('Network Error');
    const isWriteOperation = config.method && ['post', 'put', 'patch', 'delete'].includes(config.method.toLowerCase());

    if (isNetworkError && isWriteOperation) {
      try {
        const offlineStore = useOfflineStore();
        offlineStore.addToQueue({
          url: config.url || '',
          method: config.method || 'POST',
          payload: config.data ? JSON.parse(config.data) : null
        });

        // Retorna uma resposta simulada de sucesso (202 Accepted) para evitar o Rollback na UI!
        return Promise.resolve({
          data: { id: 'offline-pending', status: 'PENDING', title: 'Salvo localmente' },
          status: 202,
          statusText: 'Accepted Offline',
          headers: {},
          config
        });
      } catch (e) {
        console.warn('[OfflinePiping]: Falha ao enfileirar requisição offline.', e);
      }
    }
    // ------------------------------------------------------------

    // Identifica se a resposta obedece rigorosamente ao contrato RFC 7807 Problem Details
    const contentType = error.response?.headers['content-type'];
    const isProblem =
      (typeof contentType === 'string' && contentType.includes('problem+json')) ||
      (responseData && typeof responseData === 'object' && 'status' in responseData && 'title' in responseData);

    if (import.meta.env.DEV) {
      try {
        const devStore = useDevStore();
        devStore.logHttp({
          id: meta.correlationId || generateCorrelationId(),
          timestamp: new Date().toISOString().slice(11, 23),
          method: config.method?.toUpperCase() || 'ERR',
          url: config.url || '',
          status: error.response?.status || 0,
          latencyMs: duration,
          payload: responseData || { message: error.message },
          isProblemDetails: !!isProblem
        });
      } catch (e) { /* Silenciamento de segurança */ }
    }

    if (isProblem) {
      console.error(`[RFC 7807 Problem Details | CorrID: ${meta.correlationId}]:`, responseData);
    }

    return Promise.reject(error);
  }
);

export const CompassApi = {
  getNowDecision: async (timeZoneId = 'America/Sao_Paulo'): Promise<DecisionResponseDto> => {
    const response = await api.get<DecisionResponseDto>('/decisions/now', { params: { timeZoneId } });
    return response.data;
  },
  registerChoice: async (snapshotId: string, chosenCommitmentId: string): Promise<void> => {
    await api.post('/decisions/now/choice', null, { params: { snapshotId, chosenCommitmentId } });
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
  }
};