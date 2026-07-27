import { defineStore } from 'pinia';
import { ref } from 'vue';
import axios from 'axios';
import { useToastStore } from './toastStore';

// Contrato espelhando exatamente o Setting.cs e Portability do Backend .NET 10
export interface UserSettingsDto {
  defaultEnergyLevel: number;
  theme: string;
  autoPostponeEnabled: boolean;
  dailyReviewTime: string;
  preferencesJson: string;
}

const STORAGE_KEY = 'compass_settings_cache';

export const useSettingsStore = defineStore('settings', () => {
  const toastStore = useToastStore();

  // --- Estado Reativo Basal ---
  const settings = ref<UserSettingsDto>({
    defaultEnergyLevel: 2,
    theme: 'dark',
    autoPostponeEnabled: true,
    dailyReviewTime: '20:00',
    preferencesJson: '{}'
  });
  
  const isLoading = ref(false);
  const isSubmitting = ref(false);

  // --- Sincronização em Disco e Multi-Aba ---
  function saveToDisk() {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(settings.value));
    } catch (e) {
      console.warn('[SettingsStore] Falha ao persistir configurações no localStorage.', e);
    }
  }

  function loadFromDisk(): boolean {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (raw) {
        Object.assign(settings.value, JSON.parse(raw));
        return true;
      }
    } catch (e) {
      console.warn('[SettingsStore] Cache local corrompido.', e);
    }
    return false;
  }

  function listenToCrossTabSettings() {
    if (typeof window !== 'undefined') {
      window.addEventListener('storage', (event) => {
        if (event.key === STORAGE_KEY && event.newValue) {
          try {
            Object.assign(settings.value, JSON.parse(event.newValue));
            toastStore.showToast('Configurações atualizadas por outra aba.', 'neutral');
          } catch (e) {
            console.error('[SettingsStore] Erro no sync multi-aba.', e);
          }
        }
      });
    }
  }

  // --- Ações de Comunicação com a API ---
  const fetchSettings = async (force = false) => {
    if (!force && loadFromDisk()) return;
    
    isLoading.value = true;
    try {
      const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';
      const headers = { 'X-User-Id': '11111111-1111-1111-1111-111111111111' };
      
      const res = await axios.get<UserSettingsDto>(`${baseUrl}/settings`, { headers });
      if (res.status === 200 && res.data) {
        Object.assign(settings.value, res.data);
        saveToDisk();
      }
    } catch (e) {
      console.warn('[SettingsStore] API offline. Mantendo configurações em memória/disco.', e);
    } finally {
      isLoading.value = false;
    }
  };

  const updateSettings = async (newSettings: Partial<UserSettingsDto>) => {
    if (isSubmitting.value) return;
    isSubmitting.value = true;

    // Atualização otimista na UI
    const previous = { ...settings.value };
    Object.assign(settings.value, newSettings);
    saveToDisk();

    try {
      const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';
      const headers = { 
        'X-User-Id': '11111111-1111-1111-1111-111111111111',
        'Content-Type': 'application/json' 
      };

      await axios.put(`${baseUrl}/settings`, settings.value, { headers });
      toastStore.showToast('Preferências salvas com sucesso!', 'success');
    } catch (e) {
      console.error('[SettingsStore] Falha ao salvar no backend. Revertendo.', e);
      Object.assign(settings.value, previous);
      saveToDisk();
      toastStore.showToast('Erro ao sincronizar preferências com o servidor.', 'error');
    } finally {
      isSubmitting.value = false;
    }
  };

  // --- Portabilidade e Soberania de Dados (Semana 4) ---
  const exportBackup = async () => {
    try {
      const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';
      const headers = { 'X-User-Id': '11111111-1111-1111-1111-111111111111' };
      
      const res = await axios.get(`${baseUrl}/portability/export`, { headers, responseType: 'blob' });
      
      const url = window.URL.createObjectURL(new Blob([res.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `compass_export_${new Date().toISOString().slice(0, 10)}.json`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      
      toastStore.showToast('Backup exportado com sucesso!', 'success');
    } catch (e) {
      console.error('[SettingsStore] Erro ao exportar backup.', e);
      toastStore.showToast('Falha ao gerar arquivo de exportação.', 'error');
    }
  };

  const importBackup = async (bundleJson: any): Promise<boolean> => {
    isLoading.value = true;
    try {
      const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';
      const headers = { 
        'X-User-Id': '11111111-1111-1111-1111-111111111111',
        'Content-Type': 'application/json' 
      };

      const res = await axios.post(`${baseUrl}/portability/import`, bundleJson, { headers });
      if (res.status === 200) {
        toastStore.showToast('Base de dados restaurada com sucesso!', 'success');
        await fetchSettings(true); // Re-hidrata as preferências
        return true;
      }
    } catch (e: any) {
      console.error('[SettingsStore] Erro no import.', e);
      toastStore.showToast(e.response?.data?.message || 'Arquivo de backup inválido ou incompatível.', 'error');
    } finally {
      isLoading.value = false;
    }
    return false;
  };

  const resetDatabase = async () => {
    if (!confirm('ATENÇÃO: Isso apagará todo o seu histórico local e remoto. Deseja continuar?')) return;
    
    try {
      const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';
      const headers = { 'X-User-Id': '11111111-1111-1111-1111-111111111111' };
      
      await axios.delete(`${baseUrl}/portability/reset`, { headers });
      localStorage.clear();
      window.location.reload();
    } catch (e) {
      console.error('[SettingsStore] Erro ao resetar banco.', e);
      toastStore.showToast('Falha ao resetar base de dados.', 'error');
    }
  };

  // Inicializa o ouvinte multi-aba
  listenToCrossTabSettings();

  return {
    settings,
    isLoading,
    isSubmitting,
    fetchSettings,
    updateSettings,
    exportBackup,
    importBackup,
    resetDatabase
  };
});