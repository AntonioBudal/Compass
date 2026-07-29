<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue';
import { useOfflineStore } from '@/stores/offlineStore';
import { useToastStore } from '@/stores/toastStore';
import { CloudUpload } from 'lucide-vue-next';

const offlineStore = useOfflineStore();
const toastStore = useToastStore();

const handleBeforeUnload = (e: BeforeUnloadEvent) => {
  if (offlineStore.queue.length === 0) return;

  e.preventDefault();
  e.returnValue = '';
};

onMounted(() => {
  offlineStore.initNetworkListeners();
  window.addEventListener('beforeunload', handleBeforeUnload);
});

onUnmounted(() => {
  offlineStore.removeListeners();
  window.removeEventListener('beforeunload', handleBeforeUnload);
});

const handleForceSync = async () => {
  if (!offlineStore.isOnline) {
    toastStore.showIntervention({
      code: 'OFFLINE_SYNC_BLOCKED',
      title: 'Dispositivo sem conexão',
      explanation:
        'As alterações permanecem armazenadas localmente e serão sincronizadas automaticamente quando a conexão retornar.',
      severity: 'info',
      actions: [
        {
          label: 'Entendi',
          isPrimary: true,
          handler: () => {}
        }
      ]
    });

    return;
  }

  await offlineStore.processQueue();
};
</script>