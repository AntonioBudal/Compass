export interface TimeWindow {
  startTime: string // "HH:mm:ss" or "HH:mm"
  endTime: string   // "HH:mm:ss" or "HH:mm"
}

export interface DayAvailability {
  dayOfWeek: number // 0 = Sunday, 1 = Monday, ... 6 = Saturday
  windows: TimeWindow[]
}

export interface ScheduleProfile {
  id: string
  timeZoneId: string
  weeklyAvailability: DayAvailability[]
  createdAt: string
  updatedAt: string
}

export interface TimeZoneItem {
  id: string
  displayName: string
  baseUtcOffset: string
}

export interface CreateScheduleProfileRequest {
  timeZoneId: string
  weeklyAvailability: DayAvailability[]
}
