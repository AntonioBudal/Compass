import { defineStore } from 'pinia';
import { ref } from 'vue';
import { CompassApi, type UserSettingsDto } from '@/services/api';
import { useToastStore } from '@/stores/toastStore';
import { useCommitmentsStore } from '@/stores/commitmentsStore';

export const useSettingsStore = defineStore('settings', () => {
  const toastStore = useToastStore();
  const commitmentsStore = useCommitmentsStore();
  const isLoading = ref(false);
  const isSubmitting = ref(false);

  const settings = ref<UserSettingsDto>({
    workDayStart: '08:00',
    workDayEnd: '18:00',
    defaultEnergy: 2,
    defaultDurationMinutes: 30
  });

  const fetchSettings = async () => {
    isLoading.value = true;
    try {
      const data = await CompassApi.getSettings();
      settings.value = data;
    } catch (e) {
      console.warn('Usando configurações padrão locais (offline/fallback).');
    } finally {
      isLoading.value = false;
    }
  };

  const saveSettings = async (newSettings: UserSettingsDto) => {
    isSubmitting.value = true;
    try {
      const updated = await CompassApi.updateSettings(newSettings);
      settings.value = updated;
      toastStore.showToast('Configurações salvas e motor recalibrado.', 'success');
    } catch (e) {
      // Fallback otimista se a rota ainda não estiver exposta no .NET
      settings.value = { ...newSettings };
      toastStore.showToast('Configurações atualizadas localmente.', 'neutral');
    } finally {
      isSubmitting.value = false;
    }
  };

  const exportData = async () => {
    try {
      const blob = await CompassApi.exportBackup();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `compass_backup_${new Date().toISOString().slice(0, 10)}.json`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
      toastStore.showToast('Backup JSON gerado com sucesso.', 'success');
    } catch (e) {
      // Dump local em memória caso o endpoint de export via stream falhe
      const localDump = JSON.stringify(commitmentsStore.items, null, 2);
      const blob = new Blob([localDump], { type: 'application/json' });
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `compass_local_dump_${Date.now()}.json`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      toastStore.showToast('Backup gerado a partir do cache local.', 'neutral');
    }
  };

  const importData = async (file: File) => {
    isSubmitting.value = true;
    try {
      await CompassApi.importBackup(file);
      await commitmentsStore.fetchAllActive();
      toastStore.showToast('Backup restaurado e sincronizado com o banco.', 'success');
    } catch (e) {
      toastStore.showToast('Falha ao importar arquivo JSON. Verifique a estrutura.', 'error');
    } finally {
      isSubmitting.value = false;
    }
  };

  const resetAllData = async () => {
    isSubmitting.value = true;
    try {
      await CompassApi.resetDatabase();
      commitmentsStore.items = [];
      toastStore.showToast('Todos os dados foram resetados.', 'urgent');
    } catch (e) {
      commitmentsStore.items = [];
      toastStore.showToast('Cache local limpo.', 'neutral');
    } finally {
      isSubmitting.value = false;
    }
  };

  return {
    settings,
    isLoading,
    isSubmitting,
    fetchSettings,
    saveSettings,
    exportData,
    importData,
    resetAllData
  };
});