<template>
  <div class="step-timezone">
    <h2 class="title">Selecione seu Fuso Horário</h2>
    <p class="subtitle">
      O fuso horário é essencial para calcular sua data civil e horários locais com precisão.
    </p>

    <div class="form-container">
      <AppInput
        v-model="searchQuery"
        label="Buscar Fuso Horário"
        placeholder="Ex.: America/Sao_Paulo, UTC, Europe/London"
        hint="Digite para filtrar a lista de fusos disponíveis"
      />

      <div class="select-wrapper">
        <label for="timezone-select" class="select-label">Fuso Horário IANA</label>
        <select
          id="timezone-select"
          v-model="selectedZone"
          class="timezone-dropdown"
          aria-required="true"
        >
          <option
            v-for="zone in filteredZones"
            :key="zone.id"
            :value="zone.id"
          >
            {{ zone.displayName }} ({{ zone.id }})
          </option>
        </select>
      </div>

      <div v-if="selectedZone" class="selected-badge">
        <span>Fuso Selecionado:</span>
        <strong>{{ selectedZone }}</strong>
      </div>
    </div>

    <div class="actions">
      <AppButton variant="ghost" @click="$emit('back')">
        Voltar
      </AppButton>
      <AppButton variant="primary" :disabled="!selectedZone" @click="handleNext">
        Avançar para Disponibilidade
      </AppButton>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import AppButton from '@/shared/ui/AppButton.vue'
import AppInput from '@/shared/ui/AppInput.vue'
import { scheduleProfileApi } from '@/entities/schedule-profile/api/scheduleProfileApi'
import type { TimeZoneItem } from '@/entities/schedule-profile/api/types'

const props = defineProps<{
  modelValue: string
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void
  (e: 'next'): void
  (e: 'back'): void
}>()

const fallbackZones: TimeZoneItem[] = [
  { id: 'America/Sao_Paulo', displayName: '(UTC-03:00) Brasília Time', baseUtcOffset: '-03:00:00' },
  { id: 'UTC', displayName: '(UTC) Coordinated Universal Time', baseUtcOffset: '00:00:00' },
  { id: 'America/New_York', displayName: '(UTC-05:00) Eastern Time', baseUtcOffset: '-05:00:00' },
  { id: 'America/Los_Angeles', displayName: '(UTC-08:00) Pacific Time', baseUtcOffset: '-08:00:00' },
  { id: 'Europe/London', displayName: '(UTC+00:00) London', baseUtcOffset: '00:00:00' },
  { id: 'Europe/Paris', displayName: '(UTC+01:00) Paris', baseUtcOffset: '01:00:00' },
  { id: 'Asia/Tokyo', displayName: '(UTC+09:00) Tokyo', baseUtcOffset: '09:00:00' }
]

const searchQuery = ref('')
const selectedZone = ref(props.modelValue || 'America/Sao_Paulo')
const timezones = ref<TimeZoneItem[]>(fallbackZones)

onMounted(async () => {
  try {
    const list = await scheduleProfileApi.getSupportedTimeZones()
    if (list && list.length > 0) {
      timezones.value = list
    }
  } catch {
    // Keep fallback list on network failure or test environment
  }
})

const filteredZones = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()
  if (!query) {
    return timezones.value
  }
  return timezones.value.filter(
    (tz) =>
      tz.id.toLowerCase().includes(query) ||
      tz.displayName.toLowerCase().includes(query)
  )
})

function handleNext() {
  emit('update:modelValue', selectedZone.value)
  emit('next')
}
</script>

<style scoped>
.step-timezone {
  display: flex;
  flex-direction: column;
  gap: var(--space-5);
  max-width: 540px;
  margin: 0 auto;
}

.title {
  font-size: var(--font-size-2xl);
  font-weight: 700;
  color: var(--color-text-primary);
  text-align: center;
}

.subtitle {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  text-align: center;
}

.form-container {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
  background-color: var(--color-bg-surface);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-5);
}

.select-wrapper {
  display: flex;
  flex-direction: column;
  gap: var(--space-1);
}

.select-label {
  font-size: var(--font-size-sm);
  font-weight: 500;
  color: var(--color-text-secondary);
}

.timezone-dropdown {
  width: 100%;
  padding: var(--space-2) var(--space-3);
  font-size: var(--font-size-sm);
  color: var(--color-text-primary);
  background-color: var(--color-bg-primary);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-md);
  font-family: inherit;
}

.timezone-dropdown:focus {
  border-color: var(--color-accent-primary);
  outline: none;
}

.selected-badge {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  padding: var(--space-2) var(--space-3);
  background-color: var(--color-bg-primary);
  border-radius: var(--radius-md);
  font-size: var(--font-size-xs);
  color: var(--color-text-secondary);
}

.selected-badge strong {
  color: var(--color-accent-primary);
}

.actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: var(--space-2);
}
</style>
