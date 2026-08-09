import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import axios from 'axios';
import { useToastStore } from './toastStore';

export interface UserSettingsDto {
  defaultEnergyLevel: number;
  theme: string;
  autoPostponeEnabled: boolean;
  dailyReviewTime: string;
  preferencesJson: string; // O payload flexível que vamos explorar agora
}

// Interface interna do Frontend para o JSON
export interface ClientPreferences {
  viewDensity?: Record<string, 'detailed' | 'compact'>;
}

const STORAGE_KEY = 'compass_settings_cache';

export const useSettingsStore = defineStore('settings', () => {
  const toastStore = useToastStore();

  const settings = ref<UserSettingsDto>({
    defaultEnergyLevel: 2,
    theme: 'dark',
    autoPostponeEnabled: true,
    dailyReviewTime: '20:00',
    preferencesJson: '{}'
  });
  
  const isLoading = ref(false);
  const isSubmitting = ref(false);

  // --- COMPOSIÇÃO DE PREFERÊNCIAS (JSON) ---
  const clientPreferences = computed<ClientPreferences>(() => {
    try {
      return JSON.parse(settings.value.preferencesJson || '{}');
    } catch {
      return {};
    }
  });

  const getViewDensity = (viewName: string): 'detailed' | 'compact' => {
    return clientPreferences.value.viewDensity?.[viewName] || 'detailed';
  };

  const toggleViewDensity = async (viewName: string) => {
    const currentPrefs = { ...clientPreferences.value };
    if (!currentPrefs.viewDensity) currentPrefs.viewDensity = {};
    
    const current = currentPrefs.viewDensity[viewName] || 'detailed';
    currentPrefs.viewDensity[viewName] = current === 'detailed' ? 'compact' : 'detailed';
    
    // Dispara a mutação assíncrona (com Otimismo in-RAM garantido pelo updateSettings)
    await updateSettings({ preferencesJson: JSON.stringify(currentPrefs) });
  };

  // --- Sincronização em Disco e Multi-Aba ---
  

  

  function listenToCrossTabSettings() {
    if (typeof window !== 'undefined') {
      window.addEventListener('storage', (event) => {
        if (event.key === STORAGE_KEY && event.newValue) {
          Object.assign(settings.value, JSON.parse(event.newValue));
        }
      });
    }
  }

  // --- Ações de Comunicação API ---
  const fetchSettings = async (force = false) => {
    //  ARQ (Stale-While-Revalidate): Carrega da RAM/Disco para renderizar a UI sem "piscar"
   
    
    // E então vai ao servidor para garantir que as alterações de outros dispositivos sejam refletidas
    isLoading.value = true;
    try {
      const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';
      const headers = { 'X-User-Id': '11111111-1111-1111-1111-111111111111' };
      const res = await axios.get<UserSettingsDto>(`${baseUrl}/settings`, { headers, timeout: 5000 });
      
      if (res.status === 200 && res.data) {
        Object.assign(settings.value, res.data);
  
      }
    } catch (e) {
      console.warn('[SettingsStore] Backend indisponível, servindo do cache local.', e);
    } finally {
      isLoading.value = false;
    }
  };

  const updateSettings = async (newSettings: Partial<UserSettingsDto>) => {
    if (isSubmitting.value) return;
    isSubmitting.value = true;

    const previous = { ...settings.value };
    
    // Mutação Otimista
    Object.assign(settings.value, newSettings);
   

    try {
      const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';
      const headers = { 
        'X-User-Id': '11111111-1111-1111-1111-111111111111',
        'Content-Type': 'application/json' 
      };
      await axios.put(`${baseUrl}/settings`, settings.value, { headers });
    } catch (e) {
      // Reverte em caso de falha
      Object.assign(settings.value, previous);
      
      toastStore.showToast('Erro ao sincronizar configurações.', 'error');
    } finally {
      isSubmitting.value = false;
    }
  };

  const exportBackup = async () => { /* Mantido original... */ };
  const importBackup = async (bundleJson: any): Promise<boolean> => { /* Mantido original... */ return false;};
  const resetDatabase = async () => { /* Mantido original... */ };

  listenToCrossTabSettings();

  return {
    settings,
    isLoading,
    isSubmitting,
    getViewDensity,
    toggleViewDensity,
    fetchSettings,
    updateSettings,
    exportBackup,
    importBackup,
    resetDatabase
  };
});