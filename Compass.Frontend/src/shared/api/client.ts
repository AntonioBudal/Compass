// src/api/client.ts
import axios, { type InternalAxiosRequestConfig, type AxiosResponse, type AxiosError } from 'axios';
import { useDevStore } from '@/shared/stores/devStore';
import { useOfflineStore } from '@/shared/stores/offlineStore';

function generateCorrelationId(): string {
  if (typeof crypto !== 'undefined' && crypto.randomUUID) {
    return crypto.randomUUID();
  }
  return 'corr-' + Math.random().toString(36).substring(2, 11) + '-' + Date.now();
}

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
    'X-User-Id': '11111111-1111-1111-1111-111111111111'
  }
});

// Interceptor de Saída
apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const corrId = generateCorrelationId();
  config.headers.set('X-Correlation-Id', corrId);
  (config as any)._metadata = { startTime: performance.now(), correlationId: corrId };
  return config;
});

// Interceptor de Chegada e Resiliência
apiClient.interceptors.response.use(
  (response: AxiosResponse) => {
    const meta = (response.config as any)._metadata || {};
    const duration = Math.round(performance.now() - (meta.startTime || performance.now()));
    
    if (import.meta.env.DEV) {
      try {
        const devStore = useDevStore();
        devStore.logHttp({
          id: meta.correlationId || generateCorrelationId(), timestamp: new Date().toISOString().slice(11, 23),
          method: response.config.method?.toUpperCase() || 'GET', url: response.config.url || '',
          status: response.status, latencyMs: duration, payload: response.data, isProblemDetails: false
        });
      } catch (e) {}
    }
    return response;
  },
  async (error: AxiosError) => {
    const config = error.config as any || {};
    const meta = config._metadata || {};
    const duration = Math.round(performance.now() - (meta.startTime || performance.now()));
    const responseData: any = error.response?.data;

    const isNetworkError = !error.response || error.code === 'ECONNABORTED' || error.message.includes('Network Error');
    const isWriteOperation = config.method && ['post', 'put', 'patch', 'delete'].includes(config.method.toLowerCase());

    if (isNetworkError && isWriteOperation) {
      try {
        const offlineStore = useOfflineStore();
        offlineStore.addToQueue({
          url: config.url || '', method: config.method || 'POST', payload: config.data ? JSON.parse(config.data) : null
        });
        return Promise.resolve({
          data: { id: 'offline-pending', status: 'PENDING', title: 'Salvo localmente' },
          status: 202, statusText: 'Accepted Offline', headers: {}, config
        });
      } catch (e) {}
    }

    const contentType = error.response?.headers['content-type'];
    const isProblem = (typeof contentType === 'string' && contentType.includes('problem+json')) ||
      (responseData && typeof responseData === 'object' && 'status' in responseData && 'title' in responseData);

    if (import.meta.env.DEV) {
      try {
        const devStore = useDevStore();
        devStore.logHttp({
          id: meta.correlationId || generateCorrelationId(), timestamp: new Date().toISOString().slice(11, 23),
          method: config.method?.toUpperCase() || 'ERR', url: config.url || '', status: error.response?.status || 0,
          latencyMs: duration, payload: responseData || { message: error.message }, isProblemDetails: !!isProblem
        });
      } catch (e) {}
    }
    return Promise.reject(error);
  }
);