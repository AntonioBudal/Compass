import { defineStore } from 'pinia';
import { ref } from 'vue';
import axios from 'axios';

export interface HttpLogEntry {
  id: string;
  timestamp: string;
  method: string;
  url: string;
  status: number;
  latencyMs: number;
  payload: any;
  isProblemDetails: boolean;
}

export interface DomainEventLogEntry {
  id: string;
  timestamp: string;
  eventType: string;
  entityId: string;
  message: string;
  payload: any;
}

export interface PerformanceMetricEntry {
  id?: string;
  type: 'LONG_TASK' | 'RENDER_SLOW' | 'DB_HEALTH' | 'RAM_USAGE';
  name: string;
  durationMs: number;
  timestamp: string;
  detail: string;
}

export const useDevStore = defineStore('devConsole', () => {
  const isConsoleOpen = ref(false);
  const activeTab = ref<'http' | 'pinia' | 'events' | 'metrics'>('http');
  
  const httpLogs = ref<HttpLogEntry[]>([]);
  const domainEvents = ref<DomainEventLogEntry[]>([]);
  const metrics = ref<PerformanceMetricEntry[]>([]);
  const backendHealth = ref<{ status: string; latencyMs: number; lastCheck: string } | null>(null);

  const logHttp = (entry: HttpLogEntry) => {
    httpLogs.value.unshift(entry);
    if (httpLogs.value.length > 100) httpLogs.value.pop();
  };

  const logDomainEvent = (entry: Omit<DomainEventLogEntry, 'id' | 'timestamp'>) => {
    domainEvents.value.unshift({
      ...entry,
      id: 'evt-' + Math.random().toString(36).substring(2, 9),
      timestamp: new Date().toISOString().slice(11, 23)
    });
    if (domainEvents.value.length > 100) domainEvents.value.pop();
  };

  const logMetric = (entry: PerformanceMetricEntry) => {
    metrics.value.unshift({
      ...entry,
      id: 'met-' + Math.random().toString(36).substring(2, 9)
    });
    if (metrics.value.length > 50) metrics.value.pop();
  };

  // Sondagem assíncrona da saúde do ASP.NET Core 10
  const pingBackendHealth = async () => {
    const url = (import.meta.env.VITE_API_URL || 'http://localhost:5163/api/v1').replace('/api/v1', '') + '/api/v1/health';
    const start = performance.now();
    try {
      const res = await axios.get(url, { timeout: 3000 });
      const duration = Math.round(performance.now() - start);
      backendHealth.value = {
        status: res.data.status || 'Healthy',
        latencyMs: duration,
        lastCheck: new Date().toISOString().slice(11, 19)
      };
    } catch (e: any) {
      backendHealth.value = {
        status: 'Unreachable / Degraded',
        latencyMs: Math.round(performance.now() - start),
        lastCheck: new Date().toISOString().slice(11, 19)
      };
    }
  };

  const clearAllLogs = () => {
    httpLogs.value = [];
    domainEvents.value = [];
    metrics.value = [];
  };

  const toggleConsole = () => {
    isConsoleOpen.value = !isConsoleOpen.value;
    if (isConsoleOpen.value) {
      pingBackendHealth(); // Sonda a saúde do servidor ao abrir o painel
    }
  };

  return {
    isConsoleOpen,
    activeTab,
    httpLogs,
    domainEvents,
    metrics,
    backendHealth,
    logHttp,
    logDomainEvent,
    logMetric,
    pingBackendHealth,
    clearAllLogs,
    toggleConsole
  };
});