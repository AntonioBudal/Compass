<template>
  <div
    v-if="modelValue"
    class="modal-backdrop"
    role="dialog"
    aria-modal="true"
    :aria-labelledby="titleId"
    @click.self="handleBackdropClick"
    @keydown.esc="close"
  >
    <div class="modal-container" :style="{ maxWidth: maxWidth }" ref="modalRef">
      <header class="modal-header">
        <h2 :id="titleId" class="modal-title">{{ title }}</h2>
        <button
          type="button"
          class="modal-close-btn"
          aria-label="Fechar modal"
          @click="close"
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            width="16"
            height="16"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="2"
            stroke-linecap="round"
            stroke-linejoin="round"
            aria-hidden="true"
          >
            <line x1="18" y1="6" x2="6" y2="18" />
            <line x1="6" y1="6" x2="18" y2="18" />
          </svg>
        </button>
      </header>

      <div class="modal-body">
        <slot />
      </div>

      <footer v-if="$slots.footer" class="modal-footer">
        <slot name="footer" />
      </footer>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onUnmounted, watch, nextTick } from 'vue'

const props = withDefaults(
  defineProps<{
    modelValue: boolean
    title: string
    maxWidth?: string
    closeOnBackdrop?: boolean
  }>(),
  {
    maxWidth: '540px',
    closeOnBackdrop: true,
  }
)

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'close'): void
}>()

const titleId = `modal-title-${Math.random().toString(36).substring(2, 9)}`
const modalRef = ref<HTMLElement | null>(null)

function close() {
  emit('update:modelValue', false)
  emit('close')
}

function handleBackdropClick() {
  if (props.closeOnBackdrop) {
    close()
  }
}

function handleKeyDown(event: KeyboardEvent) {
  if (event.key === 'Escape' && props.modelValue) {
    close()
  }
}

watch(
  () => props.modelValue,
  (isOpen) => {
    if (isOpen) {
      document.body.style.overflow = 'hidden'
      window.addEventListener('keydown', handleKeyDown)
      nextTick(() => {
        const firstInput = modalRef.value?.querySelector<HTMLElement>(
          'input, button, select, textarea, [tabindex]:not([tabindex="-1"])'
        )
        firstInput?.focus()
      })
    } else {
      document.body.style.overflow = ''
      window.removeEventListener('keydown', handleKeyDown)
    }
  },
  { immediate: true }
)

onUnmounted(() => {
  document.body.style.overflow = ''
  window.removeEventListener('keydown', handleKeyDown)
})
</script>

<style scoped>
.modal-backdrop {
  position: fixed;
  inset: 0;
  background-color: rgba(27, 31, 36, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--space-4);
  z-index: 1000;
}

.modal-container {
  width: 100%;
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-modal);
  display: flex;
  flex-direction: column;
  max-height: calc(100vh - var(--space-8));
  overflow: hidden;
}

.modal-header {
  padding: var(--space-4) var(--space-5);
  border-bottom: 1px solid var(--color-border);
  display: flex;
  align-items: center;
  justify-content: space-between;
  background-color: var(--color-surface-subtle);
}

.modal-title {
  font-size: var(--font-size-base);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-primary);
  margin: 0;
}

.modal-close-btn {
  background: transparent;
  border: none;
  color: var(--color-text-secondary);
  cursor: pointer;
  padding: var(--space-1);
  border-radius: var(--radius-sm);
  display: flex;
  align-items: center;
  justify-content: center;
  transition: color var(--transition-fast), background-color var(--transition-fast);
}

.modal-close-btn:hover {
  color: var(--color-text-primary);
  background-color: var(--color-surface-hover);
}

.modal-body {
  padding: var(--space-5);
  overflow-y: auto;
  flex: 1;
}

.modal-footer {
  padding: var(--space-4) var(--space-5);
  border-top: 1px solid var(--color-border);
  background-color: var(--color-surface-subtle);
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: var(--space-3);
}

@media (max-width: 640px) {
  .modal-backdrop {
    padding: var(--space-2);
  }

  .modal-header,
  .modal-body,
  .modal-footer {
    padding: var(--space-3) var(--space-4);
  }
}
</style>
