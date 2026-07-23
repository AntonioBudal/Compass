import { describe, it, expect, beforeEach, vi } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import { useOfflineStore } from '@/stores/offlineStore';

describe('OfflineStore — Resiliência e Fila Local-First', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    localStorage.clear();
    vi.restoreAllMocks();
  });

  it('deve inicializar com a fila vazia no boot do sistema', () => {
    const store = useOfflineStore();

    expect(store.queue.length).toBe(0);
    expect(store.queue).toEqual([]);
  });


  it('deve enfileirar uma requisição de escrita quando o sistema falhar ou perder rede', () => {
    const store = useOfflineStore();

    store.addToQueue({
      url: '/commitments',
      method: 'POST',
      payload: {
        title: 'Tarefa de Teste Offline',
        estimatedDurationMinutes: 30
      }
    });

    expect(store.queue.length).toBe(1);
    expect(store.queue[0].url).toBe('/commitments');
    expect(store.queue[0].method).toBe('POST');
    expect(store.queue[0].payload.title)
      .toBe('Tarefa de Teste Offline');

    const savedDisk = JSON.parse(
      localStorage.getItem('compass_offline_queue') || '[]'
    );

    expect(savedDisk.length).toBe(1);
    expect(savedDisk[0].id).toBeDefined();
  });
});