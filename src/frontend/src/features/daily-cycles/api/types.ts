// Enums do Domínio
export type CycleStatus = 'NotStarted' | 'Active' | 'Closed'
export type ExecutionType = 'DeepWork' | 'Routine' | 'Break'

// DTOs de Entidade (Queries)
export interface ExecutionLogDto {
  id: string
  referenceId: string
  type: ExecutionType
  start: string // DateTimeOffset string formatada pelo C#
  end: string   // DateTimeOffset string formatada pelo C#
}

export interface DailyCycleDetailsDto {
  id: string
  date: string // DateOnly formato yyyy-MM-dd
  status: CycleStatus
  logs: ExecutionLogDto[]
}

// Request Bodies (Mutations)
export interface StartDailyCycleRequest {
  date: string
}

export interface StartDailyCycleResponse {
  dailyCycleId: string
}

export interface RecordExecutionRequest {
  referenceId: string | null
  start: string
  end: string
  type: string
}

// DTOs de Analytics (Queries)
export interface TaskAdherenceDto {
  referenceId: string
  title: string
  plannedMinutes: number
  executedMinutes: number
  intersectedMinutes: number
}

export interface DailyAdherenceReportDto {
  profileId: string
  date: string
  totalPlannedMinutes: number
  totalExecutedMinutes: number
  globalConformityPercentage: number
  tasks: TaskAdherenceDto[]
}