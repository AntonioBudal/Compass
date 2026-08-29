import { reactive } from 'vue'
import type { DayAvailability, TimeWindow } from '@/entities/schedule-profile/api/types'

export interface DayConfig {
  dayOfWeek: number
  name: string
  shortName: string
  enabled: boolean
  windows: TimeWindow[]
}

const DEFAULT_DAYS: DayConfig[] = [
  { dayOfWeek: 1, name: 'Segunda-feira', shortName: 'Seg', enabled: true, windows: [{ startTime: '09:00', endTime: '18:00' }] },
  { dayOfWeek: 2, name: 'Terça-feira', shortName: 'Ter', enabled: true, windows: [{ startTime: '09:00', endTime: '18:00' }] },
  { dayOfWeek: 3, name: 'Quarta-feira', shortName: 'Qua', enabled: true, windows: [{ startTime: '09:00', endTime: '18:00' }] },
  { dayOfWeek: 4, name: 'Quinta-feira', shortName: 'Qui', enabled: true, windows: [{ startTime: '09:00', endTime: '18:00' }] },
  { dayOfWeek: 5, name: 'Sexta-feira', shortName: 'Sex', enabled: true, windows: [{ startTime: '09:00', endTime: '18:00' }] },
  { dayOfWeek: 6, name: 'Sábado', shortName: 'Sáb', enabled: false, windows: [{ startTime: '09:00', endTime: '13:00' }] },
  { dayOfWeek: 0, name: 'Domingo', shortName: 'Dom', enabled: false, windows: [{ startTime: '09:00', endTime: '13:00' }] }
]

export function detectBrowserTimeZone(): string {
  try {
    return Intl.DateTimeFormat().resolvedOptions().timeZone || 'America/Sao_Paulo'
  } catch {
    return 'America/Sao_Paulo'
  }
}

export function createInitialOnboardingState() {
  return reactive({
    currentStep: 1,
    timeZoneId: detectBrowserTimeZone(),
    days: JSON.parse(JSON.stringify(DEFAULT_DAYS)) as DayConfig[],
    error: ''
  })
}

export function toWeeklyAvailability(days: DayConfig[]): DayAvailability[] {
  return days
    .filter((d) => d.enabled && d.windows.length > 0)
    .map((d) => ({
      dayOfWeek: d.dayOfWeek,
      windows: d.windows.map((w) => ({
        startTime: w.startTime.length === 5 ? `${w.startTime}:00` : w.startTime,
        endTime: w.endTime.length === 5 ? `${w.endTime}:00` : w.endTime
      }))
    }))
}

export function validateAvailability(days: DayConfig[]): string | null {
  const enabledDays = days.filter((d) => d.enabled)
  if (enabledDays.length === 0) {
    return 'Selecione pelo menos um dia disponível na semana.'
  }

  for (const day of enabledDays) {
    if (day.windows.length === 0) {
      return `O dia ${day.name} está habilitado, mas não possui nenhum intervalo de horário.`
    }

    for (const w of day.windows) {
      if (!w.startTime || !w.endTime) {
        return `Preencha os horários de início e término para ${day.name}.`
      }
      if (w.startTime >= w.endTime) {
        return `No dia ${day.name}, o horário de início (${w.startTime}) deve ser anterior ao término (${w.endTime}).`
      }
    }
  }

  return null
}
