import type {
  CreateScheduleProfileRequest,
  ScheduleProfile,
  TimeZoneItem
} from './types'

export const scheduleProfileApi = {
  async getSupportedTimeZones(): Promise<TimeZoneItem[]> {
    const res = await fetch('/api/calendar/timezones')
    if (!res.ok) {
      throw new Error(`Falha ao carregar fusos horários: ${res.statusText}`)
    }
    return res.json()
  },

  async getScheduleProfile(id: string): Promise<ScheduleProfile> {
    const res = await fetch(`/api/calendar/schedule-profiles/${encodeURIComponent(id)}`)
    if (!res.ok) {
      if (res.status === 404) {
        throw new Error('NOT_FOUND')
      }
      throw new Error(`Falha ao buscar perfil: ${res.statusText}`)
    }
    return res.json()
  },

  async createScheduleProfile(data: CreateScheduleProfileRequest): Promise<ScheduleProfile> {
    const res = await fetch('/api/calendar/schedule-profiles', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    })

    if (!res.ok) {
      const errorData = await res.json().catch(() => null)
      const message = errorData?.detail || errorData?.title || 'Falha ao criar o perfil de calendário.'
      throw new Error(message)
    }

    return res.json()
  }
}
