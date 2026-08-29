<script setup lang="ts">
import { ref } from 'vue'
import AppButton from '@/shared/ui/AppButton.vue'
import AppInput from '@/shared/ui/AppInput.vue'

const emit = defineEmits<{
  (e: 'capture', title: string): void
}>()

defineProps<{
  isLoading?: boolean
}>()

const title = ref('')
const error = ref('')

function handleSubmit() {
  const trimmed = title.value.trim()
  if (!trimmed) {
    error.value = 'Informe um título para capturar a tarefa.'
    return
  }

  error.value = ''
  emit('capture', trimmed)
  title.value = ''
}
</script>

<template>
  <form class="quick-capture-form" @submit.prevent="handleSubmit" aria-label="Captura Rápida de Tarefas">
    <div class="input-wrapper">
      <AppInput
        v-model="title"
        placeholder="Capturar nova tarefa (ex.: Escrever documentação)..."
        :error="error"
        :disabled="isLoading"
        aria-label="Título da nova tarefa"
      />
    </div>
    <AppButton
      type="submit"
      variant="primary"
      :loading="isLoading"
      class="btn-capture"
    >
      Capturar
    </AppButton>
  </form>
</template>

<style scoped>
.quick-capture-form {
  display: flex;
  gap: var(--space-2, 8px);
  align-items: flex-start;
  width: 100%;
}

.input-wrapper {
  flex: 1;
}

.btn-capture {
  white-space: nowrap;
  min-height: 42px;
}
</style>
